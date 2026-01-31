using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public struct CubeSpatialData
{
    public float3 Center;
    public float3 HalfExtents;
    public int Handle;
}

public partial class SpatialMapping : MonoBehaviour
{
    [SerializeField] private Vector3Int cellSize = Vector3Int.one * 9;

    private NativeArray<CubeSpatialData> _cubes;
    private NativeParallelMultiHashMap<int3, int> _grid;

    private NativeArray<byte> _inside;        // 0 / 1
    private NativeArray<byte> _seenThisQuery; // 0 / 1

    public void Build(List<Transform> cubes)
    {
        DisposeNativeData();
        int count = cubes.Count;

        _cubes = new NativeArray<CubeSpatialData>(count, Allocator.Persistent);
        _grid = new NativeParallelMultiHashMap<int3, int>(count * 2, Allocator.Persistent);
        _inside = new NativeArray<byte>(count, Allocator.Persistent);
        _seenThisQuery = new NativeArray<byte>(count, Allocator.Persistent);

        for (int i = 0; i < count; i++)
        {
            Vector3 scale = cubes[i].localScale;

            _cubes[i] = new CubeSpatialData
            {
                Center = cubes[i].position,
                HalfExtents = scale * 0.5f,
                Handle = i
            };
        }

        new BuildGridJob
        {
            Cubes = _cubes,
            Grid = _grid.AsParallelWriter(),
            CellSize = new int3(cellSize.x, cellSize.y, cellSize.z)
        }.Schedule(count, 64).Complete();
    }

    public void QueryDelta(Bounds bounds, NativeList<int> entered, NativeList<int> exited)
    {
        entered.Clear();
        exited.Clear();

        entered.Capacity = math.max(entered.Capacity, _cubes.Length);
        exited.Capacity = math.max(exited.Capacity, _cubes.Length);

        new ClearSeenJob
        {
            Seen = _seenThisQuery
        }.Schedule(_seenThisQuery.Length, 64).Complete();

        int3 minCell = WorldToCell(bounds.min, new int3(cellSize.x, cellSize.y, cellSize.z));
        int3 maxCell = WorldToCell(bounds.max, new int3(cellSize.x, cellSize.y, cellSize.z));
        int3 size = maxCell - minCell + 1;

        if (size.x <= 0 || size.y <= 0 || size.z <= 0)
            return;

        // Phase 1 — grid query (single-threaded, safe random access)
        new QueryGridJob
        {
            Grid = _grid,
            Cubes = _cubes,
            QueryMin = bounds.min,
            QueryMax = bounds.max,
            Seen = _seenThisQuery,
            MinCell = minCell,
            Size = size
        }.Run();

        // Phase 2 — delta detection (parallel, per-cube)
        new DeltaDetectionJob
        {
            Seen = _seenThisQuery,
            Inside = _inside,
            Cubes = _cubes,
            Entered = entered.AsParallelWriter(),
            Exited = exited.AsParallelWriter()
        }.Schedule(_cubes.Length, 64).Complete();
    }

    public void QueryDeltaMultipleBounds(IReadOnlyList<Bounds> boundsList, NativeList<int> entered, NativeList<int> exited)
    {
        entered.Clear();
        exited.Clear();

        entered.Capacity = math.max(entered.Capacity, _cubes.Length);
        exited.Capacity = math.max(exited.Capacity, _cubes.Length);

        new ClearSeenJob
        {
            Seen = _seenThisQuery
        }.Schedule(_seenThisQuery.Length, 64).Complete();

        int3 cellSize3 = new int3(cellSize.x, cellSize.y, cellSize.z);

        foreach (var bounds in boundsList)
        {
            int3 minCell = WorldToCell(bounds.min, cellSize3);
            int3 maxCell = WorldToCell(bounds.max, cellSize3);
            int3 size = maxCell - minCell + 1;

            if (size.x <= 0 || size.y <= 0 || size.z <= 0)
                continue;


            new QueryGridJob
            {
                Grid = _grid,
                Cubes = _cubes,
                QueryMin = bounds.min,
                QueryMax = bounds.max,
                Seen = _seenThisQuery,
                MinCell = minCell,
                Size = size
            }.Run();
        }

        new DeltaDetectionJob
        {
            Seen = _seenThisQuery, // current global
            Inside = _inside,      // previous global
            Cubes = _cubes,
            Entered = entered.AsParallelWriter(),
            Exited = exited.AsParallelWriter()
        }.Schedule(_cubes.Length, 64).Complete();
    }

    public void QueryAllInBounds(Bounds bounds,NativeList<int> results)
    {
        results.Clear();

        if (!_cubes.IsCreated || _cubes.Length == 0)
            return;

        results.Capacity = math.max(results.Capacity, _cubes.Length);

        // Clear seen
        new ClearSeenJob
        {
            Seen = _seenThisQuery
        }.Schedule(_seenThisQuery.Length, 64).Complete();

        int3 cellSize3 = new int3(cellSize.x, cellSize.y, cellSize.z);

        int3 minCell = WorldToCell(bounds.min, cellSize3);
        int3 maxCell = WorldToCell(bounds.max, cellSize3);
        int3 size = maxCell - minCell + 1;

        if (size.x <= 0 || size.y <= 0 || size.z <= 0)
            return;

        // Grid query (same as delta phase 1)
        new QueryGridJob
        {
            Grid = _grid,
            Cubes = _cubes,
            QueryMin = bounds.min,
            QueryMax = bounds.max,
            Seen = _seenThisQuery,
            MinCell = minCell,
            Size = size
        }.Run();

        // Collect results
        for (int i = 0; i < _seenThisQuery.Length; i++)
        {
            if (_seenThisQuery[i] == 1)
                results.Add(_cubes[i].Handle);
        }
    }

    [BurstCompile]
    private struct BuildGridJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<CubeSpatialData> Cubes;
        public NativeParallelMultiHashMap<int3, int>.ParallelWriter Grid;
        public int3 CellSize;

        public void Execute(int index)
        {
            int3 cell = WorldToCell(Cubes[index].Center, CellSize);
            Grid.Add(cell, index);
        }
    }

    [BurstCompile]
    private struct ClearSeenJob : IJobParallelFor
    {
        public NativeArray<byte> Seen;

        public void Execute(int index)
        {
            Seen[index] = 0;
        }
    }

    // Phase 1 — SAFE grid traversal
    [BurstCompile]
    private struct QueryGridJob : IJob
    {
        [ReadOnly] public NativeParallelMultiHashMap<int3, int> Grid;
        [ReadOnly] public NativeArray<CubeSpatialData> Cubes;

        public float3 QueryMin;
        public float3 QueryMax;

        public NativeArray<byte> Seen;

        public int3 MinCell;
        public int3 Size;

        public void Execute()
        {
            for (int x = 0; x < Size.x; x++)
                for (int y = 0; y < Size.y; y++)
                    for (int z = 0; z < Size.z; z++)
                    {
                        int3 cell = MinCell + new int3(x, y, z);

                        if (!Grid.TryGetFirstValue(cell, out int cubeIndex, out var it))
                            continue;

                        do
                        {
                            CubeSpatialData cube = Cubes[cubeIndex];

                            if (AABBOverlap(
                                    cube.Center,
                                    cube.HalfExtents,
                                    QueryMin,
                                    QueryMax))
                            {
                                Seen[cubeIndex] = 1;
                            }

                        } while (Grid.TryGetNextValue(out cubeIndex, ref it));
                    }
        }
    }

    // Phase 2 — SAFE delta detection
    [BurstCompile]
    private struct DeltaDetectionJob : IJobParallelFor
    {
        public NativeArray<byte> Seen;
        public NativeArray<byte> Inside;

        [ReadOnly] public NativeArray<CubeSpatialData> Cubes;

        public NativeList<int>.ParallelWriter Entered;
        public NativeList<int>.ParallelWriter Exited;

        public void Execute(int index)
        {
            byte wasInside = Inside[index];
            byte isInside = Seen[index];

            if (wasInside == 0 && isInside == 1)
                Entered.AddNoResize(Cubes[index].Handle);

            if (wasInside == 1 && isInside == 0)
                Exited.AddNoResize(Cubes[index].Handle);

            Inside[index] = isInside;
        }
    }

    public static int3 WorldToCell(float3 pos, int3 cellSize)
    {
        return new int3(
            (int)math.floor(pos.x / cellSize.x),
            (int)math.floor(pos.y / cellSize.y),
            (int)math.floor(pos.z / cellSize.z)
        );
    }

    private static bool AABBOverlap(float3 center, float3 half, float3 min, float3 max)
    {
        float3 cubeMin = center - half;
        float3 cubeMax = center + half;

        return cubeMin.x <= max.x && cubeMax.x >= min.x &&
               cubeMin.y <= max.y && cubeMax.y >= min.y &&
               cubeMin.z <= max.z && cubeMax.z >= min.z;
    }

    private void DisposeNativeData()
    {
        if (_cubes.IsCreated) _cubes.Dispose();
        if (_grid.IsCreated) _grid.Dispose();
        if (_inside.IsCreated) _inside.Dispose();
        if (_seenThisQuery.IsCreated) _seenThisQuery.Dispose();
    }

    private void OnDestroy()
    {
        DisposeNativeData();
    }
}

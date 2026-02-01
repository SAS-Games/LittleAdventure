using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public struct SpatialAABB
{
    public float3 Center;
    public float3 HalfExtents;
    public int Handle;
}

public partial class SpatialMapping : MonoBehaviour
{
    [SerializeField] private Vector3Int cellSize = Vector3Int.one * 9;

    private NativeArray<SpatialAABB> _spatialDatas;
    private NativeParallelMultiHashMap<int3, int> _grid;

    private NativeArray<byte> _inside; // 0 / 1
    private NativeArray<byte> _seenThisQuery; // 0 / 1
    private Transform[] _transformLookup;
    public int Capacity => _spatialDatas.Length;

    public void Build(List<Transform> cubes)
    {
        DisposeNativeData();
        int count = cubes.Count;

        _spatialDatas = new NativeArray<SpatialAABB>(count, Allocator.Persistent);
        _grid = new NativeParallelMultiHashMap<int3, int>(count * 2, Allocator.Persistent);
        _inside = new NativeArray<byte>(count, Allocator.Persistent);
        _seenThisQuery = new NativeArray<byte>(count, Allocator.Persistent);
        _transformLookup = new Transform[cubes.Count];

        for (int i = 0; i < count; i++)
        {
            _transformLookup[i] = cubes[i];
            _spatialDatas[i] = new SpatialAABB
            {
                Center = cubes[i].position,
                HalfExtents = cubes[i].localScale * 0.5f,
                Handle = i
            };
        }

        new BuildGridJob
        {
            Cubes = _spatialDatas,
            Grid = _grid.AsParallelWriter(),
            CellSize = new int3(cellSize.x, cellSize.y, cellSize.z)
        }.Schedule(count, 64).Complete();
    }

    public void QueryDelta(Bounds bounds, NativeList<int> entered, NativeList<int> exited)
    {
        entered.Clear();
        exited.Clear();

        entered.Capacity = math.max(entered.Capacity, _spatialDatas.Length);
        exited.Capacity = math.max(exited.Capacity, _spatialDatas.Length);

        unsafe
        {
            UnsafeUtility.MemClear(NativeArrayUnsafeUtility.GetUnsafePtr(_seenThisQuery), _seenThisQuery.Length * sizeof(byte));
        }


        int3 minCell = WorldToCell(bounds.min, new int3(cellSize.x, cellSize.y, cellSize.z));
        int3 maxCell = WorldToCell(bounds.max, new int3(cellSize.x, cellSize.y, cellSize.z));
        int3 size = maxCell - minCell + 1;

        if (size.x <= 0 || size.y <= 0 || size.z <= 0)
            return;

        // Phase 1 � grid query (single-threaded, safe random access)
        new QueryGridJob
        {
            Grid = _grid,
            Cubes = _spatialDatas,
            QueryMin = bounds.min,
            QueryMax = bounds.max,
            Seen = _seenThisQuery,
            MinCell = minCell,
            Size = size
        }.Run();

        // Phase 2 � delta detection (parallel, per-cube)
        new DeltaDetectionJob
        {
            Seen = _seenThisQuery,
            Inside = _inside,
            Cubes = _spatialDatas,
            Entered = entered.AsParallelWriter(),
            Exited = exited.AsParallelWriter()
        }.Schedule(_spatialDatas.Length, 64).Complete();
    }

    public void QueryDeltaMultipleBounds(IReadOnlyList<Bounds> boundsList, NativeList<int> entered, NativeList<int> exited)
    {
        entered.Clear();
        exited.Clear();

        entered.Capacity = math.max(entered.Capacity, _spatialDatas.Length);
        exited.Capacity = math.max(exited.Capacity, _spatialDatas.Length);

        unsafe
        {
            UnsafeUtility.MemClear(NativeArrayUnsafeUtility.GetUnsafePtr(_seenThisQuery), _seenThisQuery.Length * sizeof(byte));
        }
        
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
                Cubes = _spatialDatas,
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
            Inside = _inside, // previous global
            Cubes = _spatialDatas,
            Entered = entered.AsParallelWriter(),
            Exited = exited.AsParallelWriter()
        }.Schedule(_spatialDatas.Length, 64).Complete();
    }

    public int QueryAllInBoundsNonAlloc(Bounds bounds, NativeArray<byte> seen, Transform[] results)
    {
        if (!_spatialDatas.IsCreated || results == null || results.Length == 0)
            return 0;
        
        if (seen.Length < _spatialDatas.Length)
        {
            Debug.LogError($"Seen buffer too small. Required {_spatialDatas.Length}, got {seen.Length}");
            return 0;
        }

        unsafe
        {
            UnsafeUtility.MemClear(NativeArrayUnsafeUtility.GetUnsafePtr(seen), seen.Length * sizeof(byte));
        }
        
        int3 cellSize3 = new int3(cellSize.x, cellSize.y, cellSize.z);
        int3 minCell = WorldToCell(bounds.min, cellSize3);
        int3 maxCell = WorldToCell(bounds.max, cellSize3);
        int3 size = maxCell - minCell + 1;

        if (size.x <= 0 || size.y <= 0 || size.z <= 0)
            return 0;

        new QueryGridJob
        {
            Grid = _grid,
            Cubes = _spatialDatas,
            QueryMin = bounds.min,
            QueryMax = bounds.max,
            Seen = seen,
            MinCell = minCell,
            Size = size
        }.Run();

        int count = 0;

        for (int i = 0; i < seen.Length && count < results.Length; i++)
        {
            if (seen[i] == 1)
                results[count++] = _transformLookup[i];
        }

        return count;
    }

    [BurstCompile]
    private struct BuildGridJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<SpatialAABB> Cubes;
        public NativeParallelMultiHashMap<int3, int>.ParallelWriter Grid;
        public int3 CellSize;

        public void Execute(int index)
        {
            int3 cell = WorldToCell(Cubes[index].Center, CellSize);
            Grid.Add(cell, index);
        }
    }

    // Phase 1 � SAFE grid traversal
    [BurstCompile]
    private struct QueryGridJob : IJob
    {
        [ReadOnly] public NativeParallelMultiHashMap<int3, int> Grid;
        [ReadOnly] public NativeArray<SpatialAABB> Cubes;

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
                    SpatialAABB cube = Cubes[cubeIndex];

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

    // Phase 2 � SAFE delta detection
    [BurstCompile]
    private struct DeltaDetectionJob : IJobParallelFor
    {
        public NativeArray<byte> Seen;
        public NativeArray<byte> Inside;

        [ReadOnly] public NativeArray<SpatialAABB> Cubes;

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

    private static int3 WorldToCell(float3 pos, int3 cellSize)
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
        if (_spatialDatas.IsCreated) _spatialDatas.Dispose();
        if (_grid.IsCreated) _grid.Dispose();
        if (_inside.IsCreated) _inside.Dispose();
        if (_seenThisQuery.IsCreated) _seenThisQuery.Dispose();
    }

    private void OnDestroy()
    {
        DisposeNativeData();
    }
}
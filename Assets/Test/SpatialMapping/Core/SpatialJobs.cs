using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

public static class SpatialJobs
{
    [BurstCompile]
    internal struct BuildGridJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<SpatialAABB> Cubes;
        public NativeParallelMultiHashMap<int3, int>.ParallelWriter Grid;
        public int3 CellSize;

        public void Execute(int index)
        {
            int3 cell = SpatialUtils.WorldToCell(Cubes[index].Center, CellSize);
            Grid.Add(cell, index);
        }
    }

    [BurstCompile]
    internal struct QueryGridJob : IJob
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

                    if (SpatialUtils.AABBOverlap(cube.Center, cube.HalfExtents, QueryMin, QueryMax))
                    {
                        Seen[cubeIndex] = 1;
                    }
                } while (Grid.TryGetNextValue(out cubeIndex, ref it));
            }
        }
    }

    [BurstCompile]
    internal struct DeltaDetectionJob : IJobParallelFor
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
    
    [BurstCompile]
    public struct QueryBoundsCollectStampJob : IJob
    {
        [ReadOnly] public NativeParallelMultiHashMap<int3, int> Grid;
        [ReadOnly] public NativeArray<SpatialAABB> Cubes;

        public float3 QueryMin;
        public float3 QueryMax;

        public int3 MinCell;
        public int3 Size;

        public NativeArray<int> InsideStamp;
        public int Frame;

        public NativeList<int> Results;

        public void Execute()
        {
            for (int x = 0; x < Size.x; x++)
            for (int y = 0; y < Size.y; y++)
            for (int z = 0; z < Size.z; z++)
            {
                int3 cell = MinCell + new int3(x, y, z);

                if (!Grid.TryGetFirstValue(cell, out int index, out var it))
                    continue;

                do
                {
                    // already recorded this frame
                    if (InsideStamp[index] == Frame)
                        continue;

                    SpatialAABB cube = Cubes[index];

                    if (SpatialUtils.AABBOverlap(
                            cube.Center,
                            cube.HalfExtents,
                            QueryMin,
                            QueryMax))
                    {
                        InsideStamp[index] = Frame;
                        Results.Add(index);
                    }

                } while (Grid.TryGetNextValue(out index, ref it));
            }
        }
    }
    
    [BurstCompile]
    public struct QueryBoundsCollectJob : IJob
    {
        [ReadOnly] public NativeParallelMultiHashMap<int3, int> Grid;
        [ReadOnly] public NativeArray<SpatialAABB> Cubes;

        public float3 QueryMin;
        public float3 QueryMax;

        public int3 MinCell;
        public int3 Size;

        public NativeList<int> Results;
        public int MaxResults;

        public void Execute()
        {
            for (int x = 0; x < Size.x; x++)
            for (int y = 0; y < Size.y; y++)
            for (int z = 0; z < Size.z; z++)
            {
                int3 cell = MinCell + new int3(x, y, z);

                if (!Grid.TryGetFirstValue(cell, out int index, out var it))
                    continue;

                do
                {
                    SpatialAABB cube = Cubes[index];

                    if (SpatialUtils.AABBOverlap(cube.Center, cube.HalfExtents, QueryMin, QueryMax))
                    {
                        Results.Add(index);

                        if (Results.Length >= MaxResults)
                            return;
                    }

                } while (Grid.TryGetNextValue(out index, ref it));
            }
        }
    }
}

internal static class SpatialUtils
{
    internal static int3 WorldToCell(float3 pos, int3 cellSize)
    {
        return new int3(
            (int)math.floor(pos.x / cellSize.x),
            (int)math.floor(pos.y / cellSize.y),
            (int)math.floor(pos.z / cellSize.z)
        );
    }
    internal static bool AABBOverlap(float3 center, float3 half, float3 min, float3 max)
    {
        float3 cubeMin = center - half;
        float3 cubeMax = center + half;

        return cubeMin.x <= max.x && cubeMax.x >= min.x &&
               cubeMin.y <= max.y && cubeMax.y >= min.y &&
               cubeMin.z <= max.z && cubeMax.z >= min.z;
    }
}
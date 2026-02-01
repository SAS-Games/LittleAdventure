using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public sealed class SpatialDatabase : IDisposable
{
    public NativeArray<SpatialAABB> Data;
    public NativeParallelMultiHashMap<int3, int> Grid;
    public Transform[] TransformLookup;
    public readonly int3 CellSize;

    public int Capacity => Data.IsCreated ? Data.Length : 0;

    public SpatialDatabase(List<Transform> cubes, int3 cellSize)
    {
        int count = cubes.Count;
        CellSize = cellSize;

        Data = new NativeArray<SpatialAABB>(count, Allocator.Persistent);
        Grid = new NativeParallelMultiHashMap<int3, int>(count * 2, Allocator.Persistent);
        TransformLookup = new Transform[count];

        for (int i = 0; i < count; i++)
        {
            TransformLookup[i] = cubes[i];

            Data[i] = new SpatialAABB
            {
                Center = cubes[i].position,
                HalfExtents = cubes[i].localScale * 0.5f,
                Handle = i
            };
        }

        new SpatialJobs.BuildGridJob
        {
            Cubes = Data,
            Grid = Grid.AsParallelWriter(),
            CellSize = cellSize
        }.Schedule(count, 64).Complete();
    }

    public void Dispose()
    {
        if (Data.IsCreated) Data.Dispose();
        if (Grid.IsCreated) Grid.Dispose();
    }
}
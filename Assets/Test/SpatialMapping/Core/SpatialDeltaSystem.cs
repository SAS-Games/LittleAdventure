using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public sealed class SpatialDeltaSystem : IDisposable
{
    private readonly SpatialDatabase _database;
    private readonly int3 _cellSize;

    // stamp array
    private NativeArray<int> _insideStamp;

    // lists for iteration only
    private NativeList<int> _insidePrev;
    private NativeList<int> _insideNow;

    private int _frame;

    public SpatialDeltaSystem(SpatialDatabase database, int maxExpectedInside = 1024)
    {
        _database = database;
        _cellSize = database.CellSize;

        _insideStamp = new NativeArray<int>(database.Capacity, Allocator.Persistent);

        _insidePrev = new NativeList<int>(maxExpectedInside, Allocator.Persistent);

        _insideNow = new NativeList<int>(maxExpectedInside, Allocator.Persistent);

        _frame = 1;
    }

    public void UpdateDelta(IReadOnlyList<Bounds> boundsList, NativeList<int> entered, NativeList<int> exited)
    {
        entered.Clear();
        exited.Clear();

        _insideNow.Clear();

        int prevFrame = _frame;
        int currFrame = ++_frame;
        
        foreach (var bounds in boundsList)
        {
            int3 minCell = SpatialUtils.WorldToCell(bounds.min, _cellSize);
            int3 maxCell = SpatialUtils.WorldToCell(bounds.max, _cellSize);
            int3 size = maxCell - minCell + 1;

            if (size.x <= 0 || size.y <= 0 || size.z <= 0)
                continue;

            var job = new SpatialJobs.QueryBoundsCollectStampJob
            {
                Grid = _database.Grid,
                Cubes = _database.Data,

                QueryMin = bounds.min,
                QueryMax = bounds.max,

                MinCell = minCell,
                Size = size,

                InsideStamp = _insideStamp,
                Frame = currFrame,

                Results = _insideNow
            };

            job.Run();
        }
        
        for (int i = 0; i < _insideNow.Length; i++)
        {
            int index = _insideNow[i];

            if (_insideStamp[index] != prevFrame)
                entered.Add(index);
        }

        for (int i = 0; i < _insidePrev.Length; i++)
        {
            int index = _insidePrev[i];

            if (_insideStamp[index] != currFrame)
                exited.Add(index);
        }
        
        (_insidePrev, _insideNow) = (_insideNow, _insidePrev);
    }

    public void Dispose()
    {
        if (_insideStamp.IsCreated) _insideStamp.Dispose();
        if (_insidePrev.IsCreated) _insidePrev.Dispose();
        if (_insideNow.IsCreated)  _insideNow.Dispose();
    }
}

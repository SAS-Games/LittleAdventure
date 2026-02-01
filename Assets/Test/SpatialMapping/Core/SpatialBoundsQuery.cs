using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public sealed class SpatialBoundsQuery : IDisposable
{
    private readonly SpatialDatabase _database;
    private readonly int3 _cellSize;
    private NativeList<int> _resultIndices;

    public SpatialBoundsQuery(SpatialDatabase database, int maxResults)
    {
        _database = database;
        _cellSize = database.CellSize;
        _resultIndices = new NativeList<int>(maxResults, Allocator.Persistent);
    }

    public int QueryAllInBoundsNonAlloc(Bounds bounds, Transform[] results)
    {
        if (results.Length == 0)
            return 0;

        if (results.Length > _resultIndices.Capacity)
        {
            Debug.LogError("Result buffer exceeds SpatialBoundsQuery capacity.");
            return 0;
        }

        _resultIndices.Clear();

        int3 minCell = SpatialUtils.WorldToCell(bounds.min, _cellSize);
        int3 maxCell = SpatialUtils.WorldToCell(bounds.max, _cellSize);
        int3 size = maxCell - minCell + 1;

        if (size.x <= 0 || size.y <= 0 || size.z <= 0)
            return 0;

        var job = new SpatialJobs.QueryBoundsCollectJob
        {
            Grid = _database.Grid,
            Cubes = _database.Data,

            QueryMin = bounds.min,
            QueryMax = bounds.max,

            MinCell = minCell,
            Size = size,

            Results = _resultIndices,
            MaxResults = results.Length
        };

        job.Run(); // fast + Burst

        int count = math.min(_resultIndices.Length, results.Length);

        for (int i = 0; i < count; i++)
        {
            results[i] = _database.TransformLookup[_resultIndices[i]];
        }

        return count;
    }

    public void Dispose()
    {
        if (_resultIndices.IsCreated)
            _resultIndices.Dispose();
    }
}
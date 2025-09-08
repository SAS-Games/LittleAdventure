using System.Collections.Generic;
using UnityEngine;

public class GridStreamingStrategy : ISceneStreamingStrategy
{
    private readonly Dictionary<Vector3Int, List<SceneBoundsManager.SceneRef>> _grid 
        = new Dictionary<Vector3Int, List<SceneBoundsManager.SceneRef>>();
    private float _cellSize;

    public void BuildIndex(List<SceneBoundsManager.SceneRef> scenes, float cellSize = 100f)
    {
        _grid.Clear();
        _cellSize = cellSize;

        foreach (var scene in scenes)
        {
            var minCell = WorldToCell(scene.cachedBounds.min);
            var maxCell = WorldToCell(scene.cachedBounds.max);

            for (int x = minCell.x; x <= maxCell.x; x++)
            for (int y = minCell.y; y <= maxCell.y; y++)
            for (int z = minCell.z; z <= maxCell.z; z++)
            {
                var cell = new Vector3Int(x, y, z);
                if (!_grid.TryGetValue(cell, out var list))
                {
                    list = new List<SceneBoundsManager.SceneRef>();
                    _grid[cell] = list;
                }
                list.Add(scene);
            }
        }
    }

    public List<SceneBoundsManager.SceneRef> GetNearbyScenes(Bounds queryBounds)
    {
        var result = new HashSet<SceneBoundsManager.SceneRef>(); // ensures no duplicates

        var minCell = WorldToCell(queryBounds.min);
        var maxCell = WorldToCell(queryBounds.max);

        for (int x = minCell.x; x <= maxCell.x; x++)
        for (int y = minCell.y; y <= maxCell.y; y++)
        for (int z = minCell.z; z <= maxCell.z; z++)
        {
            var cell = new Vector3Int(x, y, z);
            if (_grid.TryGetValue(cell, out var list))
            {
                foreach (var scene in list)
                {
                    if (scene.cachedBounds.Intersects(queryBounds))
                        result.Add(scene);
                }
            }
        }

        return new List<SceneBoundsManager.SceneRef>(result);
    }

    private Vector3Int WorldToCell(Vector3 pos)
    {
        return new Vector3Int(
            Mathf.FloorToInt(pos.x / _cellSize),
            Mathf.FloorToInt(pos.y / _cellSize),
            Mathf.FloorToInt(pos.z / _cellSize)
        );
    }
}

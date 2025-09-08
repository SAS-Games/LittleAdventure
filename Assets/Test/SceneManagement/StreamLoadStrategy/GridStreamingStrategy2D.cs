using System.Collections.Generic;
using UnityEngine;

public class GridStreamingStrategy2D : ISceneStreamingStrategy
{
    private readonly Dictionary<Vector2Int, List<SceneBoundsManager.SceneRef>> _grid = new();
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
            {
                var cell = new Vector2Int(x, y);
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
        var result = new HashSet<SceneBoundsManager.SceneRef>();

        var minCell = WorldToCell(queryBounds.min);
        var maxCell = WorldToCell(queryBounds.max);

        for (int x = minCell.x; x <= maxCell.x; x++)
        for (int y = minCell.y; y <= maxCell.y; y++)
        {
            var cell = new Vector2Int(x, y);
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

    private Vector2Int WorldToCell(Vector3 pos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(pos.x / _cellSize),
            Mathf.FloorToInt(pos.y / _cellSize)
        );
    }
}
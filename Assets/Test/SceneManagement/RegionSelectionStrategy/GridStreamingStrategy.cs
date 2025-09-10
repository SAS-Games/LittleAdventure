using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Streaming/Strategies/GridStrategy")]
public class GridRegionSelection : RegionSelectionStrategySO
{
    private readonly Dictionary<Vector3Int, List<RegionManager.Region>> _grid = new();
    [SerializeField] private float m_CellSize = 100;

    public override void Initialize(List<RegionManager.Region> sceneRefs)
    {
        _grid.Clear();
        foreach (var scene in sceneRefs)
        {
            var minCell = WorldToCell(scene.CachedBounds.min);
            var maxCell = WorldToCell(scene.CachedBounds.max);

            for (int x = minCell.x; x <= maxCell.x; x++)
            for (int y = minCell.y; y <= maxCell.y; y++)
            for (int z = minCell.z; z <= maxCell.z; z++)
            {
                var cell = new Vector3Int(x, y, z);
                if (!_grid.TryGetValue(cell, out var list))
                {
                    list = new List<RegionManager.Region>();
                    _grid[cell] = list;
                }

                list.Add(scene);
            }
        }
    }

    public override List<RegionManager.Region> GetNearbyRegions(Bounds queryBounds)
    {
        var result = new HashSet<RegionManager.Region>(); // ensures no duplicates

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
                    if (scene.CachedBounds.Intersects(queryBounds))
                        result.Add(scene);
                }
            }
        }

        return new List<RegionManager.Region>(result);
    }

    private Vector3Int WorldToCell(Vector3 pos)
    {
        return new Vector3Int(
            Mathf.FloorToInt(pos.x / m_CellSize),
            Mathf.FloorToInt(pos.y / m_CellSize),
            Mathf.FloorToInt(pos.z / m_CellSize)
        );
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace LevelStreaming
{
    [CreateAssetMenu(menuName = "Streaming/RegionSelection/GridStrategy2D")]
    public class GridRegionSelection2D : RegionSelectionStrategySO
    {
        private readonly Dictionary<Vector2Int, List<RegionManager.Region>> _grid = new();
        [SerializeField] private float m_CellSize = 100;

        public override void Initialize(List<RegionManager.Region> regionRefs)
        {
            _grid.Clear();

            foreach (var scene in regionRefs)
            {
                var minCell = WorldToCell(scene.CachedBounds.min);
                var maxCell = WorldToCell(scene.CachedBounds.max);

                for (int x = minCell.x; x <= maxCell.x; x++)
                for (int y = minCell.y; y <= maxCell.y; y++)
                {
                    var cell = new Vector2Int(x, y);
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
            var result = new HashSet<RegionManager.Region>();

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
                        if (scene.CachedBounds.Intersects(queryBounds))
                            result.Add(scene);
                    }
                }
            }

            return new List<RegionManager.Region>(result);
        }

        private Vector2Int WorldToCell(Vector3 pos)
        {
            return new Vector2Int(
                Mathf.FloorToInt(pos.x / m_CellSize),
                Mathf.FloorToInt(pos.y / m_CellSize)
            );
        }
    }
}
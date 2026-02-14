using System.Collections.Generic;
using UnityEngine;

namespace LevelStreaming
{
    [CreateAssetMenu(menuName = "Streaming/RegionSelection/GridRegionSelection2D")]
    public class GridRegionSelection2D : RegionSelectionStrategySO
    {
        public Dictionary<Vector2Int, List<RegionManager.Region>> Grid { get; private set; } = new();
        [field: SerializeField] public Vector2Int CellSize { get; private set; } = new(50, 50);

        public override void Initialize(IReadOnlyList<RegionManager.Region> regionRefs)
        {
            Grid.Clear();

            foreach (var region in regionRefs)
            {
                var minCell = WorldToCell(region.CachedBounds.min);
                var maxCell = WorldToCell(region.CachedBounds.max);

                for (int x = minCell.x; x <= maxCell.x; x++)
                for (int y = minCell.y; y <= maxCell.y; y++)
                {
                    var cell = new Vector2Int(x, y);
                    if (!Grid.TryGetValue(cell, out var list))
                    {
                        list = new List<RegionManager.Region>();
                        Grid[cell] = list;
                    }

                    list.Add(region);
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
                if (Grid.TryGetValue(cell, out var list))
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
                Mathf.FloorToInt(pos.x / (float)CellSize.x),
                Mathf.FloorToInt(pos.y / (float)CellSize.y)
            );
        }
    }
}

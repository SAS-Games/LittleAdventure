using System.Collections.Generic;
using System;
using UnityEngine;

namespace LevelStreaming
{
    [CreateAssetMenu(menuName = "Streaming/RegionSelection/GridRegionSelection2D")]
    public class GridRegionSelection2D : RegionSelectionStrategySO
    {
        private const long MaxCellsPerOperation = 100000;
        public Dictionary<Vector2Int, List<RegionManager.Region>> Grid { get; private set; } = new();
        private List<RegionManager.Region> _allRegions = new();
        private List<RegionManager.Region> _overflowRegions = new();
        [field: SerializeField] public Vector2Int CellSize { get; private set; } = new(50, 50);

        public override void Initialize(IReadOnlyList<RegionManager.Region> regionRefs)
        {
            CellSize = new Vector2Int(Sanitize(CellSize.x), Sanitize(CellSize.y));
            Grid = new Dictionary<Vector2Int, List<RegionManager.Region>>();
            _allRegions = new List<RegionManager.Region>();
            _overflowRegions = new List<RegionManager.Region>();

            foreach (var region in regionRefs)
            {
                if (region == null)
                    continue;

                _allRegions.Add(region);
                var minCell = WorldToCell(region.CachedBounds.min);
                var maxCell = WorldToCell(region.CachedBounds.max);
                if (CellCount(minCell, maxCell) > MaxCellsPerOperation)
                {
                    _overflowRegions.Add(region);
                    continue;
                }

                for (long x = minCell.x; x <= maxCell.x; x++)
                for (long y = minCell.y; y <= maxCell.y; y++)
                {
                    var cell = new Vector2Int((int)x, (int)y);
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

            if (CellCount(minCell, maxCell) > MaxCellsPerOperation)
            {
                foreach (var region in _allRegions)
                {
                    if (region.CachedBounds.Intersects(queryBounds))
                        result.Add(region);
                }

                return new List<RegionManager.Region>(result);
            }

            for (long x = minCell.x; x <= maxCell.x; x++)
            for (long y = minCell.y; y <= maxCell.y; y++)
            {
                var cell = new Vector2Int((int)x, (int)y);
                if (Grid.TryGetValue(cell, out var list))
                {
                    foreach (var scene in list)
                    {
                        if (scene.CachedBounds.Intersects(queryBounds))
                            result.Add(scene);
                    }
                }
            }

            foreach (var region in _overflowRegions)
            {
                if (region.CachedBounds.Intersects(queryBounds))
                    result.Add(region);
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

        private static int Sanitize(int value)
        {
            if (value == int.MinValue)
                return int.MaxValue;
            return Math.Max(1, Math.Abs(value));
        }

        private static long CellCount(Vector2Int min, Vector2Int max)
        {
            long x = (long)max.x - min.x + 1L;
            long y = (long)max.y - min.y + 1L;
            if (x <= 0L || y <= 0L || x > MaxCellsPerOperation || y > MaxCellsPerOperation)
                return MaxCellsPerOperation + 1L;

            return x * y;
        }
    }
}

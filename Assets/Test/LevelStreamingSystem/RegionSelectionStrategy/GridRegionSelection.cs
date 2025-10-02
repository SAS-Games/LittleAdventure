using System.Collections.Generic;
using UnityEngine;

namespace LevelStreaming
{
    [CreateAssetMenu(menuName = "Streaming/RegionSelection/GridRegionSelection")]
    public class GridRegionSelection : RegionSelectionStrategySO
    {
        public Dictionary<Vector3Int, List<RegionManager.Region>> Grid { get; private set; } = new();
        [field: SerializeField] public Vector3Int CellSize { get; private set; } = Vector3Int.one * 100;

        public override void Initialize(List<RegionManager.Region> regionRefs)
        {
            Grid.Clear();
            foreach (var scene in regionRefs)
            {
                var minCell = WorldToCell(scene.CachedBounds.min);
                var maxCell = WorldToCell(scene.CachedBounds.max);

                for (int x = minCell.x; x <= maxCell.x; x++)
                for (int y = minCell.y; y <= maxCell.y; y++)
                for (int z = minCell.z; z <= maxCell.z; z++)
                {
                    var cell = new Vector3Int(x, y, z);
                    if (!Grid.TryGetValue(cell, out var list))
                    {
                        list = new List<RegionManager.Region>();
                        Grid[cell] = list;
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
            for (int z = minCell.z; z <= maxCell.z; z++)
            {
                var cell = new Vector3Int(x, y, z);
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

        private Vector3Int WorldToCell(Vector3 pos)
        {
            return new Vector3Int(
                Mathf.FloorToInt(pos.x / CellSize.x),
                Mathf.FloorToInt(pos.y / CellSize.y),
                Mathf.FloorToInt(pos.z / CellSize.z)
            );
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace LevelStreaming
{
    [CreateAssetMenu(menuName = "Streaming/RegionSelection/GridRegionSelection")]
    public class GridRegionSelection : RegionSelectionStrategySO, ISerializationCallbackReceiver
    {
        private const long MaxCellsPerOperation = 100000;
        public Dictionary<Vector3Int, List<RegionManager.Region>> Grid { get; private set; } = new();
        private List<RegionManager.Region> _allRegions = new();
        private List<RegionManager.Region> _overflowRegions = new();
        [field: SerializeField] public Vector3Int CellSize { get; private set; } = Vector3Int.one * 100;
        [FormerlySerializedAs("m_CellSize")]
        [SerializeField, HideInInspector] private float m_LegacyCellSize = -1f;
        [SerializeField, HideInInspector] private int m_SerializationVersion;

        public override void Initialize(IReadOnlyList<RegionManager.Region> regionRefs)
        {
            CellSize = SanitizeCellSize(CellSize);
            // ScriptableObject clones must never retain/share an asset's nonserialized
            // runtime collections with another manager.
            Grid = new Dictionary<Vector3Int, List<RegionManager.Region>>();
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
                for (long z = minCell.z; z <= maxCell.z; z++)
                {
                    var cell = new Vector3Int((int)x, (int)y, (int)z);
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
            for (long z = minCell.z; z <= maxCell.z; z++)
            {
                var cell = new Vector3Int((int)x, (int)y, (int)z);
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

        private Vector3Int WorldToCell(Vector3 pos)
        {
            return new Vector3Int(
                Mathf.FloorToInt(pos.x / CellSize.x),
                Mathf.FloorToInt(pos.y / CellSize.y),
                Mathf.FloorToInt(pos.z / CellSize.z)
            );
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            if (m_SerializationVersion >= 1)
                return;

            if (m_LegacyCellSize > 0f)
            {
                int size = Math.Max(1, (int)Math.Round(m_LegacyCellSize));
                CellSize = Vector3Int.one * size;
                m_LegacyCellSize = -1f;
            }

            CellSize = SanitizeCellSize(CellSize);
            m_SerializationVersion = 1;
        }

        private static Vector3Int SanitizeCellSize(Vector3Int size)
        {
            return new Vector3Int(
                SanitizeComponent(size.x),
                SanitizeComponent(size.y),
                SanitizeComponent(size.z));
        }

        private static int SanitizeComponent(int value)
        {
            return value == int.MinValue ? int.MaxValue : Math.Max(1, Math.Abs(value));
        }

        private static long CellCount(Vector3Int min, Vector3Int max)
        {
            long x = (long)max.x - min.x + 1L;
            long y = (long)max.y - min.y + 1L;
            long z = (long)max.z - min.z + 1L;
            if (x <= 0L || y <= 0L || z <= 0L || x > MaxCellsPerOperation ||
                y > MaxCellsPerOperation || z > MaxCellsPerOperation)
                return MaxCellsPerOperation + 1L;

            long xy = x * y;
            if (xy > MaxCellsPerOperation)
                return MaxCellsPerOperation + 1L;
            return xy * z;
        }
    }
}

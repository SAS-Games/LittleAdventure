using System.Collections.Generic;
using UnityEngine;

namespace LevelStreaming
{
    [CreateAssetMenu(menuName = "Streaming/RegionSelection/QuadtreeRegionSelection")]
    public class QuadtreeRegionSelection : RegionSelectionStrategySO
    {
        private QuadtreeNode _root;
        [SerializeField] private int m_MaxDepth = 6;
        [SerializeField] private int m_MaxCapacity = 4;

        public override void Initialize(IReadOnlyList<RegionManager.Region> regionRefs)
        {
            m_MaxDepth = Mathf.Max(0, m_MaxDepth);
            m_MaxCapacity = Mathf.Max(1, m_MaxCapacity);
            bool hasRegion = false;
            Bounds worldBounds = default;
            foreach (var region in regionRefs)
            {
                if (region == null)
                    continue;

                if (!hasRegion)
                {
                    worldBounds = region.CachedBounds;
                    hasRegion = true;
                }
                else
                    worldBounds.Encapsulate(region.CachedBounds);
            }

            if (!hasRegion)
            {
                _root = null;
                return;
            }

            _root = new QuadtreeNode(worldBounds, 0, m_MaxDepth, m_MaxCapacity);

            foreach (var region in regionRefs)
            {
                if (region != null)
                    _root.Insert(region);
            }
        }

        public override List<RegionManager.Region> GetNearbyRegions(Bounds queryBounds)
        {
            if (_root == null)
                return new List<RegionManager.Region>();

            var results = new HashSet<RegionManager.Region>();
            _root.Query(queryBounds, results);
            return new List<RegionManager.Region>(results);
        }

        public QuadtreeNode Root => _root;
        public int MaxDepth => m_MaxDepth;
        public int MaxCapacity => m_MaxCapacity;
    }
}

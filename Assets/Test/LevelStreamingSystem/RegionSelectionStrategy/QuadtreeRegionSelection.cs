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

        public override void Initialize(List<RegionManager.Region> regionRefs)
        {
            Bounds worldBounds = new Bounds(Vector3.zero, Vector3.one);
            foreach (var region in regionRefs)
                worldBounds.Encapsulate(region.CachedBounds);

            _root = new QuadtreeNode(worldBounds, 0, m_MaxDepth, m_MaxCapacity);

            foreach (var region in regionRefs)
                _root.Insert(region);
        }

        public override List<RegionManager.Region> GetNearbyRegions(Bounds queryBounds)
        {
            var results = new List<RegionManager.Region>();
            _root.Query(queryBounds, results);
            return results;
        }

        public QuadtreeNode Root => _root;
        public int MaxDepth => m_MaxDepth;
    }
}
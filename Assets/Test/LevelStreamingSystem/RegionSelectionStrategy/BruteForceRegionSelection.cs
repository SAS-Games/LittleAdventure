using System.Collections.Generic;
using UnityEngine;

namespace LevelStreaming
{
    [CreateAssetMenu(menuName = "Streaming/RegionSelection/BruteForceStrategy")]
    public class BruteForceRegionSelection : RegionSelectionStrategySO
    {
        private IReadOnlyList<RegionManager.Region> _sceneRefs;

        public override void Initialize(IReadOnlyList<RegionManager.Region> regionRefs)
        {
            _sceneRefs = regionRefs;
        }

        public override List<RegionManager.Region> GetNearbyRegions(IStreamingVolume queryVolume)
        {
            var result = new List<RegionManager.Region>();

            foreach (var scene in _sceneRefs)
            {
                if (scene.Intersects(queryVolume))
                    result.Add(scene);
            }

            return result;
        }
    }
}

using UnityEngine;

namespace LevelStreaming
{
    [CreateAssetMenu(menuName = "Streaming/UnloadStrategies/Bounds Intersection")]
    public class BoundsIntersectionUnloadStrategy : UnloadStrategy
    {
        public override bool ShouldUnload(IStreamingVolume unloadVolume, RegionManager regionManager, RegionManager.Region region)
        {
            return !region.Intersects(unloadVolume);
        }
    }
}

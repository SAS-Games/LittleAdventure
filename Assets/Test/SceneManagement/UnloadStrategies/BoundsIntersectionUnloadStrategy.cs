using UnityEngine;

[CreateAssetMenu(menuName = "Streaming/UnloadStrategies/Bounds Intersection")]
public class BoundsIntersectionUnloadStrategy : UnloadStrategy
{
    public override bool ShouldUnload(Bounds unloadBounds, RegionManager.Region region)
    {
        return !unloadBounds.Intersects(region.CachedBounds);
    }
}
using UnityEngine;

namespace LevelStreaming
{
    [CreateAssetMenu(menuName = "Streaming/UnloadStrategies/PortalAwareBounds")]
    public class PortalAwareBoundsUnloadStrategy : UnloadStrategy
    {
        public override bool ShouldUnload(Bounds unloadBounds, RegionManager regionManager, RegionManager.Region region)
        {
            // 1. If region itself is still inside unload bounds → keep it
            if (unloadBounds.Intersects(region.CachedBounds))
                return false;

            // 2. If any *loaded region* has a portal pointing to this region,
            //    and that portal is inside unload bounds → keep it
            foreach (var other in regionManager.loadedRegions)
            {
                if (other == region)
                    continue;

                for (int i = 0; i < other.Portals.Count; i++)
                {
                    var portal = other.Portals[i];
                    if (portal.TargetRegionName == region.RegionName)
                    {
                        var portalBounds = other.CachedWorldPortalBounds[i];
                        if (unloadBounds.Intersects(portalBounds))
                        {
                            // This region is protected by a nearby portal
                            return false;
                        }
                    }
                }
            }

            // 3. Otherwise → safe to unload
            return true;
        }
    }
}
using System;
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
                if (other == null || other == region || other.Portals == null)
                    continue;

                int portalCount = Mathf.Min(other.Portals.Count, other.CachedWorldPortalBounds.Count);
                for (int i = 0; i < portalCount; i++)
                {
                    var portal = other.Portals[i];
                    if (portal != null && string.Equals(
                            portal.TargetRegionName,
                            region.RegionName,
                            StringComparison.Ordinal))
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

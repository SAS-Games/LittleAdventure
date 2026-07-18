using UnityEngine;

namespace LevelStreaming
{
    [CreateAssetMenu(menuName = "Streaming/UnloadStrategies/Composite")]
    public class CompositeUnloadStrategy : UnloadStrategy
    {
        public enum CombinationMode
        {
            AND,
            OR
        }

        [SerializeField] private CombinationMode mode = CombinationMode.AND;
        [SerializeField] private UnloadStrategy[] strategies;

        public override bool ShouldUnload(Bounds unloadBounds, RegionManager regionManager, RegionManager.Region region)
        {
            if (strategies == null || strategies.Length == 0)
                return false;

            switch (mode)
            {
                case CombinationMode.AND:
                    bool hasAndStrategy = false;
                    foreach (var strategy in strategies)
                    {
                        if (strategy == null) continue;
                        hasAndStrategy = true;
                        if (!strategy.ShouldUnload(unloadBounds, regionManager, region))
                            return false; // fail fast
                    }

                    return hasAndStrategy;

                case CombinationMode.OR:
                    foreach (var strategy in strategies)
                    {
                        if (strategy == null) continue;
                        if (strategy.ShouldUnload(unloadBounds, regionManager, region))
                            return true; // succeed fast
                    }

                    return false;

                default:
                    return false;
            }
        }
    }
}

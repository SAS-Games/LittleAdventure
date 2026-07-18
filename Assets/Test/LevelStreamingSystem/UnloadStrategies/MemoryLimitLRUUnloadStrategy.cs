using UnityEngine;
using UnityEngine.Profiling;

namespace LevelStreaming
{
    [CreateAssetMenu(menuName = "Streaming/UnloadStrategies/MemoryLimitLRU")]
    public class MemoryLimitLRUUnloadStrategy : UnloadStrategy
    {
        [SerializeField] private long m_MemoryLimitMB = 1024;

        public override bool ShouldUnload(Bounds unloadBounds, RegionManager regionManager, RegionManager.Region region)
        {
            long currentMemory = Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024);
            if (currentMemory <= m_MemoryLimitMB)
                return false;

            var lru = FindLeastRecentlyUsed(regionManager);
            return lru != null && lru == region;
        }

        private RegionManager.Region FindLeastRecentlyUsed(RegionManager regionManager)
        {
            RegionManager.Region lru = null;
            float oldestTime = float.MaxValue;

            foreach (var r in regionManager.loadedRegions)
            {
                if (regionManager.IsRegionDesired(r))
                    continue;

                if (regionManager.TryGetMeta(r, out var meta) &&
                    meta.State == RegionManager.RegionStreamingState.Loaded &&
                    meta.LoadedTime < oldestTime)
                {
                    oldestTime = meta.LoadedTime;
                    lru = r;
                }
            }

            return lru;
        }
    }
}

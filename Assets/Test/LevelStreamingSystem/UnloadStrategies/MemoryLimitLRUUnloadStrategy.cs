using UnityEngine;
using UnityEngine.Profiling;

namespace LevelStreaming
{
    [CreateAssetMenu(menuName = "Streaming/UnloadStrategies/MemoryLimitLRU")]
    public class MemoryLimitLRUUnloadStrategy : UnloadStrategy
    {
        [SerializeField] private long m_MemoryLimitMB = 1024;

        private static RegionManager.Region _cachedLRU;
        private static int _lastFrameChecked = -1;

        public override bool ShouldUnload(Bounds unloadBounds, RegionManager regionManager, RegionManager.Region region)
        {
            long currentMemory = Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024);
            if (currentMemory <= m_MemoryLimitMB)
                return false;

            if (_lastFrameChecked != Time.frameCount)
            {
                _lastFrameChecked = Time.frameCount;
                _cachedLRU = FindLeastRecentlyUsed(regionManager);
            }

            return _cachedLRU != null && _cachedLRU == region;
        }

        private RegionManager.Region FindLeastRecentlyUsed(RegionManager regionManager)
        {
            RegionManager.Region lru = null;
            float oldestTime = float.MaxValue;

            foreach (var r in regionManager.loadedRegions)
            {
                if (r.LoadedTime < oldestTime)
                {
                    oldestTime = r.LoadedTime;
                    lru = r;
                }
            }

            return lru;
        }
    }
}
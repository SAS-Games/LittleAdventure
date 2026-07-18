using UnityEngine;
using UnityEngine.Serialization;

namespace LevelStreaming
{
    [CreateAssetMenu(menuName = "Streaming/UnloadStrategies/TimeElapsed")]
    public class TimeElapsedStrategy : UnloadStrategy
    {
        [FormerlySerializedAs("timeToUnload")]
        [SerializeField, Min(0f)] private float m_TimeToUnload = 10;

        public override bool ShouldUnload(Bounds unloadBounds, RegionManager regionManager, RegionManager.Region region)
        {
            if (regionManager.TryGetMeta(region, out var meta))
                return Time.time - meta.LoadedTime > m_TimeToUnload;
            return false;
        }
    }
}

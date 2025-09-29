using UnityEngine;

namespace LevelStreaming
{
    [CreateAssetMenu(menuName = "Streaming/UnloadStrategies/TimeAfterUndesired")]
    public class TimeAfterUndesiredUnloadStrategy : UnloadStrategy
    {
        [SerializeField] private float m_TimeToUnload = 10f;

        public override bool ShouldUnload(Bounds unloadBounds, RegionManager regionManager, RegionManager.Region region)
        {
            if (!regionManager.TryGetMeta(region, out var meta))
                return false;
            
            float timeSinceDesired = Time.time - meta.LastTimeDesired;
            return timeSinceDesired > m_TimeToUnload;
        }
    }
}
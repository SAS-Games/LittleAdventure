using UnityEngine;

namespace LevelStreaming
{
    [CreateAssetMenu(menuName = "Streaming/UnloadStrategies/Never")]
    public class NeverUnloadStrategy : UnloadStrategy
    {
        public override bool ShouldUnload(Bounds unloadBounds, RegionManager.Region region)
        {
            return false;
        }
    }
}
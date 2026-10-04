using UnityEngine;

namespace LevelStreaming
{
    [CreateAssetMenu(menuName = "Streaming/UnloadStrategies/Never")]
    public class NeverUnloadStrategy : UnloadStrategy
    {
        public override bool ShouldUnload(IStreamingVolume unloadVolume, RegionManager regionManager,  RegionManager.Region region)
        {
            return false;
        }
    }
}

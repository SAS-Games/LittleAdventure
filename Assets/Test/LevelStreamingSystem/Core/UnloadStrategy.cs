using UnityEngine;

namespace LevelStreaming
{
    public abstract class UnloadStrategy : ScriptableObject
    {
        public abstract bool ShouldUnload(IStreamingVolume unloadVolume, RegionManager regionManager, RegionManager.Region region);
        public bool ShouldUnload(Bounds unloadBounds, RegionManager manager, RegionManager.Region region) =>
            ShouldUnload(new BoxStreamingVolume(unloadBounds), manager, region);
    }
}

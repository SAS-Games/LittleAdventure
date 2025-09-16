using UnityEngine;

namespace LevelStreaming
{
    public abstract class UnloadStrategy : ScriptableObject
    {
        public abstract bool ShouldUnload(Bounds unloadBounds, RegionManager.Region region);
    }
}
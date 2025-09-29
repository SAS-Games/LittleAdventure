using System;

namespace LevelStreaming
{
    public abstract class RegionStreamingLoader : RuntimeScriptableObject<RegionStreamingLoader>,
        IStreamingLoader<RegionManager.Region>
    {
        protected RegionStreamingController _regionStreamingController;
        protected RegionManager _regionManager;
        public abstract void Load(RegionManager.Region region, Action<RegionManager.Region> onLoaded);
        public abstract void Unload(RegionManager.Region region, Action<RegionManager.Region> onUnloaded);
        public abstract bool IsLoading(RegionManager.Region region);
        public abstract bool IsLoaded(RegionManager.Region region);

        public virtual void Initialize(RegionStreamingController regionStreamingController, RegionManager regionManager)
        {
            _regionStreamingController = regionStreamingController;
            _regionManager = regionManager;
        }
    }
}
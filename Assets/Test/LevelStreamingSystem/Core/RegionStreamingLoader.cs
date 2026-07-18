using System.Threading.Tasks;

namespace LevelStreaming
{
    public abstract class RegionStreamingLoader : RuntimeScriptableObject<RegionStreamingLoader>,
        IStreamingLoader<RegionManager.Region>
    {
        protected RegionStreamingController _regionStreamingController;
        protected RegionManager _regionManager;
        public abstract Task LoadAsync(RegionManager.Region region);
        public abstract Task UnloadAsync(RegionManager.Region region);
        public abstract bool IsLoading(RegionManager.Region region);
        public abstract bool IsLoaded(RegionManager.Region region);

        /// <summary>
        /// Returns false when a region needs an optional backend that is not installed.
        /// </summary>
        public virtual bool Supports(RegionManager.RegionType type) => true;

        public virtual void Initialize(RegionStreamingController regionStreamingController, RegionManager regionManager)
        {
            _regionStreamingController = regionStreamingController;
            _regionManager = regionManager;
        }
    }
}

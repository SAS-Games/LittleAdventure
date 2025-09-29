using System;
using UnityEngine;

namespace LevelStreaming
{
    [CreateAssetMenu(menuName = "Streaming/StreamingLoader/DefaultRegionStreamingLoader")]
    public class DefaultRegionStreamingLoader : RegionStreamingLoader
    {
        private IStreamingLoader<RegionManager.Region> _sceneLoader;
        private IStreamingLoader<RegionManager.Region> _prefabLoader;

        public override void Initialize(RegionStreamingController regionStreamingController, RegionManager regionManager)
        {
            base.Initialize(regionStreamingController, regionManager);
            _sceneLoader = new SceneStreamingLoader(_regionManager);
            _prefabLoader = new PrefabStreamingLoader(_regionManager);
        }

        public override void Load(RegionManager.Region region, Action<RegionManager.Region> onLoaded) =>
            GetLoader(region).Load(region, onLoaded);

        public override void Unload(RegionManager.Region region, Action<RegionManager.Region> onUnloaded) =>
            GetLoader(region).Unload(region, onUnloaded);

        public override bool IsLoading(RegionManager.Region region) =>
            GetLoader(region).IsLoading(region);

        public override bool IsLoaded(RegionManager.Region region) =>
            GetLoader(region).IsLoaded(region);

        private IStreamingLoader<RegionManager.Region> GetLoader(RegionManager.Region region) =>
            region.Type == RegionManager.RegionType.Scene ? _sceneLoader : _prefabLoader;
    }
}
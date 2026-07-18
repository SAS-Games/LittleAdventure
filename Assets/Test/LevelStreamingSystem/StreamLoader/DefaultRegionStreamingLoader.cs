using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace LevelStreaming
{
    [CreateAssetMenu(menuName = "Streaming/StreamingLoader/DefaultRegionStreamingLoader")]
    public class DefaultRegionStreamingLoader : RegionStreamingLoader
    {
        private readonly Dictionary<RegionManager.RegionType, IStreamingLoader<RegionManager.Region>> _loaders = new();

        public override void Initialize(RegionStreamingController regionStreamingController, RegionManager regionManager)
        {
            base.Initialize(regionStreamingController, regionManager);
            _loaders.Clear();
            _loaders.Add(RegionManager.RegionType.Scene, new SceneStreamingLoader(_regionManager));

            TryAddOptionalLoader(RegionManager.RegionType.Prefab);
            TryAddOptionalLoader(RegionManager.RegionType.AddressableScene);
        }

        public override Task LoadAsync(RegionManager.Region region) =>
            GetLoader(region.Type).LoadAsync(region);

        public override Task UnloadAsync(RegionManager.Region region)
        {
            RegionManager.RegionType loadedType =
                _regionManager.GetOrCreateMeta(region).LoadedType ?? region.Type;
            return GetLoader(loadedType).UnloadAsync(region);
        }

        public override bool IsLoading(RegionManager.Region region) =>
            _regionManager.IsRegionLoading(region);

        public override bool IsLoaded(RegionManager.Region region) =>
            _regionManager.IsRegionLoaded(region);

        public override bool Supports(RegionManager.RegionType type) => _loaders.ContainsKey(type);

        private void TryAddOptionalLoader(RegionManager.RegionType type)
        {
            if (RegionStreamingBackendRegistry.TryCreate(type, _regionManager, out var loader))
                _loaders[type] = loader;
        }

        private IStreamingLoader<RegionManager.Region> GetLoader(RegionManager.RegionType type)
        {
            if (_loaders.TryGetValue(type, out var loader))
                return loader;

            throw new NotSupportedException(
                $"No streaming backend is available for region type '{type}'. " +
                "Install and enable the matching optional integration, or change the region type.");
        }
    }
}

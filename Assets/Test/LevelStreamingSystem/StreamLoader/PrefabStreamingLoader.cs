using System;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LevelStreaming
{
    public class PrefabStreamingLoader : IStreamingLoader<RegionManager.Region>
    {
        private readonly RegionManager _regionManager;

        public PrefabStreamingLoader(RegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void Load(RegionManager.Region region,
            Action<RegionManager.Region> onLoaded)
        {
            var meta = _regionManager.GetOrCreateMeta(region);

            if (meta.IsLoading || meta.IsLoaded)
                return;

            meta.IsLoading = true;
            _ = LoadPrefabAsync(region, meta, onLoaded);
        }

        private async Task LoadPrefabAsync(RegionManager.Region region, RegionManager.RegionMetaData meta, Action<RegionManager.Region> onLoaded)
        {
            string key = region.PrefabRef.RuntimeKey.ToString();

            try
            {
                GameObject prefab = await _regionManager.Registry.Acquire<GameObject>(key,
                        async () =>
                        {
                            var handle =
                                region.PrefabRef.LoadAssetAsync<GameObject>();
                            return await handle.Task;
                        },
                        async () =>
                        {
                            region.PrefabRef.ReleaseAsset();
                            await Task.CompletedTask;
                        });

                var instance = Object.Instantiate(prefab);
                instance.name = $"{region.RegionName}_Instance";
                instance.transform.position = region.CachedBounds.center;

                meta.Instance = instance;

                await UnityAsync.NextFrame();

                meta.IsLoaded = true;
                onLoaded?.Invoke(region);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load prefab for {region.RegionName}: {e}");
            }
            finally
            {
                meta.IsLoading = false;
            }
        }

        public void Unload(RegionManager.Region region, Action<RegionManager.Region> onUnloaded)
        {
            if (_regionManager.TryGetMeta(region, out var meta) && meta.Instance != null)
            {
                Object.Destroy(meta.Instance);
                meta.Instance = null;
            }

            _ = _regionManager.Registry.Release(region.PrefabRef.RuntimeKey.ToString());

            onUnloaded?.Invoke(region);
        }

        public bool IsLoading(RegionManager.Region region) => _regionManager.IsRegionLoading(region);

        public bool IsLoaded(RegionManager.Region region) => _regionManager.IsRegionLoaded(region);
    }
}
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace LevelStreaming
{
    public class SceneStreamingLoader : IStreamingLoader<RegionManager.Region>
    {
        private RegionManager _regionManager;

        public SceneStreamingLoader(RegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void Load(RegionManager.Region region, Action<RegionManager.Region> onLoaded)
        {
            var meta = _regionManager.GetOrCreateMeta(region);
            if (meta.IsLoading || meta.IsLoaded)
                return;
            
            meta.IsLoading = true;
            _ = LoadSceneAsync(region, meta, onLoaded);
        }

        private async Task LoadSceneAsync(RegionManager.Region region, RegionManager.RegionMetaData meta, Action<RegionManager.Region> onLoaded)
        {
            string key = region.SceneRef.ScenePath;

            try
            {
                await _regionManager.Registry.Acquire(
                    key,
                    async () =>
                    {
                        var scene = SceneManager.GetSceneByPath(key);
                        if (!scene.isLoaded)
                            await SceneManager
                                .LoadSceneAsync(key, LoadSceneMode.Additive)
                                .ToTask();
                    },
                    async () =>
                    {
                        await SceneManager.UnloadSceneAsync(key).ToTask();
                    });

              await UnityAsync.NextFrame();

                meta.IsLoaded = true;
                onLoaded?.Invoke(region);
            }
            finally
            {
                meta.IsLoading = false;
            }
        }

        public void Unload(RegionManager.Region region, Action<RegionManager.Region> onUnloaded)
        {
            if (!IsLoaded(region))
                return;

            _ = UnloadSceneAsync(region, onUnloaded);
        }

        private async Task UnloadSceneAsync(RegionManager.Region region, Action<RegionManager.Region> onUnloaded)
        {
            try
            {
                await _regionManager.Registry.Release(region.SceneRef.ScenePath);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }

            onUnloaded?.Invoke(region);
        }

        public bool IsLoading(RegionManager.Region region) => _regionManager.IsRegionLoading(region);
        public bool IsLoaded(RegionManager.Region region) => _regionManager.IsRegionLoaded(region);
    }
}
using System;
using System.Threading.Tasks;
using UnityEngine;
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
           
            var scene = SceneManager.GetSceneByPath(region.SceneRef.ScenePath);
            if (scene.isLoaded)
            {
                onLoaded?.Invoke(region);
                return;
            }

            meta.IsLoading = true;
            _ = LoadSceneAsync(region, onLoaded);
        }

        private async Task LoadSceneAsync(RegionManager.Region region, Action<RegionManager.Region> onLoaded)
        {
            var meta = _regionManager.GetOrCreateMeta(region);
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(region.SceneRef.ScenePath, LoadSceneMode.Additive);
            asyncLoad.allowSceneActivation = true;

            try
            {
                await asyncLoad.ToTask();
                onLoaded?.Invoke(region);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load scene {region.RegionName}: {e}");
            }
            finally
            {
                meta.IsLoading = false;
            }
        }

        public void Unload(RegionManager.Region region, Action<RegionManager.Region> onUnloaded)
        {
            if ( !IsLoaded(region))
                return;

            _ = UnloadSceneAsync(region, onUnloaded);
        }

        private async Task UnloadSceneAsync(RegionManager.Region region, Action<RegionManager.Region> onUnloaded)
        {
            var meta = _regionManager.GetOrCreateMeta(region);
            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(region.SceneRef.ScenePath);
            if (asyncUnload != null)
            {
                try
                {
                    await asyncUnload.ToTask();
                    onUnloaded?.Invoke(region);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to unload scene {region.RegionName}: {e}");
                }
            }

            meta.IsLoading = false;
        }

        public bool IsLoading(RegionManager.Region region) => _regionManager.IsRegionLoading(region);
        public bool IsLoaded(RegionManager.Region region) => _regionManager.IsRegionLoaded(region);
    }
}
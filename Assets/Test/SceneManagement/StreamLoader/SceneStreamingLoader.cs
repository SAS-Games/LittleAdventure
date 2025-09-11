using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneStreamingLoader : IStreamingLoader<RegionManager.Region>
{
    public void Load(RegionManager.Region region, Action<RegionManager.Region> onLoaded)
    {
        if (region.Type != RegionManager.RegionType.Scene || 
            region.IsLoading || 
            region.IsLoaded)
        {
            return;
        }

        region.IsLoading = true;
        _ = LoadSceneAsync(region, onLoaded);
    }

    private async Task LoadSceneAsync(RegionManager.Region region, Action<RegionManager.Region> onLoaded)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(region.SceneRef.ScenePath, LoadSceneMode.Additive);
        asyncLoad.allowSceneActivation = true;

        try
        {
            await asyncLoad.ToTask();
            region.IsLoaded = true;
            onLoaded?.Invoke(region);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load scene {region.RegionName}: {e}");
        }
        finally
        {
            region.IsLoading = false;
        }
    }

    public void Unload(RegionManager.Region region, Action<RegionManager.Region> onUnloaded)
    {
        if (region.Type != RegionManager.RegionType.Scene || !region.IsLoaded) 
            return;

        _ = UnloadSceneAsync(region, onUnloaded);
    }

    private async Task UnloadSceneAsync(RegionManager.Region region, Action<RegionManager.Region> onUnloaded)
    {
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(region.SceneRef.ScenePath);
        if (asyncUnload != null)
        {
            try
            {
                await asyncUnload.ToTask();
                region.IsLoaded = false;
                onUnloaded?.Invoke(region);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to unload scene {region.RegionName}: {e}");
            }
        }
        region.IsLoading = false;
    }

    public bool IsLoading(RegionManager.Region region) => region.IsLoading;
    public bool IsLoaded(RegionManager.Region region) => region.IsLoaded;
}

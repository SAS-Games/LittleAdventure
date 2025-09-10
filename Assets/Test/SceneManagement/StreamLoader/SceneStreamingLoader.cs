using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneStreamingLoader : IStreamingLoader<RegionManager.Region>
{
    private readonly HashSet<RegionManager.Region> _loadingRegions = new();
    private readonly HashSet<RegionManager.Region> _loadedRegions = new();

    public void Load(RegionManager.Region region, Action<RegionManager.Region> onLoaded)
    {
        if (region.Type != RegionManager.RegionType.Scene || 
            _loadingRegions.Contains(region) || 
            _loadedRegions.Contains(region))
        {
            return;
        }

        _loadingRegions.Add(region);
        _ = LoadSceneAsync(region, onLoaded);
    }

    private async Task LoadSceneAsync(RegionManager.Region region, Action<RegionManager.Region> onLoaded)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(region.SceneRef.ScenePath, LoadSceneMode.Additive);
        asyncLoad.allowSceneActivation = true;

        try
        {
            await asyncLoad.ToTask();
            _loadedRegions.Add(region);
            onLoaded?.Invoke(region);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load scene {region.RegionName}: {e}");
        }
        finally
        {
            _loadingRegions.Remove(region);
        }
    }

    public void Unload(RegionManager.Region region, Action<RegionManager.Region> onUnloaded)
    {
        if (region.Type != RegionManager.RegionType.Scene || !_loadedRegions.Contains(region)) 
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
                _loadedRegions.Remove(region);
                onUnloaded?.Invoke(region);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to unload scene {region.RegionName}: {e}");
            }
        }
    }

    public bool IsLoading(RegionManager.Region region) => _loadingRegions.Contains(region);

    public bool IsLoaded(RegionManager.Region region) => _loadedRegions.Contains(region);
}

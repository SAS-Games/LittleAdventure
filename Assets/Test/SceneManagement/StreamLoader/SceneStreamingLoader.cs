using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneStreamingLoader : IStreamingLoader<RegionManager.Region>
{
    private readonly HashSet<RegionManager.Region> _loadingRegions = new();

    public void Load(RegionManager.Region region, Action<RegionManager.Region> onLoaded)
    {
        if (region.regionType != RegionManager.RegionType.Scene || _loadingRegions.Contains(region))
            return;

        _loadingRegions.Add(region);
        _ = LoadSceneAsync(region, onLoaded);
    }

    private async Task LoadSceneAsync(RegionManager.Region region, Action<RegionManager.Region> onLoaded)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(region.sceneRef.ScenePath, LoadSceneMode.Additive);
        asyncLoad.allowSceneActivation = true;
        await asyncLoad.ToTask();

        _loadingRegions.Remove(region);
        onLoaded?.Invoke(region);
    }

    public void Unload(RegionManager.Region region, Action<RegionManager.Region> onUnloaded)
    {
        if (region.regionType != RegionManager.RegionType.Scene) return;
        _ = UnloadSceneAsync(region, onUnloaded);
    }

    private async Task UnloadSceneAsync(RegionManager.Region region, Action<RegionManager.Region> onUnloaded)
    {
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(region.sceneRef.ScenePath);
        if (asyncUnload != null)
            await asyncUnload.ToTask();

        onUnloaded?.Invoke(region);
    }

    public bool IsLoading(RegionManager.Region region) => _loadingRegions.Contains(region);
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

public class PrefabStreamingLoader : IStreamingLoader<RegionManager.Region>
{
    private readonly HashSet<RegionManager.Region> _loadingRegions = new();
    private readonly Dictionary<RegionManager.Region, GameObject> _loadedInstances = new();

    public void Load(RegionManager.Region region, Action<RegionManager.Region> onLoaded)
    {
        if (region.regionType != RegionManager.RegionType.Prefab || _loadingRegions.Contains(region)) 
            return;

        _loadingRegions.Add(region);
        _ = LoadPrefabAsync(region, onLoaded);
    }

    private async Task LoadPrefabAsync(RegionManager.Region region, Action<RegionManager.Region> onLoaded)
    {
        var handle = region.prefabAddress.LoadAssetAsync<GameObject>();
        try
        {
            GameObject prefab = await handle.Task;
            GameObject instance = Object.Instantiate(prefab);
            _loadedInstances[region] = instance;
            onLoaded?.Invoke(region);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load {region.RegionName}: {e}");
        }
        finally
        {
            _loadingRegions.Remove(region);
        }
    }

    public void Unload(RegionManager.Region region, Action<RegionManager.Region> onUnloaded)
    {
        if (_loadedInstances.TryGetValue(region, out var instance))
        {
            Object.Destroy(instance);
            _loadedInstances.Remove(region);
        }

        region.prefabAddress?.ReleaseAsset();
        onUnloaded?.Invoke(region);
    }

    public bool IsLoading(RegionManager.Region region) => _loadingRegions.Contains(region);
}
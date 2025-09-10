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
        if (region.Type != RegionManager.RegionType.Prefab || 
            _loadingRegions.Contains(region) || 
            _loadedInstances.ContainsKey(region))
        {
            return;
        }

        _loadingRegions.Add(region);
        _ = LoadPrefabAsync(region, onLoaded);
    }

    private async Task LoadPrefabAsync(RegionManager.Region region, Action<RegionManager.Region> onLoaded)
    {
        var handle = region.PrefabRef.LoadAssetAsync<GameObject>();
        try
        {
            GameObject prefab = await handle.Task;
            if (prefab == null)
            {
                Debug.LogError($"Prefab reference is null for {region.RegionName}");
                return;
            }

            GameObject instance = Object.Instantiate(prefab);
            instance.name = $"{region.RegionName}_Instance";
            _loadedInstances[region] = instance;

            onLoaded?.Invoke(region);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load prefab for {region.RegionName}: {e}");
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

        if (region.PrefabRef != null)
            region.PrefabRef.ReleaseAsset();

        onUnloaded?.Invoke(region);
    }

    public bool IsLoading(RegionManager.Region region) => _loadingRegions.Contains(region);

    public bool IsLoaded(RegionManager.Region region) => _loadedInstances.ContainsKey(region);
}

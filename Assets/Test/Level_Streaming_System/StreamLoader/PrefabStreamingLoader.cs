using System;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

public class PrefabStreamingLoader : IStreamingLoader<RegionManager.Region>
{
    public void Load(RegionManager.Region region, Action<RegionManager.Region> onLoaded)
    {
        if (region.Type != RegionManager.RegionType.Prefab || 
            region.IsLoading || 
            region.IsLoaded)
        {
            return;
        }

        region.IsLoading = true;
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
            instance.transform.position = region.CachedBounds.center;

            region.Instance = instance;
            onLoaded?.Invoke(region);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load prefab for {region.RegionName}: {e}");
        }
        finally
        {
            region.IsLoading = false;
        }
    }

    public void Unload(RegionManager.Region region, Action<RegionManager.Region> onUnloaded)
    {
        if (region.Instance != null)
        {
            Object.Destroy(region.Instance);
            region.Instance = null;
        }

        if (region.PrefabRef != null)
            region.PrefabRef.ReleaseAsset();
        onUnloaded?.Invoke(region);
    }

    public bool IsLoading(RegionManager.Region region) => region.IsLoading;
    public bool IsLoaded(RegionManager.Region region) => region.IsLoaded;
}
using System;
using UnityEngine;

public class RegionStreamingLoader : MonoBehaviour, IStreamingLoader<RegionManager.Region>
{
    private IStreamingLoader<RegionManager.Region> _sceneLoader;
    private IStreamingLoader<RegionManager.Region> _prefabLoader;

    private void Awake()
    {
        _sceneLoader = new SceneStreamingLoader();
        _prefabLoader = new PrefabStreamingLoader();
    }

    public void Load(RegionManager.Region region, Action<RegionManager.Region> onLoaded) =>
        GetLoader(region).Load(region, onLoaded);

    public void Unload(RegionManager.Region region, Action<RegionManager.Region> onUnloaded) =>
        GetLoader(region).Unload(region, onUnloaded);

    public bool IsLoading(RegionManager.Region region) =>
        GetLoader(region).IsLoading(region);

    public bool IsLoaded(RegionManager.Region region) =>
        GetLoader(region).IsLoaded(region);

    private IStreamingLoader<RegionManager.Region> GetLoader(RegionManager.Region region) =>
        region.Type == RegionManager.RegionType.Scene ? _sceneLoader : _prefabLoader;
}
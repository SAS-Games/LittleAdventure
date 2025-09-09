using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RegionManager)), DisallowMultipleComponent]
public class StreamingController : MonoBehaviour
{
    [Header("Dependencies")] [SerializeField]
    private MonoBehaviour loaderComponent; // must implement IStreamingLoader

    [SerializeField] private MonoBehaviour targetComponent; // must implement IStreamingTarget

    [Header("Streaming Settings")] [SerializeField]
    private float m_UpdateInterval = 0.1f;

    private IStreamingLoader _streamingLoader;
    private IStreamingTarget _target;
    private RegionManager _regionManager;

    private readonly HashSet<string> _loadedRegions = new();
    private readonly HashSet<string> _desiredRegions = new();
    private readonly HashSet<string> _unloadCandidates = new();

    private Dictionary<string, RegionManager.Region> _regionLookup = new();
    private float _lastUpdateTime;

    private void Awake()
    {
        _regionManager = GetComponent<RegionManager>();
        _streamingLoader = loaderComponent as IStreamingLoader;
        if (_streamingLoader == null)
        {
            Debug.LogError("[StreamingController] Loader must implement IStreamingLoader.");
            enabled = false;
            return;
        }

        _target = targetComponent as IStreamingTarget;
        if (_target == null)
        {
            Debug.LogError("[StreamingController] Target must implement IStreamingTarget.");
            enabled = false;
            return;
        }

        foreach (var region in _regionManager.Scenes)
        {
            if (string.IsNullOrEmpty(region.RegionName))
                _regionLookup[region.RegionName] = region;
        }
    }

    private void Update()
    {
        if (Time.time - _lastUpdateTime < m_UpdateInterval) return;
        _lastUpdateTime = Time.time;

        UpdateDesiredRegions();
        HandleLoading();
        HandleUnloading();

        _regionManager.UpdateLoadedRegions(_loadedRegions);
    }

    private void UpdateDesiredRegions()
    {
        _desiredRegions.Clear();

        Bounds loadBounds = _target.GetLoadBounds();
        var nearby = _regionManager.FindRegionsInRange(loadBounds);
        foreach (var region in nearby)
        {
            _desiredRegions.Add(region.RegionName);
        }
    }

    private void HandleLoading()
    {
        foreach (var regionName in _desiredRegions)
        {
            if (_loadedRegions.Contains(regionName) || _streamingLoader.IsLoading(regionName))
                continue;

            _streamingLoader.Load(regionName, OnLoadComplete);
        }
    }

    private void HandleUnloading()
    {
        _unloadCandidates.Clear();
        _unloadCandidates.UnionWith(_loadedRegions);
        _unloadCandidates.ExceptWith(_desiredRegions);

        Bounds unloadBounds = _target.GetUnloadBounds();

        foreach (var regionName in _unloadCandidates)
        {
            if (!_regionLookup.TryGetValue(regionName, out var region)) continue;
            if (region.unloadStrategy == null) continue;

            if (region.unloadStrategy.ShouldUnload(unloadBounds, region) && !_streamingLoader.IsLoading(regionName))
            {
                _streamingLoader.Unload(regionName, OnUnloadComplete);
            }
        }
    }

    private void OnLoadComplete(string regionName) => _loadedRegions.Add(regionName);
    private void OnUnloadComplete(string regionName) => _loadedRegions.Remove(regionName);

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_target == null) return;

        // Load bounds
        Bounds loadBounds = _target.GetLoadBounds();
        Color loadWire = Color.yellow;
        Color loadFill = new Color(loadWire.r, loadWire.g, loadWire.b, 0.1f);

        Gizmos.color = loadFill;
        Gizmos.DrawCube(loadBounds.center, loadBounds.size);
        Gizmos.color = loadWire;
        Gizmos.DrawWireCube(loadBounds.center, loadBounds.size);

        // Unload bounds
        Bounds unloadBounds = _target.GetUnloadBounds();
        Color unloadWire = Color.red;
        Color unloadFill = new Color(unloadWire.r, unloadWire.g, unloadWire.b, 0.1f);

        Gizmos.color = unloadFill;
        Gizmos.DrawCube(unloadBounds.center, unloadBounds.size);
        Gizmos.color = unloadWire;
        Gizmos.DrawWireCube(unloadBounds.center, unloadBounds.size);
    }
#endif
}
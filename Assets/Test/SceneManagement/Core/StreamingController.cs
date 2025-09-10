using System.Collections.Generic;
using UnityEngine;
using Debug = SAS.Debug;

[RequireComponent(typeof(RegionManager)), DisallowMultipleComponent]
public class StreamingController : MonoBehaviour
{
    const string TAG = "StreamingController";

    [Header("Dependencies")] [SerializeField]
    private MonoBehaviour m_StreamingLoader;

    [Header("Streaming Settings")] [SerializeField]
    private float m_UpdateInterval = 0.1f;

    private IStreamingLoader<RegionManager.Region> _streamingLoader;
    private IRegionLoadBoundsProvider _target;
    private RegionManager _regionManager;

    private readonly HashSet<RegionManager.Region> _loadedRegions = new();
    private readonly HashSet<RegionManager.Region> _desiredRegions = new();
    private readonly HashSet<RegionManager.Region> _unloadCandidates = new();

    private float _lastUpdateTime;

    private void Awake()
    {
        _regionManager = GetComponent<RegionManager>();

        _streamingLoader = m_StreamingLoader as IStreamingLoader<RegionManager.Region>;
        if (_streamingLoader == null)
        {
            Debug.LogError("Loader must implement IStreamingLoader<Region>.", this, TAG);
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        if (_target == null)
            return;

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
            _desiredRegions.Add(region);
        }
    }

    private void HandleLoading()
    {
        foreach (var region in _desiredRegions)
        {
            if (_loadedRegions.Contains(region) || _streamingLoader.IsLoading(region))
                continue;

            _streamingLoader.Load(region, OnLoadComplete);
        }
    }

    private void HandleUnloading()
    {
        _unloadCandidates.Clear();
        _unloadCandidates.UnionWith(_loadedRegions);
        _unloadCandidates.ExceptWith(_desiredRegions);

        Bounds unloadBounds = _target.GetUnloadBounds();

        foreach (var region in _unloadCandidates)
        {
            if (region.UnloadStrategy == null) continue;

            if (region.UnloadStrategy.ShouldUnload(unloadBounds, region) && !_streamingLoader.IsLoading(region))
                _streamingLoader.Unload(region, OnUnloadComplete);
        }
    }

    private void OnLoadComplete(RegionManager.Region region) => _loadedRegions.Add(region);
    private void OnUnloadComplete(RegionManager.Region region) => _loadedRegions.Remove(region);

    public void SetRegionLoadBoundsProvider(IRegionLoadBoundsProvider regionLoadBoundsProvider)
    {
        _target = regionLoadBoundsProvider;
    }

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
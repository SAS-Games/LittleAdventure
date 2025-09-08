using System.Collections.Generic;
using UnityEngine;

public interface IStreamingTarget
{
    Bounds GetLoadBounds();
    Bounds GetUnloadBounds();
}

public class SceneStreamingManager : MonoBehaviour
{
    [Header("Dependencies")] [SerializeField]
    private SceneBoundsManager boundsManager;

    [SerializeField] private MonoBehaviour m_StreamLoader; // must implement IStreamingLoader
    [SerializeField] private MonoBehaviour m_Target; // must implement IStreamingTarget

    [Header("Streaming Settings")] [SerializeField]
    private float updateInterval = 0.1f;

    private IStreamingLoader _streamLoader;
    private IStreamingTarget _target;

    private readonly HashSet<string> _loadedScenes = new();
    private readonly HashSet<string> _desiredScenes = new();
    private readonly HashSet<string> _candidates = new(); // reused for unloading

    private Dictionary<string, SceneBoundsManager.SceneRef> _sceneRefsByName;

    private float _lastUpdateTime;

    private void Awake()
    {
        if (boundsManager == null)
        {
            Debug.LogError("[SceneStreamingManager] Missing SceneBoundsManager.");
            enabled = false;
            return;
        }

        _streamLoader = m_StreamLoader as IStreamingLoader;
        if (_streamLoader == null)
        {
            Debug.LogError("[SceneStreamingManager] StreamLoader must implement IStreamingLoader.");
            enabled = false;
            return;
        }

        _target = m_Target as IStreamingTarget;
        if (_target == null)
        {
            Debug.LogError("[SceneStreamingManager] Target must implement IStreamingTarget.");
            enabled = false;
            return;
        }

        // Pre-cache scene references by name
        _sceneRefsByName = new Dictionary<string, SceneBoundsManager.SceneRef>();
        foreach (var sceneRef in boundsManager.Scenes)
        {
            if (sceneRef.sceneAsset != null)
                _sceneRefsByName[sceneRef.sceneAsset.name] = sceneRef;
        }
    }

    private void Update()
    {
        if (Time.time - _lastUpdateTime < updateInterval) return;
        _lastUpdateTime = Time.time;

        UpdateDesiredScenes();
        HandleLoading();
        HandleUnloading();
    }

    private void UpdateDesiredScenes()
    {
        _desiredScenes.Clear();

        Bounds loadBounds = _target.GetLoadBounds();
        var nearby = boundsManager.GetNearbyScenes(loadBounds);
        foreach (var sceneRef in nearby)
        {
            if (sceneRef.sceneAsset != null)
                _desiredScenes.Add(sceneRef.sceneAsset.name);
        }
    }

    private void HandleLoading()
    {
        foreach (var sceneName in _desiredScenes)
        {
            if (_loadedScenes.Contains(sceneName) || _streamLoader.IsLoading(sceneName))
                continue;

            _streamLoader.Load(sceneName, OnLoadComplete);
        }
    }

    private void HandleUnloading()
    {
        _candidates.Clear();
        _candidates.UnionWith(_loadedScenes);
        _candidates.ExceptWith(_desiredScenes);

        Bounds unloadBounds = _target.GetUnloadBounds();

        foreach (var sceneName in _candidates)
        {
            if (!_sceneRefsByName.TryGetValue(sceneName, out var sceneRef)) continue;
            if (sceneRef.unloadStrategy == null) continue;

            if (sceneRef.unloadStrategy.ShouldUnload(unloadBounds, sceneRef) &&
                !_streamLoader.IsLoading(sceneName))
            {
                _streamLoader.Unload(sceneName, OnUnloadComplete);
            }
        }
    }

    private void OnLoadComplete(string sceneName)
    {
        _loadedScenes.Add(sceneName);
#if UNITY_EDITOR
        Debug.Log($"[SceneStreamingManager] Loaded {sceneName}");
#endif
    }

    private void OnUnloadComplete(string sceneName)
    {
        _loadedScenes.Remove(sceneName);
#if UNITY_EDITOR
        Debug.Log($"[SceneStreamingManager] Unloaded {sceneName}");
#endif
    }
}
using System.Collections.Generic;
using UnityEngine;
using Debug = SAS.Debug;

namespace LevelStreaming
{
    [RequireComponent(typeof(RegionManager)), DisallowMultipleComponent]
    public class RegionStreamingController : MonoBehaviour
    {
        const string TAG = "StreamingController";

        [SerializeField] private RuntimeScriptableObject<RegionStreamingLoader> m_StreamingLoader;
        [SerializeField] private float m_UpdateInterval = 0.1f;

        private RegionStreamingLoader _streamingLoader => m_StreamingLoader;
        private IStreamingBoundsProvider _target;
        private RegionManager _regionManager;

        private readonly HashSet<RegionManager.Region> _desiredRegions = new();
        private readonly HashSet<RegionManager.Region> _loadedRegions = new();
        private readonly HashSet<RegionManager.Region> _unloadCandidates = new();
        private readonly HashSet<RegionManager.Region> _activeRegions = new();

        private float _lastUpdateTime;

        private void Awake()
        {
            _regionManager = GetComponent<RegionManager>();
            if (_streamingLoader == null)
            {
                Debug.LogError("Loader must implement IStreamingLoader<Region>.", this, TAG);
                enabled = false;
                return;
            }

            _streamingLoader.Initialize(this, _regionManager);
        }

        private void Update()
        {
            if (_target == null)
                return;

            if (Time.time - _lastUpdateTime < m_UpdateInterval)
                return;

            _lastUpdateTime = Time.time;

            UpdateDesiredRegions();
            HandleUnloading();
            HandleLoading();
            HandleActivation();

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
                var meta = _regionManager.GetOrCreateMeta(region);
                meta.LastTimeDesired = Time.time;
            }

            foreach (var region in nearby)
                CheckRegion(region);
            foreach (var region in _loadedRegions)
                CheckRegion(region);

            void CheckRegion(RegionManager.Region region)
            {
                for (int i = 0; i < region.Portals.Count; i++)
                {
                    var portalBounds = region.CachedWorldPortalBounds[i];
                    if (loadBounds.Intersects(portalBounds))
                    {
                        if (_regionManager.RegionLookup.TryGetValue(region.Portals[i].TargetRegionName, out var target))
                        {
                            _desiredRegions.Add(target);
                            var meta = _regionManager.GetOrCreateMeta(region);
                            meta.LastTimeDesired = Time.time;
                        }
                    }
                }
            }
        }

        private void HandleLoading()
        {
            foreach (var region in _desiredRegions)
            {
                if (_regionManager.TryGetMeta(region, out var meta))
                {
                    if (meta.IsLoaded || meta.IsLoading)
                        continue;
                }

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
                if (region.UnloadStrategy == null) 
                    continue;
                
                if (region.UnloadStrategy.ShouldUnload(unloadBounds, _regionManager, region) &&
                    _regionManager.TryGetMeta(region, out var meta) && !meta.IsLoading)
                    _streamingLoader.Unload(region, OnUnloadComplete);
            }
        }

        private void HandleActivation()
        {
            Bounds activateBounds = _target.GetActivateBounds();

            foreach (var region in _loadedRegions)
            {
                bool inside = activateBounds.Intersects(region.CachedBounds);

                if (inside && !_activeRegions.Contains(region))
                {
                    CallRegionActivatable(region, true);
                    _activeRegions.Add(region);
                }
                else if (!inside && _activeRegions.Contains(region))
                {
                    CallRegionActivatable(region, false);
                    _activeRegions.Remove(region);
                }
            }
        }

        private void CallRegionActivatable(RegionManager.Region region, bool active)
        {
            var meta = _regionManager.GetOrCreateMeta(region);
            if (region.Type == RegionManager.RegionType.Scene)
            {
                var rootObjects = UnityEngine.SceneManagement.SceneManager
                    .GetSceneByPath(region.SceneRef.ScenePath).GetRootGameObjects();

                foreach (var go in rootObjects)
                {
                    var activatable = go.GetComponent<IRegionActivatable>();
                    if (activatable != null)
                        activatable.OnRegionActivated(region, active);
                }
            }
            else if (region.Type == RegionManager.RegionType.Prefab && meta.Instance != null)
            {
                var activatable = meta.Instance.GetComponent<IRegionActivatable>();
                if (activatable != null)
                    activatable.OnRegionActivated(region, active);
            }

            Debug.Log($"Region {region.RegionName} is in active range {active}", this, TAG);
        }

        private void OnLoadComplete(RegionManager.Region region)
        {
            _loadedRegions.Add(region);

            if (_target != null && _target.GetActivateBounds().Intersects(region.CachedBounds))
            {
                CallRegionActivatable(region, true);
                _activeRegions.Add(region);
            }

            _regionManager.MarkRegionLoaded(region);
            _regionManager.UpdateLoadedRegions(_loadedRegions);
        }

        private void OnUnloadComplete(RegionManager.Region region)
        {
            _loadedRegions.Remove(region);
            _activeRegions.Remove(region);
            _regionManager.MarkRegionUnloaded(region);
            _regionManager.UpdateLoadedRegions(_loadedRegions);
        }

        public void SetRegionLoadBoundsProvider(IStreamingBoundsProvider streamingBoundsProvider)
        {
            _target = streamingBoundsProvider;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_target == null) return;

            DrawBounds(_target.GetLoadBounds(), Color.yellow);
            DrawBounds(_target.GetActivateBounds(), Color.blue);
            DrawBounds(_target.GetUnloadBounds(), Color.red);
        }

        private void DrawBounds(Bounds b, Color c)
        {
            Color fill = new Color(c.r, c.g, c.b, 0.1f);
            Gizmos.color = fill;
            Gizmos.DrawCube(b.center, b.size);
            Gizmos.color = c;
            Gizmos.DrawWireCube(b.center, b.size);
        }
#endif
    }
}
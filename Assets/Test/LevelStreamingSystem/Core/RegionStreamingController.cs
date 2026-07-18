using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace LevelStreaming
{
    [RequireComponent(typeof(RegionManager)), DisallowMultipleComponent]
    public class RegionStreamingController : MonoBehaviour
    {
        [Flags]
        public enum RegionDesireReason
        {
            None = 0,
            Bounds = 1,
            Portal = 2
        }

        public readonly struct RegionDebugSnapshot
        {
            public RegionDebugSnapshot(string name, RegionManager.RegionType type,
                RegionManager.RegionStreamingState state, RegionDesireReason desireReason,
                bool isActive, string registryKey, int registryReferenceCount,
                int consecutiveFailures, string lastError)
            {
                Name = name;
                Type = type;
                State = state;
                DesireReason = desireReason;
                IsActive = isActive;
                RegistryKey = registryKey;
                RegistryReferenceCount = registryReferenceCount;
                ConsecutiveFailures = consecutiveFailures;
                LastError = lastError;
            }

            public string Name { get; }
            public RegionManager.RegionType Type { get; }
            public RegionManager.RegionStreamingState State { get; }
            public RegionDesireReason DesireReason { get; }
            public bool IsActive { get; }
            public string RegistryKey { get; }
            public int RegistryReferenceCount { get; }
            public int ConsecutiveFailures { get; }
            public string LastError { get; }
        }

        [SerializeField] private RegionStreamingLoader m_StreamingLoader;
        [SerializeField, Min(0f)] private float m_UpdateInterval = 0.1f;
        [SerializeField] private bool m_LogStateChanges;
#if UNITY_EDITOR
        [SerializeField] private bool m_DrawStreamingBounds = true;
#endif

        private RegionStreamingLoader _streamingLoader;
        private IStreamingBoundsProvider _target;
        private RegionManager _regionManager;

        private readonly HashSet<RegionManager.Region> _desiredRegions = new();
        private readonly HashSet<RegionManager.Region> _loadedRegions = new();
        private readonly HashSet<RegionManager.Region> _unloadCandidates = new();
        private readonly HashSet<RegionManager.Region> _activeRegions = new();
        private readonly Dictionary<RegionManager.Region, Task> _operations = new();
        private readonly Dictionary<RegionManager.Region, RegionDesireReason> _desireReasons = new();
        private readonly HashSet<RegionManager.RegionType> _unsupportedTypesReported = new();

        private float _lastUpdateTime;
        private bool _isShuttingDown;
        private Task _shutdownTask;

        public bool IsShuttingDown => _isShuttingDown;
        public IReadOnlyCollection<RegionManager.Region> DesiredRegions => _desiredRegions;
        public IReadOnlyCollection<RegionManager.Region> LoadedRegions => _loadedRegions;
        public IReadOnlyCollection<RegionManager.Region> ActiveRegions => _activeRegions;

        private void Awake()
        {
            _regionManager = GetComponent<RegionManager>();
            if (m_StreamingLoader == null)
            {
                Debug.LogError("A RegionStreamingLoader asset must be assigned.", this);
                enabled = false;
                return;
            }

            _streamingLoader = m_StreamingLoader.CreateRuntimeInstance();
            if (_streamingLoader == null)
            {
                Debug.LogError($"Could not create a runtime loader from '{m_StreamingLoader.name}'.", this);
                enabled = false;
                return;
            }

            _streamingLoader.Initialize(this, _regionManager);
        }

        private void Update()
        {
            if (_isShuttingDown || !HasStreamingTarget() || _streamingLoader == null)
                return;

            if (Time.time - _lastUpdateTime < m_UpdateInterval)
                return;

            _lastUpdateTime = Time.time;

            UpdateDesiredRegions();
            _regionManager.UpdateDesiredRegions(_desiredRegions);
            HandleUnloading();
            HandleLoading();
            HandleActivation();

            _regionManager.UpdateLoadedRegions(_loadedRegions);
        }

        private void UpdateDesiredRegions()
        {
            _desiredRegions.Clear();
            _desireReasons.Clear();

            Bounds loadBounds = _target.GetLoadBounds();
            var nearby = _regionManager.FindRegionsInRange(loadBounds);
            foreach (var region in nearby)
                MarkDesired(region, RegionDesireReason.Bounds);

            foreach (var region in nearby)
                CheckPortals(region);
            foreach (var region in _loadedRegions)
                CheckPortals(region);

            void MarkDesired(RegionManager.Region region, RegionDesireReason reason)
            {
                if (region == null)
                    return;

                _desiredRegions.Add(region);
                _desireReasons.TryGetValue(region, out RegionDesireReason currentReason);
                _desireReasons[region] = currentReason | reason;
                _regionManager.GetOrCreateMeta(region).LastTimeDesired = Time.time;
            }

            void CheckPortals(RegionManager.Region source)
            {
                if (source?.Portals == null)
                    return;

                int count = Mathf.Min(source.Portals.Count, source.CachedWorldPortalBounds.Count);
                for (int i = 0; i < count; i++)
                {
                    var portal = source.Portals[i];
                    if (portal == null || string.IsNullOrWhiteSpace(portal.TargetRegionName))
                        continue;
                    if (!loadBounds.Intersects(source.CachedWorldPortalBounds[i]))
                        continue;

                    if (_regionManager.RegionLookup.TryGetValue(portal.TargetRegionName, out var target))
                        MarkDesired(target, RegionDesireReason.Portal);
                }
            }
        }

        private void HandleLoading()
        {
            foreach (var region in _desiredRegions)
            {
                if (region == null || !_streamingLoader.Supports(region.Type))
                {
                    if (region != null && _unsupportedTypesReported.Add(region.Type))
                    {
                        Debug.LogWarning(
                            $"Skipping '{region.Type}' streaming regions because their optional backend is not installed. " +
                            "Build Settings scene streaming remains available.",
                            this);
                    }
                    continue;
                }

                if (!_regionManager.TryBeginRegionLoad(region))
                    continue;

                TrackOperation(region, LoadRegionAsync(region));
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
                if (region?.UnloadStrategy == null)
                    continue;

                bool shouldUnload;
                try
                {
                    shouldUnload = region.UnloadStrategy.ShouldUnload(unloadBounds, _regionManager, region);
                }
                catch (Exception exception)
                {
                    Debug.LogError(
                        $"Unload strategy for region '{region.RegionName}' failed: {exception}",
                        region.UnloadStrategy);
                    continue;
                }

                if (!shouldUnload)
                    continue;
                if (!_regionManager.TryBeginRegionUnload(region))
                    continue;

                DeactivateRegion(region);
                TrackOperation(region, UnloadRegionAsync(region));
            }
        }

        private void HandleActivation()
        {
            Bounds activateBounds = _target.GetActivateBounds();

            foreach (var region in _loadedRegions)
            {
                if (!_regionManager.TryGetMeta(region, out var meta) ||
                    meta.State != RegionManager.RegionStreamingState.Loaded)
                    continue;

                if (activateBounds.Intersects(region.CachedBounds))
                    ActivateRegion(region);
                else
                    DeactivateRegion(region);
            }
        }

        private async Task LoadRegionAsync(RegionManager.Region region)
        {
            try
            {
                await _streamingLoader.LoadAsync(region);
            }
            catch (Exception exception)
            {
                _regionManager.MarkRegionLoadFailed(region, exception);
                Debug.LogError($"Failed to load region '{region.RegionName}': {exception}", this);
                return;
            }

            try
            {
                // Give newly instantiated/activated content a frame boundary so Start
                // runs before IRegionActivatable notifications.
                await UnityAsync.NextFrame();
            }
            catch (Exception exception)
            {
                // The resource is already physically owned. A frame-delay failure is a
                // diagnostic, not a load failure, and shutdown still needs to release it.
                Debug.LogWarning(
                    $"Region '{region.RegionName}' could not defer activation by one frame: {exception.Message}",
                    this);
            }

            // Physical ownership has succeeded. Completion/activation diagnostics must
            // never relabel that resource as a failed load and strand its registry ref.
            try
            {
                OnLoadComplete(region);
            }
            catch (Exception exception)
            {
                EnsureLoadedBookkeeping(region);
                Debug.LogError(
                    $"Region '{region.RegionName}' loaded, but completion bookkeeping failed: {exception}",
                    this);
            }
        }

        private async Task UnloadRegionAsync(RegionManager.Region region)
        {
            try
            {
                await _streamingLoader.UnloadAsync(region);
            }
            catch (Exception exception)
            {
                _regionManager.MarkRegionUnloadFailed(region, exception);
                Debug.LogError($"Failed to unload region '{region.RegionName}': {exception}", this);
                return;
            }

            // The physical resource is gone. A diagnostic/bookkeeping exception must
            // not transition the region back to Loaded.
            try
            {
                OnUnloadComplete(region);
            }
            catch (Exception exception)
            {
                EnsureUnloadedBookkeeping(region);
                Debug.LogError(
                    $"Region '{region.RegionName}' unloaded, but completion bookkeeping failed: {exception}",
                    this);
            }
        }

        private void TrackOperation(RegionManager.Region region, Task operation)
        {
            _operations[region] = operation;
            _ = RemoveWhenComplete(region, operation);
        }

        private async Task RemoveWhenComplete(RegionManager.Region region, Task operation)
        {
            // Region operations catch and report their own failures, but awaiting here also
            // guarantees the task is observed if a future implementation regresses.
            try
            {
                await operation;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
            finally
            {
                if (_operations.TryGetValue(region, out var current) && ReferenceEquals(current, operation))
                    _operations.Remove(region);
            }
        }

        private void ActivateRegion(RegionManager.Region region)
        {
            if (!_activeRegions.Add(region))
                return;

            NotifyRegionActivatables(region, true);
            LogState(region, "activated");
        }

        private void DeactivateRegion(RegionManager.Region region)
        {
            if (!_activeRegions.Remove(region))
                return;

            NotifyRegionActivatables(region, false);
            LogState(region, "deactivated");
        }

        private void NotifyRegionActivatables(RegionManager.Region region, bool active)
        {
            var meta = _regionManager.GetOrCreateMeta(region);
            RegionManager.RegionType loadedType = meta.LoadedType ?? region.Type;
            if (loadedType is RegionManager.RegionType.Scene or RegionManager.RegionType.AddressableScene)
            {
                var scene = meta.LoadedScene;
                if (!scene.IsValid() || !scene.isLoaded)
                    return;

                foreach (var root in scene.GetRootGameObjects())
                    NotifyChildren(root, region, active);
            }
            else if (loadedType == RegionManager.RegionType.Prefab && meta.Instance != null)
            {
                NotifyChildren(meta.Instance, region, active);
            }
        }

        private void NotifyChildren(GameObject root, RegionManager.Region region, bool active)
        {
            foreach (var behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour is not IRegionActivatable activatable)
                    continue;

                try
                {
                    activatable.OnRegionActivated(region, active);
                }
                catch (Exception exception)
                {
                    Debug.LogError(
                        $"IRegionActivatable on '{behaviour.name}' failed while setting region " +
                        $"'{region.RegionName}' active={active}: {exception}", behaviour);
                }
            }
        }

        private void OnLoadComplete(RegionManager.Region region)
        {
            EnsureLoadedBookkeeping(region);
            LogState(region, "loaded");

            if (!_isShuttingDown && HasStreamingTarget() &&
                _target.GetActivateBounds().Intersects(region.CachedBounds))
                ActivateRegion(region);
        }

        private void OnUnloadComplete(RegionManager.Region region)
        {
            EnsureUnloadedBookkeeping(region);
            LogState(region, "unloaded");
        }

        private void EnsureLoadedBookkeeping(RegionManager.Region region)
        {
            if (!_regionManager.TryGetMeta(region, out var meta) ||
                meta.State != RegionManager.RegionStreamingState.Loaded)
                _regionManager.MarkRegionLoaded(region);

            _loadedRegions.Add(region);
            _regionManager.UpdateLoadedRegions(_loadedRegions);
        }

        private void EnsureUnloadedBookkeeping(RegionManager.Region region)
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

        public IReadOnlyList<RegionDebugSnapshot> GetDebugSnapshot()
        {
            if (_regionManager == null)
                return Array.Empty<RegionDebugSnapshot>();

            var snapshot = new List<RegionDebugSnapshot>(_regionManager.Regions.Count);
            foreach (var region in _regionManager.Regions)
            {
                if (region == null)
                    continue;

                _regionManager.TryGetMeta(region, out var meta);
                _desireReasons.TryGetValue(region, out RegionDesireReason desireReason);

                int referenceCount = 0;
                string registryKey = meta?.RegistryKey;
                if (_regionManager.Registry != null && !string.IsNullOrWhiteSpace(registryKey) &&
                    _regionManager.Registry.TryGetSnapshot(registryKey, out var registryEntry))
                    referenceCount = registryEntry.ReferenceCount;

                snapshot.Add(new RegionDebugSnapshot(
                    region.RegionName,
                    region.Type,
                    meta?.State ?? RegionManager.RegionStreamingState.Unloaded,
                    desireReason,
                    _activeRegions.Contains(region),
                    registryKey,
                    referenceCount,
                    meta?.ConsecutiveFailures ?? 0,
                    meta?.LastError));
            }

            return snapshot;
        }

        private bool HasStreamingTarget()
        {
            if (_target == null)
                return false;

            if (_target is UnityEngine.Object unityObject && unityObject == null)
            {
                _target = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Stops new streaming work, waits for operations already in flight, deactivates
        /// all active regions, and unloads every region owned by this controller.
        /// Callers that explicitly tear down a streaming world should await this method
        /// before destroying the manager.
        /// </summary>
        public Task ShutdownAsync()
        {
            return _shutdownTask ??= ShutdownCoreAsync();
        }

        private async Task ShutdownCoreAsync()
        {
            _isShuttingDown = true;
            enabled = false;

            foreach (var region in new List<RegionManager.Region>(_activeRegions))
                DeactivateRegion(region);

            if (_operations.Count > 0)
                await Task.WhenAll(new List<Task>(_operations.Values));

            var unloadTasks = new List<Task>();
            foreach (var region in new List<RegionManager.Region>(_loadedRegions))
            {
                if (!_regionManager.TryBeginRegionUnload(region, ignoreRetryDelay: true))
                    continue;

                Task operation = UnloadRegionAsync(region);
                TrackOperation(region, operation);
                unloadTasks.Add(operation);
            }

            if (unloadTasks.Count > 0)
                await Task.WhenAll(unloadTasks);

            _desiredRegions.Clear();
            _desireReasons.Clear();
            _regionManager.UpdateDesiredRegions(_desiredRegions);

            int registryCount = _regionManager.Registry?.Count ?? 0;
            if (_loadedRegions.Count > 0 || registryCount > 0)
            {
                Debug.LogError(
                    $"Streaming shutdown completed with {_loadedRegions.Count} loaded region(s) and " +
                    $"{registryCount} registry entry/entries still resident.",
                    this);
            }

            RegionStreamingLoader runtimeLoader = _streamingLoader;
            _streamingLoader = null;
            if (runtimeLoader != null)
                Destroy(runtimeLoader);
        }

        private void OnDestroy()
        {
            if (!_isShuttingDown && _streamingLoader != null)
                _ = ShutdownAsync();
        }

        private void LogState(RegionManager.Region region, string state)
        {
            if (m_LogStateChanges)
                Debug.Log($"Region '{region.RegionName}' {state}.", this);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            m_UpdateInterval = Mathf.Max(0f, m_UpdateInterval);
        }

        private void OnDrawGizmos()
        {
            if (!m_DrawStreamingBounds || !HasStreamingTarget())
                return;

            DrawBounds(_target.GetLoadBounds(), Color.yellow);
            DrawBounds(_target.GetActivateBounds(), Color.blue);
            DrawBounds(_target.GetUnloadBounds(), Color.red);
        }

        private static void DrawBounds(Bounds bounds, Color color)
        {
            Color fill = new(color.r, color.g, color.b, 0.1f);
            Gizmos.color = fill;
            Gizmos.DrawCube(bounds.center, bounds.size);
            Gizmos.color = color;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
#endif
    }
}

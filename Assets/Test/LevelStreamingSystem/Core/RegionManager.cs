using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace LevelStreaming
{
    public partial class RegionManager : MonoBehaviour
    {
        [Serializable]
        public class Portal
        {
            [field: SerializeField] public string TargetRegionName { get; private set; }
            [field: SerializeField] public Bounds LocalBounds { get; set; }=new Bounds(Vector3.zero, Vector3.one);
        }

        public enum RegionType
        {
            Scene,
            Prefab,
            AddressableScene
        }

        [Serializable]
        public partial class Region
        {
            [FormerlySerializedAs("<SceneRef>k__BackingField")]
            [SerializeField] private SceneReference sceneRef = new();
            [FormerlySerializedAs("<PrefabRef>k__BackingField")]
            [FormerlySerializedAs("prefabAddress")]
            [SerializeField] private StreamingPrefabReference prefabRef = new();
            [SerializeField] private StreamingSceneReference addressableSceneRef = new();
            [FormerlySerializedAs("<RegionName>k__BackingField")]
            [SerializeField] private string regionName = string.Empty;
            [FormerlySerializedAs("<Type>k__BackingField")]
            [FormerlySerializedAs("regionType")]
            [SerializeField] private RegionType type;
            [FormerlySerializedAs("<CachedBounds>k__BackingField")]
            [SerializeField] private Bounds cachedBounds = new Bounds(Vector3.zero, Vector3.one*2);
            [FormerlySerializedAs("<Portals>k__BackingField")]
            [SerializeField] private List<Portal> portals = new();
            [FormerlySerializedAs("<UnloadStrategy>k__BackingField")]
            [SerializeField] private UnloadStrategy unloadStrategy;

            public SceneReference SceneRef => sceneRef;
            public StreamingPrefabReference PrefabRef => prefabRef;
            public StreamingSceneReference AddressableSceneRef => addressableSceneRef;
            public string RegionName => regionName;
            public RegionType Type => type;
            public Bounds CachedBounds { get => cachedBounds; set => cachedBounds = value; }
            public List<Portal> Portals => portals;
            public UnloadStrategy UnloadStrategy => unloadStrategy;
            [NonSerialized] public List<Bounds> CachedWorldPortalBounds = new();

            /// <summary>
            /// Rebuild the portal bounds in world-space using the region's cached bounds.
            /// </summary>
            public void RebuildPortalWorldBounds()
            {
                CachedWorldPortalBounds.Clear();
                if (Portals == null)
                    return;

                foreach (var portal in Portals)
                {
                    if (portal == null)
                    {
                        // Keep indices aligned with Portals so runtime checks can safely
                        // address the cached list even when serialized data is malformed.
                        CachedWorldPortalBounds.Add(default);
                        continue;
                    }

                    var b = portal.LocalBounds;
                    b.center += CachedBounds.center;
                    CachedWorldPortalBounds.Add(b);
                }
            }
        }

        [FormerlySerializedAs("<Regions>k__BackingField")]
        [FormerlySerializedAs("<Scenes>k__BackingField")]
        [SerializeField] private List<Region> regions = new();
        public IReadOnlyList<Region> Regions => regions ??= new List<Region>();
        [field: SerializeField]
        [field: FormerlySerializedAs("m_RegionSelectionStrategy")]
        [field: FormerlySerializedAs("m_StreamingStrategy")]
        public RegionSelectionStrategySO RegionSelectionStrategy { get; private set; }
        public readonly HashSet<Region> loadedRegions = new();
        private readonly HashSet<Region> _desiredRegions = new();
        public Dictionary<string, Region> RegionLookup { get; private set; }
        private readonly Dictionary<Region, RegionMetaData> _metaByRegion = new();
        private RegionSelectionStrategySO _runtimeRegionSelectionStrategy;
        public SharedStreamingRegistry Registry { get; private set; }
        public RegionSelectionStrategySO ActiveRegionSelectionStrategy =>
            _runtimeRegionSelectionStrategy != null ? _runtimeRegionSelectionStrategy : RegionSelectionStrategy;

        void Awake()
        {
            var regionNames = new HashSet<string>(StringComparer.Ordinal);
            var validRegions = new List<Region>();
            Registry = new SharedStreamingRegistry();
            foreach (var region in Regions)
            {
                if (region == null)
                {
                    Debug.LogError("Null region entry detected.", this);
                    continue;
                }

                validRegions.Add(region);

                if (string.IsNullOrEmpty(region.RegionName))
                {
                    Debug.LogWarning($"Region has no valid name: {region.Type}", this);
                }
                else if (!regionNames.Add(region.RegionName))
                    Debug.LogWarning($"Duplicate region name detected: {region.RegionName}", this);

                GetOrCreateMeta(region);

                // Precompute portal world bounds
                region.RebuildPortalWorldBounds();
            }

            BuildLookup();
            if (RegionSelectionStrategy != null)
            {
                _runtimeRegionSelectionStrategy = Instantiate(RegionSelectionStrategy);
                _runtimeRegionSelectionStrategy.name = $"{RegionSelectionStrategy.name} (Runtime)";
                _runtimeRegionSelectionStrategy.Initialize(validRegions);
            }
            else
                Debug.LogError("No RegionSelectionStrategy assigned!", this);
        }

        private void OnDestroy()
        {
            if (_runtimeRegionSelectionStrategy != null)
                Destroy(_runtimeRegionSelectionStrategy);
        }

        public void UpdateLoadedRegions(HashSet<Region> loadedRegions)
        {
            this.loadedRegions.Clear();
            this.loadedRegions.UnionWith(loadedRegions);
        }

        public void UpdateDesiredRegions(HashSet<Region> desiredRegions)
        {
            _desiredRegions.Clear();
            _desiredRegions.UnionWith(desiredRegions);
        }

        public bool IsRegionDesired(Region region) => region != null && _desiredRegions.Contains(region);

        public List<Region> FindRegionsInRange(Bounds queryBounds)
        {
            if (ActiveRegionSelectionStrategy == null)
            {
                Debug.LogError("[RegionManager] Not initialized with a strategy!");
                return new List<Region>();
            }

            return ActiveRegionSelectionStrategy.GetNearbyRegions(queryBounds);
        }

        private void BuildLookup()
        {
            RegionLookup = new Dictionary<string, Region>(StringComparer.Ordinal);
            foreach (var region in Regions)
            {
                if (region == null || string.IsNullOrWhiteSpace(region.RegionName))
                    continue;

                if (RegionLookup.ContainsKey(region.RegionName))
                    Debug.LogError($"Duplicate region name '{region.RegionName}'. The first region will be used.", this);
                else
                    RegionLookup.Add(region.RegionName, region);
            }
        }

        public bool TryBeginRegionLoad(Region region)
        {
            return region != null && GetOrCreateMeta(region).TryBeginLoad(Time.realtimeSinceStartup);
        }

        public void MarkRegionLoaded(Region region)
        {
            if (region == null) return;

            var meta = GetOrCreateMeta(region);
            meta.State = RegionStreamingState.Loaded;
            meta.LoadedTime = Time.time;
            meta.LastTimeDesired = Time.time;
            meta.RetryNotBeforeRealtime = 0f;
            meta.ConsecutiveFailures = 0;
            meta.LastError = null;
        }

        public void MarkRegionLoadFailed(Region region, Exception exception)
        {
            if (region == null)
                return;

            var meta = GetOrCreateMeta(region);
            meta.State = RegionStreamingState.Failed;
            meta.RecordFailure(exception, Time.realtimeSinceStartup);
        }

        public bool TryBeginRegionUnload(Region region, bool ignoreRetryDelay = false)
        {
            return region != null &&
                   GetOrCreateMeta(region).TryBeginUnload(Time.realtimeSinceStartup, ignoreRetryDelay);
        }

        public void MarkRegionUnloadFailed(Region region, Exception exception)
        {
            if (region == null)
                return;

            var meta = GetOrCreateMeta(region);
            meta.State = RegionStreamingState.Loaded;
            meta.RecordFailure(exception, Time.realtimeSinceStartup);
        }

        public void MarkRegionUnloaded(Region region)
        {
            if (region == null)
                return;

            if (_metaByRegion.TryGetValue(region, out var meta))
                meta.ResetTransient();
        }

        public enum RegionStreamingState
        {
            Unloaded,
            Loading,
            Loaded,
            Unloading,
            Failed
        }

        public class RegionMetaData
        {
            public RegionStreamingState State { get; internal set; } = RegionStreamingState.Unloaded;
            public bool IsLoading => State == RegionStreamingState.Loading;
            public bool IsLoaded => State == RegionStreamingState.Loaded;
            public bool IsUnloading => State == RegionStreamingState.Unloading;
            public float LoadedTime = -1f;
            public GameObject Instance = null;
            public Scene LoadedScene;
            public string RegistryKey { get; internal set; }
            public RegionType? LoadedType { get; internal set; }

            public float LastTimeDesired = -1f;
            public object UserData = null;
            public string LastError { get; internal set; }
            public int ConsecutiveFailures { get; internal set; }
            public float RetryNotBeforeRealtime { get; internal set; }

            internal bool TryBeginLoad(float realtime)
            {
                if (State != RegionStreamingState.Unloaded && State != RegionStreamingState.Failed)
                    return false;
                if (realtime < RetryNotBeforeRealtime)
                    return false;

                State = RegionStreamingState.Loading;
                LastError = null;
                return true;
            }

            internal bool TryBeginUnload(float realtime, bool ignoreRetryDelay)
            {
                if (State != RegionStreamingState.Loaded ||
                    (!ignoreRetryDelay && realtime < RetryNotBeforeRealtime))
                    return false;

                State = RegionStreamingState.Unloading;
                LastError = null;
                return true;
            }

            internal void RecordFailure(Exception exception, float realtime)
            {
                ConsecutiveFailures++;
                LastError = exception?.Message ?? "Unknown streaming failure.";
                float delay = Mathf.Min(30f, 0.5f * Mathf.Pow(2f, Mathf.Min(ConsecutiveFailures - 1, 6)));
                RetryNotBeforeRealtime = realtime + delay;
            }

            public void ResetTransient()
            {
                State = RegionStreamingState.Unloaded;
                LoadedTime = -1f;
                Instance = null;
                LoadedScene = default;
                RegistryKey = null;
                LoadedType = null;
                LastTimeDesired = -1f;
                UserData = null;
                LastError = null;
                ConsecutiveFailures = 0;
                RetryNotBeforeRealtime = 0f;
            }
        }

        /// <summary>
        /// Return existing metadata or create a new one for the region.
        /// </summary>
        public RegionMetaData GetOrCreateMeta(Region region)
        {
            if (region == null)
                throw new ArgumentNullException(nameof(region));

            if (!_metaByRegion.TryGetValue(region, out var meta))
            {
                meta = new RegionMetaData();
                meta.LoadedTime = -1f;
                _metaByRegion[region] = meta;
            }

            return meta;
        }

        public bool TryGetMeta(Region region, out RegionMetaData meta)
        {
            if (region == null)
            {
                meta = null;
                return false;
            }

            return _metaByRegion.TryGetValue(region, out meta);
        }

        public void RemoveMeta(Region region)
        {
            if (region == null) return;
            _metaByRegion.Remove(region);
        }

        public void ResetMeta(Region region)
        {
            if (region == null)
                return;

            if (_metaByRegion.TryGetValue(region, out var meta))
                meta.ResetTransient();
        }

        public void ClearAllMeta()
        {
            _metaByRegion.Clear();
        }

        public bool IsRegionLoaded(Region region)
        {
            return TryGetMeta(region, out var meta) &&
                   meta.State is RegionStreamingState.Loaded or RegionStreamingState.Unloading;
        }

        public bool IsRegionLoading(Region region)
        {
            return TryGetMeta(region, out var meta) && meta.IsLoading;
        }

        public bool IsRegionUnloading(Region region)
        {
            return TryGetMeta(region, out var meta) && meta.IsUnloading;
        }
    }
}

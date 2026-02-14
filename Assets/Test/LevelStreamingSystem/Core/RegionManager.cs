using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Debug = SAS.Debug;

namespace LevelStreaming
{
    public partial class RegionManager : MonoBehaviour
    {
        const string TAG = "RegionManager";

        [Serializable]
        public class Portal
        {
            [field: SerializeField] public string TargetRegionName { get; private set; }
            [field: SerializeField] public Bounds LocalBounds { get; set; }=new Bounds(Vector3.zero, Vector3.one);
        }

        public enum RegionType
        {
            Scene,
            Prefab
        }

        [Serializable]
        public partial class Region
        {
            [SerializeField] private SceneReference sceneRef;
            [SerializeField] private AssetReferenceGameObject prefabRef;
            [SerializeField] private string regionName;
            [SerializeField] private RegionType type;
            [SerializeField] private Bounds cachedBounds = new Bounds(Vector3.zero, Vector3.one*2);
            [SerializeField] private List<Portal> portals = new();
            [SerializeField] private UnloadStrategy unloadStrategy;

            public SceneReference SceneRef => sceneRef;
            public AssetReferenceGameObject PrefabRef => prefabRef;
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
                foreach (var p in Portals)
                {
                    var b = p.LocalBounds;
                    b.center += CachedBounds.center;
                    CachedWorldPortalBounds.Add(b);
                }
            }
        }

        [SerializeField] private List<Region> regions = new();
        public IReadOnlyList<Region> Regions => regions;
        [field: SerializeField] public RegionSelectionStrategySO RegionSelectionStrategy { get; private set; }
        public readonly HashSet<Region> loadedRegions = new();
        public Dictionary<string, Region> RegionLookup { get; private set; }
        private readonly Dictionary<Region, RegionMetaData> _metaByRegion = new();

        void Awake()
        {
            var regionNames = new HashSet<string>();
            foreach (var region in Regions)
            {
                if (string.IsNullOrEmpty(region.RegionName))
                {
                    Debug.LogWarning($"Region has no valid name: {region.Type}", this, TAG);
                    continue;
                }

                if (!regionNames.Add(region.RegionName))
                    Debug.LogWarning($"Duplicate region name detected: {region.RegionName}", this, TAG);

                GetOrCreateMeta(region);

                // Precompute portal world bounds
                region.RebuildPortalWorldBounds();
            }

            BuildLookup();
            if (RegionSelectionStrategy != null)
                RegionSelectionStrategy.Initialize(regions);
            else
                Debug.LogError("No RegionSelectionStrategy assigned!", this, TAG);
        }

        public void UpdateLoadedRegions(HashSet<Region> loadedRegions)
        {
            this.loadedRegions.Clear();
            this.loadedRegions.UnionWith(loadedRegions);
        }

        public List<Region> FindRegionsInRange(Bounds queryBounds)
        {
            if (RegionSelectionStrategy == null)
            {
                Debug.LogError("[RegionManager] Not initialized with a strategy!");
                return new List<Region>();
            }

            return RegionSelectionStrategy.GetNearbyRegions(queryBounds);
        }

        private void BuildLookup()
        {
            RegionLookup = new Dictionary<string, Region>();
            foreach (var region in Regions)
                RegionLookup[region.RegionName] = region;
        }

        public void MarkRegionLoaded(Region region)
        {
            if (region == null) return;

            var meta = GetOrCreateMeta(region);
            meta.IsLoading = false;
            meta.IsLoaded = true;
            meta.LoadedTime = Time.time;
            meta.LastTimeDesired = Time.time;
        }

        public void MarkRegionUnloaded(Region region)
        {
            if (region == null)
                return;

            if (_metaByRegion.TryGetValue(region, out var meta))
                meta.ResetTransient();
        }

        public class RegionMetaData
        {
            public bool IsLoading = false;
            public bool IsLoaded = false;
            public float LoadedTime = -1f;
            public GameObject Instance = null;

            public float LastTimeDesired = -1f;
            public object UserData = null;

            public void ResetTransient()
            {
                IsLoading = false;
                IsLoaded = false;
                LoadedTime = -1f;
                Instance = null;
                LastTimeDesired = -1f;
                UserData = null;
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
            return TryGetMeta(region, out var meta) && meta.IsLoaded;
        }

        public bool IsRegionLoading(Region region)
        {
            return TryGetMeta(region, out var meta) && meta.IsLoading;
        }
    }
}
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
            [field: SerializeField] public Bounds LocalBounds { get; set; }
        }

        public enum RegionType
        {
            Scene,
            Prefab
        }

        [Serializable]
        public partial class Region
        {
            [field: SerializeField] public RegionType Type { get; private set; } = RegionType.Scene;
            [field: SerializeField] public SceneReference SceneRef { get; private set; }
            [field: SerializeField] public AssetReferenceGameObject PrefabRef { get; private set; }
            [field: SerializeField] public Bounds CachedBounds { get; set; }
            [field: SerializeField] public string RegionName { get; private set; }
            [field: SerializeField] public List<Portal> Portals { get; private set; } = new();
            [field: SerializeField] public UnloadStrategy UnloadStrategy { get; private set; }
            [NonSerialized] public List<Bounds> CachedWorldPortalBounds = new();
            // --- Runtime state ---
            public bool IsLoading { get; internal set; }
            public bool IsLoaded { get; private set; }
            public float LoadedTime { get; private set; }

            /// <summary>
            /// If this region is prefab-based, this will store the instantiated root.
            /// Null if not loaded or if it's a scene region.
            /// </summary>
            public GameObject Instance { get; internal set; }


            internal void MarkLoaded()
            {
                IsLoaded = true;
                IsLoading = false;
                LoadedTime = Time.time;
            }

            internal void MarkUnloaded()
            {
                Instance = null;
                IsLoaded = false;
                IsLoading = false;
            }


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

        [field: SerializeField] public List<Region> Regions { get; private set; } = new();

        [field: SerializeField] public RegionSelectionStrategySO m_RegionSelectionStrategy;

        private readonly HashSet<Region> _loadedRegions = new();
        public Dictionary<string, Region> RegionLookup { get; private set; }

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

                region.RebuildPortalWorldBounds();
            }

            BuildLookup();
            if (m_RegionSelectionStrategy != null)
                m_RegionSelectionStrategy.Initialize(Regions);
            else
                Debug.LogError("No RegionSelectionStrategy assigned!", this, TAG);
        }

        public void UpdateLoadedRegions(HashSet<Region> loadedRegions)
        {
            _loadedRegions.Clear();
            _loadedRegions.UnionWith(loadedRegions);
        }

        public List<Region> FindRegionsInRange(Bounds queryBounds)
        {
            if (m_RegionSelectionStrategy == null)
            {
                Debug.LogError("[RegionManager] Not initialized with a strategy!");
                return new List<Region>();
            }

            return m_RegionSelectionStrategy.GetNearbyRegions(queryBounds);
        }

        private void BuildLookup()
        {
            RegionLookup = new Dictionary<string, Region>();
            foreach (var region in Regions)
                RegionLookup[region.RegionName] = region;
        }

        public void MarkRegionLoaded(Region region)
        {
            region.MarkLoaded();
        }

        public void MarkRegionUnloaded(Region region)
        {
            region.MarkUnloaded();
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public partial class RegionManager : MonoBehaviour
{
    public enum RegionType
    {
        Scene,
        Prefab
    }

    [Serializable]
    public partial class Region
    {
        public RegionType regionType = RegionType.Scene;
        public SceneReference sceneRef;
        public AssetReferenceGameObject prefabAddress;
        public Bounds cachedBounds;
        public UnloadStrategy unloadStrategy;

        [field: SerializeField, ReadOnly] 
        public string RegionName { get; private set; }
    }

    [field: SerializeField] 
    public List<Region> Regions { get; private set; } = new();

    [SerializeField] 
    private RegionSelectionStrategySO m_RegionSelectionStrategy;

    private readonly HashSet<Region> _loadedRegions = new();

    void Awake()
    {
        var seenNames = new HashSet<string>();
        foreach (var region in Regions)
        {
            if (string.IsNullOrEmpty(region.RegionName))
            {
                Debug.LogWarning($"[RegionManager] Region has no valid name: {region.regionType}");
                continue;
            }

            if (!seenNames.Add(region.RegionName))
                Debug.LogWarning($"[RegionManager] Duplicate region name detected: {region.RegionName}");
        }

        if (m_RegionSelectionStrategy != null)
            m_RegionSelectionStrategy.Initialize(Regions);
        else
            Debug.LogError("[RegionManager] No RegionSelectionStrategy assigned!");
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
}

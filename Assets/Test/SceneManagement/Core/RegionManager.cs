using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Debug = SAS.Debug;

public partial class RegionManager : MonoBehaviour
{
    const string TAG = "RegionManager";

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
        [field: SerializeField] public UnloadStrategy UnloadStrategy { get; private set; }
        [field: SerializeField, ReadOnly] public string RegionName { get; private set; }
    }

    [field: SerializeField] public List<Region> Regions { get; private set; } = new();

    [SerializeField] private RegionSelectionStrategySO m_RegionSelectionStrategy;

    private readonly HashSet<Region> _loadedRegions = new();

    void Awake()
    {
        var seenNames = new HashSet<string>();
        foreach (var region in Regions)
        {
            if (string.IsNullOrEmpty(region.RegionName))
            {
                Debug.LogWarning($"Region has no valid name: {region.Type}", this, TAG);
                continue;
            }

            if (!seenNames.Add(region.RegionName))
                Debug.LogWarning($"Duplicate region name detected: {region.RegionName}", this, TAG);
        }

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
}
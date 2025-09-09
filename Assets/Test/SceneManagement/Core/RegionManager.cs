using System;
using System.Collections.Generic;
using UnityEngine;

public partial class RegionManager : MonoBehaviour
{
    [Serializable]
    public partial class Region
    {
        public Bounds cachedBounds;
        public UnloadStrategy unloadStrategy;
        [field: SerializeField, ReadOnly] public string RegionName { get; private set; }
    }

    [field: SerializeField] public List<Region> Scenes { get; private set; } = new();
    [SerializeField] private SceneStreamingStrategySO m_StreamingStrategy;
    private readonly Dictionary<string, Region> _sceneLookup = new();
    private readonly HashSet<string> _loadedSceneNames = new();

    void Awake()
    {
        _sceneLookup.Clear();
        foreach (var s in Scenes)
        {
            if (string.IsNullOrEmpty(s.RegionName))
                _sceneLookup[s.RegionName] = s;
        }

        m_StreamingStrategy.Initialize(Scenes);
    }

    public void UpdateLoadedRegions(HashSet<string> loadedScenes)
    {
        _loadedSceneNames.Clear();
        _loadedSceneNames.UnionWith(loadedScenes);
    }

    public List<Region> FindRegionsInRange(Bounds queryBounds)
    {
        if (m_StreamingStrategy == null)
        {
            Debug.LogError("[SceneBoundsManager] Not initialized with a strategy!");
            return new List<Region>();
        }

        return m_StreamingStrategy.GetNearbyScenes(queryBounds);
    }

    public Region GetSceneRefByName(string sceneName)
    {
        _sceneLookup.TryGetValue(sceneName, out var sceneRef);
        return sceneRef;
    }
}
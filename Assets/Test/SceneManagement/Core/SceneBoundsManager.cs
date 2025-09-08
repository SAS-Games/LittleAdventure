using System;
using System.Collections.Generic;
using UnityEngine;

public partial class SceneBoundsManager : MonoBehaviour
{
    [Serializable]
    public partial class SceneRef
    {
        public Bounds cachedBounds;
        public UnloadStrategy unloadStrategy;

        [ReadOnly, SerializeField] private string sceneName;
        public string SceneName => sceneName;
    }

    [SerializeField] private float defaultCellSize = 100f;
    [field: SerializeField] public List<SceneRef> Scenes { get; private set; } = new();

    private ISceneStreamingStrategy _strategy;
    private readonly Dictionary<string, SceneRef> _sceneLookup = new();

    public void Initialize(ISceneStreamingStrategy chosenStrategy)
    {
        _strategy = chosenStrategy;
        _sceneLookup.Clear();
        foreach (var s in Scenes)
        {
            if (s.sceneAsset != null)
                _sceneLookup[s.sceneAsset.name] = s;
        }
    }

    public List<SceneRef> GetNearbyScenes(Bounds queryBounds)
    {
        if (_strategy == null)
        {
            Debug.LogError("[SceneBoundsManager] Not initialized with a strategy!");
            return new List<SceneRef>();
        }

        return _strategy.GetNearbyScenes(queryBounds);
    }

    public SceneRef GetSceneRefByName(string sceneName)
    {
        _sceneLookup.TryGetValue(sceneName, out var sceneRef);
        return sceneRef;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        foreach (var s in Scenes)
        {
            Gizmos.DrawWireCube(s.cachedBounds.center, s.cachedBounds.size);
        }
    }
}
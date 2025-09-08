using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BruteForceStreamingStrategy : ISceneStreamingStrategy
{
    private List<SceneBoundsManager.SceneRef> _allScenes;

    public void BuildIndex(List<SceneBoundsManager.SceneRef> scenes, float cellSize = 100f)
    {
        _allScenes = scenes;
    }

    public List<SceneBoundsManager.SceneRef> GetNearbyScenes(Bounds queryBounds)
    {
        var result = new List<SceneBoundsManager.SceneRef>();

        foreach (var scene in _allScenes)
        {
            if (scene.cachedBounds.Intersects(queryBounds))
                result.Add(scene);
        }

        return result;
    }

    public List<SceneBoundsManager.SceneRef> GetNearbyScenes(Vector3 position, float range)
    {
        var result = new List<SceneBoundsManager.SceneRef>();
        Bounds queryArea = new Bounds(position, new Vector3(range, range, range) * 2);

        foreach (var scene in _allScenes)
        {
            if (scene.cachedBounds.Intersects(queryArea))
                result.Add(scene);
        }

        return result;
    }
}
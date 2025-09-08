using System.Collections.Generic;
using UnityEngine;

public class QuadtreeStreamingStrategy : ISceneStreamingStrategy
{
    private QuadtreeNode root;
    private float worldSize = 1000f;  // adjust depending on your level size
    private int maxDepth = 6;
    private int maxCapacity = 4;

    public void BuildIndex(List<SceneBoundsManager.SceneRef> scenes, float cellSize)
    {
        // Find world bounds from scenes
        Bounds worldBounds = new Bounds(Vector3.zero, Vector3.one);
        foreach (var scene in scenes)
            worldBounds.Encapsulate(scene.cachedBounds);

        root = new QuadtreeNode(worldBounds, 0, maxDepth, maxCapacity);

        foreach (var scene in scenes)
            root.Insert(scene);
    }

    public List<SceneBoundsManager.SceneRef> GetNearbyScenes(Bounds queryBounds)
    {
        var results = new List<SceneBoundsManager.SceneRef>();
        root.Query(queryBounds, results);
        return results;
    }
}
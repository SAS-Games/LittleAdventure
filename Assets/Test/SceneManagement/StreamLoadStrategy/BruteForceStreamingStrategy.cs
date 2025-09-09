using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Streaming/Strategies/BruteForceStrategy")]
public class BruteForceStreamingStrategy : SceneStreamingStrategySO
{
    private List<RegionManager.Region> _sceneRefs;
    public override void Initialize(List<RegionManager.Region> sceneRefs)
    {
        _sceneRefs = sceneRefs;
    }

    public override List<RegionManager.Region> GetNearbyScenes(Bounds queryBounds)
    {
        var result = new List<RegionManager.Region>();

        foreach (var scene in _sceneRefs)
        {
            if (scene.cachedBounds.Intersects(queryBounds))
                result.Add(scene);
        }

        return result;
    }
}
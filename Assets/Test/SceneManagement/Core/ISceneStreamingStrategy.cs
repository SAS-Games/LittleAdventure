using System.Collections.Generic;
using UnityEngine;

public interface ISceneStreamingStrategy
{
    void Initialize(List<RegionManager.Region> sceneRefs);
    List<RegionManager.Region> GetNearbyScenes(Bounds queryBounds);
}

public abstract class SceneStreamingStrategySO : ScriptableObject, ISceneStreamingStrategy
{
    public abstract void Initialize(List<RegionManager.Region> sceneRefs);
    public abstract List<RegionManager.Region> GetNearbyScenes(Bounds queryBounds);
}
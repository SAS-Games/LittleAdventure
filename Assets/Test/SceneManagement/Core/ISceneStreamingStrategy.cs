using System.Collections.Generic;
using UnityEngine;

public interface ISceneStreamingStrategy
{
    List<SceneBoundsManager.SceneRef> GetNearbyScenes(Bounds queryBounds);
}
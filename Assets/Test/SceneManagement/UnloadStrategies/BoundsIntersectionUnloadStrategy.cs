using UnityEngine;

[CreateAssetMenu(menuName = "Streaming/UnloadStrategies/Bounds Intersection")]
public class BoundsIntersectionUnloadStrategy : UnloadStrategy
{
    public override bool ShouldUnload(Bounds unloadBounds, SceneBoundsManager.SceneRef sceneRef)
    {
        return !unloadBounds.Intersects(sceneRef.cachedBounds);
    }
}
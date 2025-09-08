using UnityEngine;

[CreateAssetMenu(menuName = "Streaming/UnloadStrategies/Never")]
public class NeverUnloadStrategy : UnloadStrategy
{
    public override bool ShouldUnload(Bounds unloadBounds, SceneBoundsManager.SceneRef sceneRef)
    {
        return false;
    }
}
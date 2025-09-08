using UnityEngine;

[CreateAssetMenu(menuName = "Streaming/UnloadStrategies/Composite")]
public class CompositeUnloadStrategy : UnloadStrategy
{
    public enum CombinationMode { AND, OR }

    [SerializeField] private CombinationMode mode = CombinationMode.AND;
    [SerializeField] private UnloadStrategy[] strategies;

    public override bool ShouldUnload(Bounds unloadBounds, SceneBoundsManager.SceneRef sceneRef)
    {
        if (strategies == null || strategies.Length == 0)
            return false;

        switch (mode)
        {
            case CombinationMode.AND:
                foreach (var strategy in strategies)
                {
                    if (strategy == null) continue;
                    if (!strategy.ShouldUnload(unloadBounds, sceneRef))
                        return false; // fail fast
                }
                return true;

            case CombinationMode.OR:
                foreach (var strategy in strategies)
                {
                    if (strategy == null) continue;
                    if (strategy.ShouldUnload(unloadBounds, sceneRef))
                        return true; // succeed fast
                }
                return false;

            default:
                return false;
        }
    }
}
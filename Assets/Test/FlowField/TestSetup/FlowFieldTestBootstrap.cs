using UnityEngine;

public class FlowFieldTestBootstrap : MonoBehaviour
{
    public FlowFieldAsset asset;
    public Transform target;

    private void Start()
    {
        if (FlowFieldManager.Instance == null)
        {
            Debug.LogError(
                "FlowFieldManager missing.");
            return;
        }

        FlowFieldManager.Instance.Rebuild(
            asset,
            target.position);
    }
}
using UnityEngine;

public sealed class TransformMotionHandle : MonoBehaviour
{
    private void OnDestroy()
    {
        if (TransformMotionSystem.Instance != null)
            TransformMotionSystem.Instance.Unregister(transform);
    }
}
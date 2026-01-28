using UnityEngine;

[DisallowMultipleComponent]
public sealed class RenderProxy : MonoBehaviour
{
    [SerializeField] private MeshRenderer m_MeshRenderer;

    private void OnEnable()
    {
        if (m_MeshRenderer == null)
            m_MeshRenderer = GetComponent<MeshRenderer>();
        if (m_MeshRenderer != null)
        {
            m_MeshRenderer.enabled = false;
            DynamicInstancedBatch.Instance?.Register(transform);
        }
    }

    private void OnDisable()
    {
        if (m_MeshRenderer != null)
            DynamicInstancedBatch.Instance?.Unregister(transform);
    }
}
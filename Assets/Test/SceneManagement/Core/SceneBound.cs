using UnityEngine;

[ExecuteAlways]
public class SceneBound : MonoBehaviour
{
    [SerializeField] private Bounds m_SceneBounds;

    public Bounds Bounds
    {
        get => m_SceneBounds;
        set => m_SceneBounds = value;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(m_SceneBounds.center, m_SceneBounds.size);
    }
#endif
}
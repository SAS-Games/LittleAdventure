using UnityEngine;

namespace LevelStreaming
{
    [ExecuteAlways]
    public class RegionBound : MonoBehaviour
    {
        [SerializeField] private Bounds m_RegionBounds;

        public Bounds Bounds
        {
            get => m_RegionBounds;
            set => m_RegionBounds = value;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(m_RegionBounds.center, m_RegionBounds.size);
        }
#endif
    }
}
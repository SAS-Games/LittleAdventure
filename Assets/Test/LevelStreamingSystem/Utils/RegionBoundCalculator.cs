using UnityEngine;

namespace LevelStreaming
{
    [ExecuteAlways]
    [RequireComponent(typeof(RegionBound))]
    public class RegionBoundCalculator : MonoBehaviour
    {
        [SerializeField] private bool m_IncludeInactive = true;
        [SerializeField] private bool m_UseRenderers = true;
        [SerializeField] private bool m_UseColliders = true;

        private RegionBound m_RegionBound;

        private void Awake()
        {
            m_RegionBound = GetComponent<RegionBound>();
        }

        /// <summary>
        /// Calculate bounds from children and update RegionBound.
        /// </summary>
        [ContextMenu("Recalculate Bounds")]
        public void RecalculateBounds()
        {
            var bounds = new Bounds(transform.position, Vector3.zero);
            bool hasBounds = false;

            // Collect bounds from renderers
            if (m_UseRenderers)
            {
                foreach (var renderer in GetComponentsInChildren<Renderer>(m_IncludeInactive))
                {
                    if (!hasBounds)
                    {
                        bounds = renderer.bounds;
                        hasBounds = true;
                    }
                    else
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }
                }
            }

            // Collect bounds from colliders
            if (m_UseColliders)
            {
                foreach (var col in GetComponentsInChildren<Collider>(m_IncludeInactive))
                {
                    if (!hasBounds)
                    {
                        bounds = col.bounds;
                        hasBounds = true;
                    }
                    else
                    {
                        bounds.Encapsulate(col.bounds);
                    }
                }

                foreach (var col2D in GetComponentsInChildren<Collider2D>(m_IncludeInactive))
                {
                    if (!hasBounds)
                    {
                        bounds = new Bounds(col2D.bounds.center, col2D.bounds.size);
                        hasBounds = true;
                    }
                    else
                    {
                        bounds.Encapsulate(col2D.bounds);
                    }
                }
            }

            if (!hasBounds)
            {
                Debug.LogWarning($"[{name}] No Renderers/Colliders found to calculate bounds.");
                return;
            }

            // Convert world-space bounds to local space
            var localCenter = transform.InverseTransformPoint(bounds.center);
            var localSize = transform.InverseTransformVector(bounds.size);

            m_RegionBound.Bounds = new Bounds(localCenter, localSize);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(m_RegionBound);
#endif
        }
    }
}

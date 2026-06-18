using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = SAS.Debug;

namespace LevelStreaming
{
    [ExecuteAlways]
    [RequireComponent(typeof(RegionBound))]
    public class RegionBoundCalculator : MonoBehaviour
    {
        public enum BoundsSource
        {
            ChildrenOnly,
            EntireScene
        }

        [Header("Source")]
        [SerializeField] private BoundsSource m_Source = BoundsSource.ChildrenOnly;

        [Header("Options")]
        [SerializeField] private bool m_IncludeInactive = true;
        [SerializeField] private bool m_UseRenderers = true;
        [SerializeField] private bool m_UseColliders = true;

        private RegionBound m_RegionBound;

        private void Awake()
        {
            m_RegionBound = GetComponent<RegionBound>();
        }

        // -----------------------------------------------------

        [ContextMenu("Recalculate Bounds")]
        public void RecalculateBounds()
        {
            if (m_RegionBound == null)
                m_RegionBound = GetComponent<RegionBound>();

            bool hasBounds = false;
            Bounds worldBounds = default;

            if (m_Source == BoundsSource.ChildrenOnly)
            {
                CollectFromChildren(ref worldBounds, ref hasBounds);
            }
            else
            {
                CollectFromScene(ref worldBounds, ref hasBounds);
            }

            if (!hasBounds)
            {
                Debug.LogWarning($"[{name}] No Renderers/Colliders found.");
                return;
            }

            // ✅ Correct world → local conversion
            var localCenter = transform.InverseTransformPoint(worldBounds.center);

            // IMPORTANT: size must ignore rotation scaling issues
            Vector3 localSize = worldBounds.size;
            localSize = new Vector3(
                localSize.x / transform.lossyScale.x,
                localSize.y / transform.lossyScale.y,
                localSize.z / transform.lossyScale.z
            );

            m_RegionBound.Bounds = new Bounds(localCenter, localSize);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(m_RegionBound);
#endif
        }

        // -----------------------------------------------------
        // CHILD COLLECTION
        // -----------------------------------------------------

        private void CollectFromChildren(ref Bounds bounds, ref bool hasBounds)
        {
            if (m_UseRenderers)
            {
                foreach (var r in GetComponentsInChildren<Renderer>(m_IncludeInactive))
                    Encapsulate(ref bounds, ref hasBounds, r.bounds);
            }

            if (m_UseColliders)
            {
                foreach (var c in GetComponentsInChildren<Collider>(m_IncludeInactive))
                    Encapsulate(ref bounds, ref hasBounds, c.bounds);

                foreach (var c2d in GetComponentsInChildren<Collider2D>(m_IncludeInactive))
                    Encapsulate(ref bounds, ref hasBounds, c2d.bounds);
            }
        }

        // -----------------------------------------------------
        // SCENE COLLECTION
        // -----------------------------------------------------

        private void CollectFromScene(ref Bounds bounds, ref bool hasBounds)
        {
            Scene scene = gameObject.scene;
            var roots = scene.GetRootGameObjects();

            foreach (var root in roots)
            {
                if (m_UseRenderers)
                {
                    foreach (var r in root.GetComponentsInChildren<Renderer>(m_IncludeInactive))
                        Encapsulate(ref bounds, ref hasBounds, r.bounds);
                }

                if (m_UseColliders)
                {
                    foreach (var c in root.GetComponentsInChildren<Collider>(m_IncludeInactive))
                        Encapsulate(ref bounds, ref hasBounds, c.bounds);

                    foreach (var c2d in root.GetComponentsInChildren<Collider2D>(m_IncludeInactive))
                        Encapsulate(ref bounds, ref hasBounds, c2d.bounds);
                }
            }
        }

        // -----------------------------------------------------

        private void Encapsulate(ref Bounds total, ref bool hasBounds, Bounds add)
        {
            if (!hasBounds)
            {
                total = add;
                hasBounds = true;
            }
            else
            {
                total.Encapsulate(add);
            }
        }
    }
}
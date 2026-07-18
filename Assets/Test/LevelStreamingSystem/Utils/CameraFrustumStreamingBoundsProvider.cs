using UnityEngine;

namespace LevelStreaming
{
    /// <summary>
    /// Produces world-axis-aligned broad-phase bounds around a camera-oriented box.
    /// The returned Bounds encloses the rotated box because Unity Bounds cannot store
    /// orientation.
    /// </summary>
    [DefaultExecutionOrder(-1), RequireComponent(typeof(Camera))]
    public class CameraFrustumStreamingBoundsProvider : MonoBehaviour, IStreamingBoundsProvider
    {
        [SerializeField] private RegionStreamingController m_Controller;

        [Header("Base Sizes")]
        [SerializeField] private Vector3 loadSize = new(30, 15, 30);
        [SerializeField] private Vector3 activateSize = new(15, 8, 15);
        [SerializeField] private Vector3 unloadSize = new(45, 20, 45);

        [Header("Forward Bias")]
        [Tooltip("How far the oriented volume is shifted along the camera forward axis.")]
        [SerializeField] private float forwardBias = 15f;

        [Tooltip("Scale applied along the camera-local forward axis.")]
        [SerializeField, Min(0.01f)] private float forwardScale = 1.5f;

        private void Awake()
        {
            if (m_Controller == null)
            {
                var controllers = FindObjectsByType<RegionStreamingController>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
                if (controllers.Length == 1)
                    m_Controller = controllers[0];
                else if (controllers.Length > 1)
                {
                    Debug.LogError(
                        "Multiple RegionStreamingControllers exist. Assign this bounds provider explicitly.",
                        this);
                }
            }

            if (m_Controller != null)
                m_Controller.SetRegionLoadBoundsProvider(this);
            else
                Debug.LogError("No RegionStreamingController was found for this bounds provider.", this);
        }

        public Bounds GetLoadBounds() => CreateWorldAabb(loadSize);
        public Bounds GetActivateBounds() => CreateWorldAabb(activateSize);
        public Bounds GetUnloadBounds() => CreateWorldAabb(unloadSize);

        private Bounds CreateWorldAabb(Vector3 localSize)
        {
            localSize = Sanitize(localSize);
            localSize.z *= Mathf.Max(0.01f, forwardScale);

            Vector3 localExtents = localSize * 0.5f;
            Transform cameraTransform = transform;
            Vector3 worldExtents =
                Abs(cameraTransform.right) * localExtents.x +
                Abs(cameraTransform.up) * localExtents.y +
                Abs(cameraTransform.forward) * localExtents.z;

            Vector3 center = cameraTransform.position + cameraTransform.forward * forwardBias;
            return new Bounds(center, worldExtents * 2f);
        }

        private static Vector3 Sanitize(Vector3 size)
        {
            return new Vector3(
                Mathf.Max(0.01f, Mathf.Abs(size.x)),
                Mathf.Max(0.01f, Mathf.Abs(size.y)),
                Mathf.Max(0.01f, Mathf.Abs(size.z)));
        }

        private static Vector3 Abs(Vector3 value)
        {
            return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            loadSize = Sanitize(loadSize);
            activateSize = Sanitize(activateSize);
            unloadSize = Sanitize(unloadSize);
            forwardScale = Mathf.Max(0.01f, forwardScale);
        }
#endif
    }
}

using UnityEngine;

namespace LevelStreaming
{
    [DefaultExecutionOrder(-1), RequireComponent(typeof(Camera))]
    public class CameraFrustumStreamingBoundsProvider : MonoBehaviour, IStreamingBoundsProvider
    {
        [Header("Base Sizes")]
        [SerializeField] Vector3 loadSize = new(30,15,30);
        [SerializeField] Vector3 activateSize = new(15,8,15);
        [SerializeField] Vector3 unloadSize = new(45,20,45);

        [Header("Forward Bias")]
        [Tooltip("How much bounds shift forward (in meters)")]
        [SerializeField] float forwardBias = 15f;

        [Tooltip("Extra scaling applied in forward direction")]
        [SerializeField] float forwardScale = 1.5f;

        Camera _cam;

        Bounds _load;
        Bounds _activate;
        Bounds _unload;

        void Awake()
        {
            FindFirstObjectByType<RegionStreamingController>().SetRegionLoadBoundsProvider(this);
            _cam = GetComponent<Camera>();
            UpdateSizes();
        }

        void UpdateSizes()
        {
            _load.size = ApplyForwardScale(loadSize);
            _activate.size = ApplyForwardScale(activateSize);
            _unload.size = ApplyForwardScale(unloadSize);
        }

        Vector3 ApplyForwardScale(Vector3 baseSize)
        {
            // stretch Z (forward axis)
            baseSize.z *= forwardScale;
            return baseSize;
        }

        Vector3 GetBiasedCenter()
        {
            if (_cam == null)
                return transform.position;

            var t = _cam.transform;

            // push bounds forward
            return t.position + t.forward * forwardBias;
        }

        public Bounds GetLoadBounds()
        {
            _load.center = GetBiasedCenter();
            return _load;
        }

        public Bounds GetActivateBounds()
        {
            _activate.center = GetBiasedCenter();
            return _activate;
        }

        public Bounds GetUnloadBounds()
        {
            _unload.center = GetBiasedCenter();
            return _unload;
        }
    }
}
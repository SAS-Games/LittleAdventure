using UnityEngine;

namespace LevelStreaming
{
    public enum StreamingViewMode
    {
        TargetOnly,
        TargetWithZoomMultiplier,
        CameraGroundFootprint,
        TargetAndCameraFootprint
    }

    public enum StreamingSpace
    {
        Full3D,
        GroundPlaneXZ,
        GroundPlaneXY
    }

    public enum StreamingZoomInput
    {
        None,
        CameraDistance,
        OrthographicSize,
        FieldOfView,
        CustomSource,
        Manual
    }

    /// <summary>
    /// Produces stable target- and camera-aware streaming bounds for open worlds.
    /// The provider predicts target motion, can include the camera footprint on a
    /// ground plane, expands immediately, and contracts gradually to avoid churn.
    /// </summary>
    [DefaultExecutionOrder(-100), DisallowMultipleComponent]
    public sealed class AdaptiveStreamingBoundsProvider : MonoBehaviour, IStreamingBoundsSnapshotProvider
    {
        [Header("Registration")]
        [SerializeField] private RegionStreamingController m_Controller;
        [SerializeField] private Transform m_Target;
        [SerializeField] private Camera m_Camera;
        [SerializeField] private bool m_UseSelfWhenTargetMissing = true;

        [Header("Coverage")]
        [SerializeField] private StreamingViewMode m_ViewMode = StreamingViewMode.TargetAndCameraFootprint;
        [SerializeField] private StreamingSpace m_StreamingSpace = StreamingSpace.GroundPlaneXZ;
        [SerializeField] private Vector3 m_ActivateSize = new(40f, 20f, 40f);
        [SerializeField] private Vector3 m_LoadSize = new(100f, 40f, 100f);
        [SerializeField] private Vector3 m_UnloadSize = new(140f, 60f, 140f);

        [Header("Motion Prediction")]
        [SerializeField] private Rigidbody m_TargetRigidbody;
        [SerializeField] private bool m_UseRigidbodyVelocity = true;
        [SerializeField, Min(0f)] private float m_PredictionTime = 1.5f;
        [SerializeField, Min(0f)] private float m_MaxPredictionDistance = 75f;
        [SerializeField, Min(0f)] private float m_MinPredictionSpeed = 0.25f;
        [Tooltip("Seconds used to smooth measured target velocity. Zero disables smoothing.")]
        [SerializeField, Min(0f)] private float m_VelocitySmoothingTime = 0.25f;
        [SerializeField, Min(0.01f)] private float m_TeleportDistance = 100f;

        [Header("Zoom")]
        [SerializeField] private StreamingZoomInput m_ZoomInput = StreamingZoomInput.CameraDistance;
        [Tooltip("A MonoBehaviour implementing IStreamingZoomSource.")]
        [SerializeField] private MonoBehaviour m_CustomZoomSource;
        [SerializeField] private Vector2 m_CameraDistanceRange = new(5f, 100f);
        [SerializeField] private Vector2 m_OrthographicSizeRange = new(5f, 100f);
        [SerializeField] private Vector2 m_FieldOfViewRange = new(30f, 90f);
        [SerializeField, Range(0f, 1f)] private float m_ManualZoom;
        [SerializeField] private AnimationCurve m_ActivateScaleByZoom = AnimationCurve.Linear(0f, 1f, 1f, 1.25f);
        [SerializeField] private AnimationCurve m_LoadScaleByZoom = AnimationCurve.Linear(0f, 1f, 1f, 2f);
        [SerializeField] private AnimationCurve m_UnloadScaleByZoom = AnimationCurve.Linear(0f, 1f, 1f, 2f);

        [Header("Camera Ground Footprint")]
        [SerializeField] private bool m_GroundPlaneFollowsTarget = true;
        [SerializeField] private float m_GroundPlaneHeight;
        [SerializeField, Min(1f)] private float m_MaxCameraRayDistance = 1000f;
        [SerializeField, Min(0.01f)] private float m_FootprintHeight = 40f;
        [SerializeField] private Vector3 m_ActivateFootprintPadding = new(5f, 0f, 5f);
        [SerializeField] private Vector3 m_LoadFootprintPadding = new(30f, 10f, 30f);
        [SerializeField] private Vector3 m_UnloadFootprintPadding = new(60f, 20f, 60f);

        [Header("Hysteresis")]
        [Tooltip("Units per second at which the load bounds may contract. Expansion is immediate.")]
        [SerializeField, Min(0f)] private float m_LoadShrinkSpeed = 80f;
        [Tooltip("Units per second at which the unload bounds may contract. Expansion is immediate.")]
        [SerializeField, Min(0f)] private float m_UnloadShrinkSpeed = 40f;

        [Header("Debug")]
        [SerializeField] private bool m_DrawGizmos = true;
        [SerializeField] private bool m_DrawCameraFootprint = true;

        private StreamingBoundsSnapshot _snapshot;
        private Bounds _lastCameraFootprint;
        private Vector3 _lastTargetPosition;
        private Vector3 _smoothedVelocity;
        private Vector3 _externalVelocity;
        private bool _hasSnapshot;
        private bool _hasTargetSample;
        private bool _hasExternalVelocity;
        private bool _hasCameraFootprint;
        private bool _reportedInvalidZoomSource;
        private uint _revision;

        public Transform Target => m_Target;
        public Vector3 SmoothedVelocity => _smoothedVelocity;
        public float NormalizedZoom => _hasSnapshot ? _snapshot.NormalizedZoom : EvaluateZoom();
        internal bool DrawGizmos => m_DrawGizmos;
        internal bool DrawCameraFootprint => m_DrawCameraFootprint;

        private void Awake()
        {
            ResolveReferences();
            ResetPrediction();
            RegisterWithController();
            Refresh(0f, true);
        }

        private void OnEnable()
        {
            ResetPrediction();
        }

        private void OnValidate()
        {
            ValidateConfiguration();
            if (Application.isPlaying)
                ResetPrediction();
        }

        private void Update()
        {
            Refresh(Time.deltaTime, false);
        }

        public void SetTarget(Transform target)
        {
            if (m_Target == target)
                return;

            m_Target = target;
            if (m_TargetRigidbody == null && target != null)
                m_TargetRigidbody = target.GetComponent<Rigidbody>();
            ResetPrediction();
            Refresh(0f, true);
        }

        public void SetController(RegionStreamingController controller)
        {
            m_Controller = controller;
            RegisterWithController();
        }

        public void SetManualZoom(float normalizedZoom)
        {
            m_ManualZoom = SanitizeNormalized(normalizedZoom);
        }

        /// <summary>
        /// Supplies authoritative world velocity, for example from a character or
        /// navigation controller. Call <see cref="ClearExternalVelocity"/> to resume
        /// Rigidbody or transform-derived velocity.
        /// </summary>
        public void SetVelocity(Vector3 worldVelocity)
        {
            _externalVelocity = IsFinite(worldVelocity) ? worldVelocity : Vector3.zero;
            _hasExternalVelocity = true;
        }

        public void ClearExternalVelocity()
        {
            _hasExternalVelocity = false;
        }

        public void Refresh()
        {
            Refresh(Time.deltaTime, false);
        }

        public void ResetPrediction()
        {
            _smoothedVelocity = Vector3.zero;
            _hasTargetSample = false;
            _hasSnapshot = false;
            _hasCameraFootprint = false;
        }

        public Bounds GetLoadBounds()
        {
            EnsureSnapshot();
            return _snapshot.Load;
        }

        public Bounds GetUnloadBounds()
        {
            EnsureSnapshot();
            return _snapshot.Unload;
        }

        public Bounds GetActivateBounds()
        {
            EnsureSnapshot();
            return _snapshot.Activate;
        }

        public bool TryGetSnapshot(out StreamingBoundsSnapshot snapshot)
        {
            EnsureSnapshot();
            snapshot = _snapshot;
            return _hasSnapshot;
        }

        private void EnsureSnapshot()
        {
            if (!_hasSnapshot)
                Refresh(0f, true);
        }

        private void Refresh(float deltaTime, bool forceSnap)
        {
            Transform observer = GetObserver();
            if (observer == null)
            {
                _hasSnapshot = false;
                return;
            }

            Vector3 observerPosition = observer.position;
            bool teleported = _hasTargetSample && Vector3.Distance(observerPosition, _lastTargetPosition) >= m_TeleportDistance;
            Vector3 rawVelocity = GetRawVelocity(observerPosition, Mathf.Max(0f, deltaTime));
            rawVelocity = ProjectToStreamingSpace(rawVelocity, m_StreamingSpace);

            if (teleported)
            {
                _smoothedVelocity = Vector3.zero;
                forceSnap = true;
            }
            else
            {
                float smoothingTime = Mathf.Max(0f, m_VelocitySmoothingTime);
                float alpha = smoothingTime <= 0.0001f || deltaTime <= 0f ? 1f : 1f - Mathf.Exp(-deltaTime / smoothingTime);
                _smoothedVelocity = Vector3.Lerp(_smoothedVelocity, rawVelocity, alpha);
            }

            _lastTargetPosition = observerPosition;
            _hasTargetSample = true;

            Vector3 prediction = AdaptiveStreamingBoundsMath.CalculatePrediction(_smoothedVelocity, m_PredictionTime, m_MinPredictionSpeed, m_MaxPredictionDistance);

            float zoom = EvaluateZoom();
            Vector3 predictedCenter = observerPosition + prediction;
            StreamingBoundsSnapshot desired = CreateDesiredSnapshot(observerPosition, predictedCenter, zoom);

            Bounds activate = desired.Activate;
            Bounds load = desired.Load;
            Bounds unload = desired.Unload;

            if (_hasSnapshot && !forceSnap)
            {
                load = ContractBounds(_snapshot.Load, load, m_LoadShrinkSpeed, deltaTime);
                unload = ContractBounds(_snapshot.Unload, unload, m_UnloadShrinkSpeed, deltaTime);
            }

            load = Encapsulate(load, activate);
            unload = Encapsulate(unload, load);

            _snapshot = new StreamingBoundsSnapshot(activate, load, unload, observerPosition, _smoothedVelocity, zoom, ++_revision);
            _hasSnapshot = true;
        }

        private StreamingBoundsSnapshot CreateDesiredSnapshot(Vector3 observerPosition, Vector3 predictedCenter, float zoom)
        {
            bool useZoom = m_ViewMode != StreamingViewMode.TargetOnly;
            float activateScale = useZoom ? EvaluateScale(m_ActivateScaleByZoom, zoom) : 1f;
            float loadScale = useZoom ? EvaluateScale(m_LoadScaleByZoom, zoom) : 1f;
            float unloadScale = useZoom ? EvaluateScale(m_UnloadScaleByZoom, zoom) : 1f;

            Vector3 activateSize = SanitizeSize(m_ActivateSize) * activateScale;
            Vector3 loadSize = Max(SanitizeSize(m_LoadSize) * loadScale, activateSize);
            Vector3 unloadSize = Max(SanitizeSize(m_UnloadSize) * unloadScale, loadSize);

            Bounds activate = new(predictedCenter, activateSize);
            Bounds load = new(predictedCenter, loadSize);
            Bounds unload = new(predictedCenter, unloadSize);

            bool wantsFootprint = m_ViewMode is StreamingViewMode.CameraGroundFootprint or StreamingViewMode.TargetAndCameraFootprint;
            bool wantsTarget = m_ViewMode != StreamingViewMode.CameraGroundFootprint;

            Bounds footprint = default;
            _hasCameraFootprint = wantsFootprint && TryCreateCameraFootprint(observerPosition, out footprint);
            if (_hasCameraFootprint)
            {
                _lastCameraFootprint = footprint;
                Bounds footprintActivate = Expanded(footprint, m_ActivateFootprintPadding);
                Bounds footprintLoad = Expanded(footprint, m_LoadFootprintPadding);
                Bounds footprintUnload = Expanded(footprint, m_UnloadFootprintPadding);

                if (wantsTarget)
                {
                    activate = Encapsulate(activate, footprintActivate);
                    load = Encapsulate(load, footprintLoad);
                    unload = Encapsulate(unload, footprintUnload);
                }
                else
                {
                    activate = footprintActivate;
                    load = footprintLoad;
                    unload = footprintUnload;
                }
            }

            // A missing camera must never leave a camera-only provider with invalid data.
            if (!wantsTarget && !_hasCameraFootprint)
            {
                activate = new Bounds(predictedCenter, activateSize);
                load = new Bounds(predictedCenter, loadSize);
                unload = new Bounds(predictedCenter, unloadSize);
            }

            load = Encapsulate(load, activate);
            unload = Encapsulate(unload, load);
            return new StreamingBoundsSnapshot(activate, load, unload, observerPosition, _smoothedVelocity, zoom, _revision + 1);
        }

        private Vector3 GetRawVelocity(Vector3 observerPosition, float deltaTime)
        {
            if (_hasExternalVelocity)
                return _externalVelocity;

            if (m_UseRigidbodyVelocity && m_TargetRigidbody != null)
                return m_TargetRigidbody.linearVelocity;

            if (!_hasTargetSample || deltaTime <= 0.000001f)
                return Vector3.zero;

            return (observerPosition - _lastTargetPosition) / deltaTime;
        }

        private float EvaluateZoom()
        {
            float value;
            switch (m_ZoomInput)
            {
                case StreamingZoomInput.CameraDistance:
                    if (m_Camera == null)
                        return 0f;
                    value = Vector3.Distance(m_Camera.transform.position, GetObserverPosition());
                    return AdaptiveStreamingBoundsMath.Normalize(value, m_CameraDistanceRange);

                case StreamingZoomInput.OrthographicSize:
                    return m_Camera != null ? AdaptiveStreamingBoundsMath.Normalize(m_Camera.orthographicSize, m_OrthographicSizeRange) : 0f;

                case StreamingZoomInput.FieldOfView:
                    return m_Camera != null ? AdaptiveStreamingBoundsMath.Normalize(m_Camera.fieldOfView, m_FieldOfViewRange) : 0f;

                case StreamingZoomInput.CustomSource:
                    if (m_CustomZoomSource is IStreamingZoomSource source)
                        return SanitizeNormalized(source.NormalizedZoom);

                    if (!_reportedInvalidZoomSource)
                    {
                        Debug.LogWarning("The custom zoom source must implement IStreamingZoomSource. Zoom defaults to zero.", this);
                        _reportedInvalidZoomSource = true;
                    }
                    return 0f;

                case StreamingZoomInput.Manual:
                    return SanitizeNormalized(m_ManualZoom);

                default:
                    return 0f;
            }
        }

        private bool TryCreateCameraFootprint(Vector3 observerPosition, out Bounds footprint)
        {
            footprint = default;
            if (m_Camera == null)
                return false;

            // When the camera itself is the fallback observer, a plane at the
            // observer height would intersect every viewport ray at its origin.
            float planeHeight = m_GroundPlaneFollowsTarget && m_Target != null
                ? observerPosition.y
                : m_GroundPlaneHeight;
            return AdaptiveStreamingBoundsMath.TryCreateGroundFootprint(
                m_Camera,
                planeHeight,
                m_MaxCameraRayDistance,
                m_FootprintHeight,
                out footprint);
        }

        private Transform GetObserver()
        {
            if (m_Target != null)
                return m_Target;
            return m_UseSelfWhenTargetMissing ? transform : null;
        }

        private Vector3 GetObserverPosition()
        {
            Transform observer = GetObserver();
            return observer != null ? observer.position : transform.position;
        }

        private void ResolveReferences()
        {
            if (m_Camera == null)
                m_Camera = GetComponent<Camera>();
            if (m_Camera == null)
                m_Camera = Camera.main;

            if (m_TargetRigidbody == null && m_Target != null)
                m_TargetRigidbody = m_Target.GetComponent<Rigidbody>();

            if (m_Controller != null)
                return;

            var controllers = FindObjectsByType<RegionStreamingController>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            if (controllers.Length == 1)
                m_Controller = controllers[0];
            else if (controllers.Length > 1)
                Debug.LogError(
                    "Multiple RegionStreamingControllers exist. Assign the adaptive bounds provider explicitly.",
                    this);
        }

        private void RegisterWithController()
        {
            if (m_Controller != null)
                m_Controller.SetRegionLoadBoundsProvider(this);
            else
                Debug.LogError("No RegionStreamingController was found for this bounds provider.", this);
        }

        private static float EvaluateScale(AnimationCurve curve, float zoom)
        {
            float value = curve != null ? curve.Evaluate(zoom) : 1f;
            return float.IsFinite(value) ? Mathf.Max(0.01f, value) : 1f;
        }

        private static float SanitizeNormalized(float value)
        {
            return float.IsFinite(value) ? Mathf.Clamp01(value) : 0f;
        }

        internal static Vector3 ProjectToStreamingSpace(Vector3 value, StreamingSpace space)
        {
            return space switch
            {
                StreamingSpace.GroundPlaneXZ => new Vector3(value.x, 0f, value.z),
                StreamingSpace.GroundPlaneXY => new Vector3(value.x, value.y, 0f),
                _ => value
            };
        }

        internal static Bounds ContractBounds(Bounds current, Bounds desired, float speed, float deltaTime)
        {
            float step = Mathf.Max(0f, speed) * Mathf.Max(0f, deltaTime);
            Vector3 min = ContractFace(current.min, desired.min, step, true);
            Vector3 max = ContractFace(current.max, desired.max, step, false);
            return CreateFromMinMax(min, max);
        }

        private static Vector3 ContractFace(Vector3 current, Vector3 desired, float step, bool minimumFace)
        {
            for (int axis = 0; axis < 3; axis++)
            {
                bool expands = minimumFace ? desired[axis] < current[axis] : desired[axis] > current[axis];
                current[axis] = expands
                    ? desired[axis]
                    : Mathf.MoveTowards(current[axis], desired[axis], step);
            }
            return current;
        }

        internal static Bounds Encapsulate(Bounds outer, Bounds inner)
        {
            outer.Encapsulate(inner.min);
            outer.Encapsulate(inner.max);
            return outer;
        }

        private static Bounds Expanded(Bounds bounds, Vector3 padding)
        {
            padding = new Vector3(
                Mathf.Max(0f, padding.x),
                Mathf.Max(0f, padding.y),
                Mathf.Max(0f, padding.z));
            bounds.Expand(padding * 2f);
            return bounds;
        }

        private static Bounds CreateFromMinMax(Vector3 min, Vector3 max)
        {
            var bounds = new Bounds();
            bounds.SetMinMax(Vector3.Min(min, max), Vector3.Max(min, max));
            return bounds;
        }

        private static Vector3 SanitizeSize(Vector3 size)
        {
            return new Vector3(
                Mathf.Max(0.01f, Mathf.Abs(size.x)),
                Mathf.Max(0.01f, Mathf.Abs(size.y)),
                Mathf.Max(0.01f, Mathf.Abs(size.z)));
        }

        private static Vector3 Max(Vector3 a, Vector3 b)
        {
            return new Vector3(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y), Mathf.Max(a.z, b.z));
        }

        private static bool IsFinite(Vector3 value)
        {
            return float.IsFinite(value.x) && float.IsFinite(value.y) && float.IsFinite(value.z);
        }

        internal void ValidateConfiguration()
        {
            m_ActivateSize = SanitizeSize(m_ActivateSize);
            m_LoadSize = Max(SanitizeSize(m_LoadSize), m_ActivateSize);
            m_UnloadSize = Max(SanitizeSize(m_UnloadSize), m_LoadSize);
            m_PredictionTime = Mathf.Max(0f, m_PredictionTime);
            m_MaxPredictionDistance = Mathf.Max(0f, m_MaxPredictionDistance);
            m_MinPredictionSpeed = Mathf.Max(0f, m_MinPredictionSpeed);
            m_VelocitySmoothingTime = Mathf.Max(0f, m_VelocitySmoothingTime);
            m_TeleportDistance = Mathf.Max(0.01f, m_TeleportDistance);
            m_MaxCameraRayDistance = Mathf.Max(1f, m_MaxCameraRayDistance);
            m_FootprintHeight = Mathf.Max(0.01f, m_FootprintHeight);
            m_LoadShrinkSpeed = Mathf.Max(0f, m_LoadShrinkSpeed);
            m_UnloadShrinkSpeed = Mathf.Max(0f, m_UnloadShrinkSpeed);
            m_ManualZoom = SanitizeNormalized(m_ManualZoom);

            if (m_Camera == null)
                m_Camera = GetComponent<Camera>();
        }

        internal bool TryGetDebugBounds(out StreamingBoundsSnapshot sample,
            out Bounds cameraFootprint, out bool hasCameraFootprint)
        {
            if (_hasSnapshot)
            {
                sample = _snapshot;
            }
            else
            {
                if (Application.isPlaying)
                {
                    sample = default;
                    cameraFootprint = default;
                    hasCameraFootprint = false;
                    return false;
                }

                if (m_Camera == null)
                    m_Camera = GetComponent<Camera>();

                Vector3 position = GetObserverPosition();
                sample = CreateDesiredSnapshot(position, position, EvaluateZoom());
            }

            cameraFootprint = _lastCameraFootprint;
            hasCameraFootprint = _hasCameraFootprint;
            return true;
        }
    }

    internal static class AdaptiveStreamingBoundsMath
    {
        private static readonly Vector3[] ViewportCorners =
        {
            new(0f, 0f, 0f),
            new(0f, 1f, 0f),
            new(1f, 0f, 0f),
            new(1f, 1f, 0f)
        };

        public static float Normalize(float value, Vector2 range)
        {
            float min = Mathf.Min(range.x, range.y);
            float max = Mathf.Max(range.x, range.y);
            if (max - min <= 0.0001f)
                return value >= max ? 1f : 0f;
            return Mathf.InverseLerp(min, max, value);
        }

        public static Vector3 CalculatePrediction(Vector3 velocity, float predictionTime,
            float minimumSpeed, float maximumDistance)
        {
            if (velocity.magnitude < Mathf.Max(0f, minimumSpeed))
                return Vector3.zero;

            Vector3 prediction = velocity * Mathf.Max(0f, predictionTime);
            return Vector3.ClampMagnitude(prediction, Mathf.Max(0f, maximumDistance));
        }

        public static bool TryCreateGroundFootprint(Camera camera, float planeHeight,
            float maxRayDistance, float height, out Bounds footprint)
        {
            footprint = default;
            if (camera == null || maxRayDistance <= 0f || height <= 0f)
                return false;

            var plane = new Plane(Vector3.up, new Vector3(0f, planeHeight, 0f));
            bool initialized = false;
            Vector3 min = default;
            Vector3 max = default;

            foreach (Vector3 corner in ViewportCorners)
            {
                Ray ray = camera.ViewportPointToRay(corner);
                Vector3 point;
                if (plane.Raycast(ray, out float distance) && distance >= 0f && distance <= maxRayDistance)
                {
                    point = ray.GetPoint(distance);
                }
                else
                {
                    // Looking toward or above the horizon must remain bounded.
                    point = ray.GetPoint(maxRayDistance);
                    point.y = planeHeight;
                }

                point.y = planeHeight;
                if (!initialized)
                {
                    min = point;
                    max = point;
                    initialized = true;
                }
                else
                {
                    min = Vector3.Min(min, point);
                    max = Vector3.Max(max, point);
                }
            }

            if (!initialized)
                return false;

            min.y = planeHeight - height * 0.5f;
            max.y = planeHeight + height * 0.5f;
            footprint.SetMinMax(min, max);
            return true;
        }
    }
}

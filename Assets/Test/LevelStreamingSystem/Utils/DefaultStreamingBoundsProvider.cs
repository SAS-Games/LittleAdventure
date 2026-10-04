using UnityEngine;

namespace LevelStreaming
{
    public class DefaultStreamingBoundsProvider : MonoBehaviour, IStreamingVolumeProvider
    {
        [Header("Load/Unload Settings")] [SerializeField]
        private Vector3 m_LoadBoundsSize = new(20, 10, 20);

        [SerializeField] private Vector3 m_ActivateBoundsSize = new(10, 5, 10);
        [SerializeField] private Vector3 m_UnloadBoundsSize = new(30, 15, 30);

        private readonly BoxStreamingVolume _loadVolume = new(default);
        private readonly BoxStreamingVolume _activateVolume = new(default);
        private readonly BoxStreamingVolume _unloadVolume = new(default);
        private uint _revision;

        // AABB implementation data
        private Bounds _loadBounds;
        private Bounds _activateBounds;
        private Bounds _unloadBounds;

        private void Awake()
        {
            UpdateCachedBounds();
        }

        private void OnValidate()
        {
            m_LoadBoundsSize = Sanitize(m_LoadBoundsSize);
            m_ActivateBoundsSize = Sanitize(m_ActivateBoundsSize);
            m_UnloadBoundsSize = Sanitize(m_UnloadBoundsSize);
            UpdateCachedBounds();
        }

        private static Vector3 Sanitize(Vector3 size)
        {
            return new Vector3(
                Mathf.Max(0.01f, Mathf.Abs(size.x)),
                Mathf.Max(0.01f, Mathf.Abs(size.y)),
                Mathf.Max(0.01f, Mathf.Abs(size.z)));
        }

        private void UpdateCachedBounds()
        {
            _loadBounds.size = m_LoadBoundsSize;
            _activateBounds.size = m_ActivateBoundsSize;
            _unloadBounds.size = m_UnloadBoundsSize;

            _loadBounds.center = transform.position;
            _activateBounds.center = transform.position;
            _unloadBounds.center = transform.position;
        }

        public bool TryGetVolumes(out StreamingVolumeSnapshot snapshot)
        {
            UpdateCachedBounds();
            _loadVolume.Bounds = _loadBounds;
            _activateVolume.Bounds = _activateBounds;
            _unloadVolume.Bounds = _unloadBounds;
            snapshot = new StreamingVolumeSnapshot(_activateVolume, _loadVolume,
                _unloadVolume, transform.position, revision: ++_revision);
            return true;
        }
    }
}

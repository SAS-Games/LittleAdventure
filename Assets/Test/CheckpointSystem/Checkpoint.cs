using System;
using System.Threading.Tasks;
using SAS.Core.TagSystem;
using UnityEngine;

namespace SAS.Checkpoints
{
    [DisallowMultipleComponent]
    public sealed class Checkpoint : MonoBehaviour
    {
        [Header("Definition")]
        [SerializeField] private CheckpointDefinition m_Definition;
        [Header("Activation")]
        [SerializeField] private bool m_CompleteOnlyOnce = true;
        [SerializeField] private bool m_AllowBackwardActivation;
        [Header("Spawn")]
        [SerializeField] private SpawnPointGroup m_SpawnPointGroup;
        [SerializeField] private Transform m_FallbackSpawnPoint;
        [Header("Visual State")]
        [SerializeField] private GameObject[] m_ActiveObjects;
        [SerializeField] private GameObject[] m_InactiveObjects;
        [Inject] private ICheckpointManager _checkpointManager;
        [Inject] private ICheckpointProgressService _progressService;

        public event Action<Checkpoint> Activated;
        public event Action<Checkpoint> Deactivated;

        public CheckpointDefinition Definition => m_Definition;
        public string Id => m_Definition?.Id;
        public int Order => m_Definition?.Order ?? 0;
        public bool IsValid => m_Definition != null && m_Definition.IsValid;
        public bool CompleteOnlyOnce => m_CompleteOnlyOnce;
        public bool AllowBackwardActivation => m_AllowBackwardActivation;
        public SpawnPointGroup SpawnPointGroup => m_SpawnPointGroup;

        public bool IsActive => _checkpointManager != null && _checkpointManager.IsActive(this);

        public Vector3 FallbackPosition
        {
            get
            {
                Transform target = m_FallbackSpawnPoint != null ? m_FallbackSpawnPoint : transform;
                return target.position;
            }
        }

        public Quaternion FallbackRotation
        {
            get
            {
                Transform target = m_FallbackSpawnPoint != null ? m_FallbackSpawnPoint : transform;
                return target.rotation;
            }
        }

        private void Awake()
        {
            this.InjectFieldBindings();
            ResolveSpawnPointGroup();
            ValidateConfiguration();
        }

        private void OnEnable()
        {
            if (_checkpointManager == null)
                return;

            _checkpointManager.ActiveCheckpointChanged += OnActiveCheckpointChanged;
            _checkpointManager.RegisterCheckpoint(this);
            RefreshVisualState();
        }

        private void OnDisable()
        {
            if (_checkpointManager == null)
                return;

            _checkpointManager.ActiveCheckpointChanged -= OnActiveCheckpointChanged;
            _checkpointManager.UnregisterCheckpoint(this);
        }

        public async void Complete()
        {
            try
            {
                await CompleteAsync();
            }
            catch (Exception exception)
            {
                Debug.LogError($"Checkpoint '{name}' activation failed.\n{exception}", this);
            }
        }

        public async Task<bool> CompleteAsync()
        {
            if (_checkpointManager == null)
            {
                Debug.LogError($"{nameof(Checkpoint)} '{name}' is not initialized.", this);
                return false;
            }

            if (!CanComplete())
                return false;

            return await _checkpointManager.ActivateAsync(this);
        }

        public ActiveCheckpointData CreateProgressData()
        {
            if (!IsValid)
            {
                throw new InvalidOperationException($"Checkpoint '{name}' has an invalid definition.");
            }

            return new ActiveCheckpointData(Id, gameObject.scene.name, m_SpawnPointGroup != null ? m_SpawnPointGroup.SpawnPointGroupId : null, FallbackPosition, FallbackRotation);
        }

        private bool CanComplete()
        {
            if (!IsValid)
            {
                Debug.LogError($"Checkpoint '{name}' has an invalid definition.", this);
                return false;
            }

            if (IsActive)
                return false;

            if (_progressService == null)
            {
                Debug.LogError($"Checkpoint '{name}' has no progress service.", this);
                return false;
            }

            if (!_progressService.IsInitialized)
            {
                Debug.LogError($"Checkpoint '{name}' cannot activate before checkpoint progress is initialized.", this);
                return false;
            }

            if (!_checkpointManager.CanActivate(this))
                return false;

            if (m_CompleteOnlyOnce)
            {
                if (_progressService.IsCompleted(Id) && !m_AllowBackwardActivation)
                    return false;
            }

            return true;
        }

        private void OnActiveCheckpointChanged(Checkpoint previousCheckpoint, Checkpoint currentCheckpoint)
        {
            if (previousCheckpoint == this)
                Deactivated?.Invoke(this);

            if (currentCheckpoint == this)
                Activated?.Invoke(this);

            RefreshVisualState();
        }

        private void RefreshVisualState()
        {
            bool active = IsActive;

            SetObjectsActive(m_ActiveObjects, active);
            SetObjectsActive(m_InactiveObjects, !active);
        }

        private static void SetObjectsActive(GameObject[] objects, bool active)
        {
            if (objects == null)
                return;

            foreach (GameObject target in objects)
            {
                if (target != null)
                    target.SetActive(active);
            }
        }

        private void ValidateConfiguration()
        {
            if (m_Definition == null)
            {
                Debug.LogError($"Checkpoint '{name}' has no definition.", this);
                return;
            }

            if (!m_Definition.IsValid)
                Debug.LogError($"Checkpoint '{name}' has an empty ID.", this);

            if (m_SpawnPointGroup == null)
                Debug.LogWarning($"Checkpoint '{name}' does not have spawn-point group. The fallback transform will be used.", this);
        }

        private void ResolveSpawnPointGroup()
        {
            if (m_SpawnPointGroup == null)
                m_SpawnPointGroup = GetComponentInChildren<SpawnPointGroup>(true);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResolveSpawnPointGroup();

            if (m_Definition == null)
                Debug.LogWarning($"Checkpoint '{name}' has no definition.", this);
            else if (!m_Definition.IsValid)
                Debug.LogWarning($"Checkpoint '{name}' has an empty ID.", this);

            if (m_SpawnPointGroup == null)
                Debug.LogWarning($"Checkpoint '{name}' has no spawn-point group; " + "respawning will use its fallback transform.", this);
        }

        private void OnDrawGizmosSelected()
        {
            Transform target = m_FallbackSpawnPoint != null ? m_FallbackSpawnPoint : transform;
            Gizmos.DrawWireSphere(target.position, 0.4f);
            Gizmos.DrawLine(target.position, target.position + target.forward);
        }
#endif
    }
}

using SAS.Core.TagSystem;
using UnityEngine;

namespace SAS.Checkpoints
{
    [DisallowMultipleComponent]
    public sealed class SpawnPointGroup : MonoBehaviour
    {
        [field: SerializeField] public string SpawnPointGroupId { get; private set; } = "SpawnPointGroup";
        [field: SerializeField] public bool IsDefault { get; private set; }
        [SerializeField] private SpawnPoint[] m_SpawnPoints;

        [Inject] private ICheckpointManager _checkpointManager;

        private void Awake()
        {
            this.InjectFieldBindings();

            if (m_SpawnPoints == null || m_SpawnPoints.Length == 0)
                m_SpawnPoints = GetComponentsInChildren<SpawnPoint>(true);
        }

        private void OnEnable()
        {
            if (_checkpointManager == null)
            {
                Debug.LogError($"Spawn-point group '{name}' has no checkpoint manager.", this);
                return;
            }

            _checkpointManager.RegisterGroup(this);
        }

        private void OnDisable()
        {
            if (_checkpointManager != null)
                _checkpointManager.UnregisterGroup(this);
        }

        public bool TryGetByPlayerId(int playerId, out SpawnPoint spawnPoint)
        {
            spawnPoint = null;

            if (m_SpawnPoints == null || m_SpawnPoints.Length == 0)
                return false;

            int index = PositiveModulo(playerId, m_SpawnPoints.Length);
            spawnPoint = m_SpawnPoints[index];
            return spawnPoint != null;
        }

        public bool TryGetAvailableByPlayerId(int playerId, out SpawnPoint spawnPoint)
        {
            spawnPoint = null;

            if (m_SpawnPoints == null || m_SpawnPoints.Length == 0)
                return false;

            int startIndex = PositiveModulo(
                playerId,
                m_SpawnPoints.Length);

            for (int offset = 0; offset < m_SpawnPoints.Length; offset++)
            {
                int index = (startIndex + offset) % m_SpawnPoints.Length;
                SpawnPoint point = m_SpawnPoints[index];

                if (point != null && !point.IsOccupied)
                {
                    spawnPoint = point;
                    return true;
                }
            }

            return false;
        }

        private static int PositiveModulo(int value, int divisor)
        {
            int remainder = value % divisor;
            return remainder < 0 ? remainder + divisor : remainder;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(SpawnPointGroupId))
            {
                Debug.LogWarning($"Spawn-point group '{name}' has an empty ID.", this);
            }

            if (m_SpawnPoints == null || m_SpawnPoints.Length == 0)
                m_SpawnPoints = GetComponentsInChildren<SpawnPoint>(true);

            for (int index = 0; index < m_SpawnPoints.Length; index++)
            {
                SpawnPoint point = m_SpawnPoints[index];

                if (point == null)
                {
                    Debug.LogWarning($"Spawn-point group '{name}' contains a null " + $"entry at index {index}.", this);
                    continue;
                }

                for (int previous = 0; previous < index; previous++)
                {
                    if (m_SpawnPoints[previous] == point)
                    {
                        Debug.LogWarning($"Spawn-point group '{name}' contains the " + $"same spawn point more than once: " + $"'{point.name}'.", this);
                        break;
                    }
                }
            }
        }
#endif
    }
}

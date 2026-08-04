using UnityEngine;
using UnityEngine.Events;

public class SpawnerGroup : MonoBehaviour
{
    private int _activeSpawners;
    [SerializeField] private UnityEvent m_OnAllCleared;
    [SerializeField] private Spawner[] m_Spawners;

    private void Awake()
    {
        if (m_Spawners == null || m_Spawners.Length == 0)
        {
            Debug.LogWarning($"SpawnerGroup {name} has no spawners assigned!");
            return;
        }

        _activeSpawners = m_Spawners.Length;

        foreach (var spawner in m_Spawners)
        {
            spawner.OnAllSpawnedObjectsCollected += HandleSpawnerCleared;
        }
    }

    private void HandleSpawnerCleared()
    {
        _activeSpawners--;

        if (_activeSpawners <= 0)
        {
            m_OnAllCleared?.Invoke();
            Debug.Log($"All spawners in {name} are cleared!");
        }
    }

    private void OnDestroy()
    {
        foreach (var spawner in m_Spawners)
        {
            spawner.OnAllSpawnedObjectsCollected -= HandleSpawnerCleared;
        }
    }
}

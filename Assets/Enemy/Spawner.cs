using SAS.Pool;
using SAS.Utilities.TagSystem;
using UnityEngine;

[RequireComponent(typeof(OnTriggerHandler))]
public class Spawner : MonoBehaviour
{
    [SerializeField] private SpawnablePoolSO m_PoolSO;
    [FieldRequiresChild] private SpawnPoint[] _spawnPoints;
    [FieldRequiresSelf] Collider _collider;
    private bool _hasSpawned;

    private void Awake()
    {
        this.Initialize();
    }

    private void Spawn()
    {
        if (_hasSpawned)
            return;

        _hasSpawned = true;

        foreach (SpawnPoint point in _spawnPoints)
        {
            if (point.SpawnedObject == null)
                m_PoolSO.Spawn(point);
        }
    }

    private void OnDrawGizmos()
    {
        if (_collider == null)
            _collider = GetComponent<Collider>();
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, _collider.bounds.size);
    }
}

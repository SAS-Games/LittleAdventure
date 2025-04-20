using SAS.Pool;
using SAS.Utilities.TagSystem;
using System;
using UnityEngine;
public struct SpawnData
{
    public SpawnPoint Point;
    public Action<GameObject> OnDespawn;

    public SpawnData(SpawnPoint point, Action<GameObject> onDespawn)
    {
        Point = point;
        OnDespawn = onDespawn;
    }
}


[RequireComponent(typeof(OnTriggerHandler))]
public class Spawner : MonoBehaviour
{
    [SerializeField] private SpawnablePoolSO m_PoolSO;
    [FieldRequiresChild] private SpawnPoint[] _spawnPoints;
    [FieldRequiresSelf] Collider _collider;
    private bool _hasSpawned;

    public Action OnAllSpawnedObjectsCollected { get; set; }

    private void Awake()
    {
        this.Initialize();
    }

    //this will get invoked by TriggerHandler
    private void Spawn()
    {
        if (_hasSpawned)
            return;

        _hasSpawned = true;
        gameObject.SetActive(false);

        foreach (SpawnPoint point in _spawnPoints)
        {
            if (point.SpawnedObject == null)
            {
                var spawnData = new SpawnData(point, OnObjectDespawned);
                m_PoolSO.Spawn(spawnData);
            }
        }
    }

    private void OnObjectDespawned(GameObject gameObject)
    {
        Debug.Log($"{gameObject.name} has been despawned from Spawner {name}.");

        // Check if all objects in the Spawner are gone
        bool allDespawned = true;
        foreach (SpawnPoint point in _spawnPoints)
        {
            if (point.SpawnedObject != null)
            {
                allDespawned = false;
                break;
            }
        }

        if (allDespawned)
        {
            Debug.Log($"All objects in  Spawner {name} have been eliminated!");
            OnAllSpawnedObjectsCollected?.Invoke();
        }
    }

    private void OnDrawGizmos()
    {
        if (_collider == null)
            _collider = GetComponent<Collider>();
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position-_collider.bounds.center, _collider.bounds.size);
    }
}

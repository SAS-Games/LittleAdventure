using EnemySystem;
using UnityEngine;

public class EnemySpawner : Spawner
{
    [SerializeField] private EnemyTargetingSystem _targetingSystem;

    protected override void OnObjectsSpawned(GameObject gameObject)
    {
        var enemy = gameObject.GetComponent<Enemy>();
        if (enemy)
            _targetingSystem.RegisterEnemy(enemy);
    }

    protected override void OnObjectDespawned(GameObject gameObject)
    {
        var enemy = gameObject.GetComponent<Enemy>();
        if (enemy)
            _targetingSystem.UnregisterEnemy(enemy);
        base.OnObjectDespawned(gameObject);
    }
}
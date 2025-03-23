using SAS.Pool;
using UnityEngine;

public class ObjectPositionSetter : MonoBehaviour, ISpawnable
{
    private SpawnPoint _spawnPoint;
    void ISpawnable.OnSpawn(object data)
    {
        if (data is SpawnPoint point)
        {
            _spawnPoint = point;
            this.transform.position = point.transform.position;
            this.transform.rotation = point.transform.rotation;
            point.SpawnedObject = this.gameObject;
        }
    }

    void ISpawnable.OnDespawn()
    {
        _spawnPoint.SpawnedObject = null;
        _spawnPoint = null;
    }
}

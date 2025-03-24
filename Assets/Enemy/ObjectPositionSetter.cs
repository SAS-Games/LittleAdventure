using SAS.Pool;
using UnityEngine;

public class ObjectPositionSetter : MonoBehaviour, ISpawnable
{
    private SpawnData _spawnData;

    void ISpawnable.OnSpawn(object data)
    {
        if (data is SpawnData spawnData)
        {
            _spawnData = spawnData;

            this.transform.SetPositionAndRotation(spawnData.Point.transform.position, spawnData.Point.transform.rotation);
            spawnData.Point.SpawnedObject = this.gameObject;
        }
    }

    void ISpawnable.OnDespawn()
    {
        _spawnData.Point.SpawnedObject = null;
        _spawnData.OnDespawn?.Invoke(gameObject);
        _spawnData = default;
    }
}

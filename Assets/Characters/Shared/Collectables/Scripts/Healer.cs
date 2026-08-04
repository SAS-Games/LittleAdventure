using SAS.Collectables;
using SAS.Pool;
using UnityEngine;

public class Healer : BaseCollectable<Healer>, ISpawnable
{
    [field: SerializeField] public float Value { get; private set; }
    
    void ISpawnable.OnSpawn(object data)
    {
        this.transform.position = (Vector3)data;
    }

    void ISpawnable.OnDespawn()
    {
    }
}

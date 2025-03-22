using SAS.Collectables;
using SAS.Pool;
using UnityEngine;

public class Healer : BaseCollectable<Healer>, ISpawnable
{
    [field: SerializeField] public float Value { get; private set; }
    private SelfDespawnable _selfDespawnable;

    void Awake()
    {
        _selfDespawnable = GetComponent<SelfDespawnable>();
    }

    public override void Collect(Collector collector)
    {
        base.Collect(collector);
        if (_selfDespawnable)
            _selfDespawnable.StartDespawnTimer();
        else
            Despawn();
    }
    void ISpawnable.OnSpawn(object data)
    {
        this.transform.position = (Vector3)data;
    }

    void ISpawnable.OnDespawn()
    {
    }


}

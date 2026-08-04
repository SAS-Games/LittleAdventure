using SAS.Pool;
using UnityEngine;

public class SelfDespawnable : MonoBehaviour, ISpawnable
{
    [SerializeField] private bool m_Auto = true;
    [SerializeField] private float m_DespawnTime = 3f; // Time before despawning
    private Poolable _poolable;

    private void OnEnable()
    {
        if (_poolable == null)
            _poolable = GetComponent<Poolable>();
    }

    public void StartDespawnTimer()
    {
        Invoke(nameof(Despawn), m_DespawnTime);
    }

    public void StartDespawnTimer(float time)
    {
        Invoke(nameof(Despawn), time);
    }

    protected void Despawn()
    {
        _poolable?.Despawn();
    }

    protected virtual void OnSpawn(object data)
    {
    }

    protected virtual void OnDespawn()
    {
    }

    void ISpawnable.OnSpawn(object data)
    {
        if (m_Auto)
        {
            CancelInvoke(nameof(Despawn));
            OnSpawn(data);
            Invoke(nameof(Despawn), m_DespawnTime);
        }
    }

    void ISpawnable.OnDespawn()
    {
        CancelInvoke(nameof(Despawn));
        OnDespawn();
    }
}
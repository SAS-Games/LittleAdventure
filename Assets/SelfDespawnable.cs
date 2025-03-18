using SAS.Pool;
using System;
using UnityEngine;

public abstract class SelfDespawnable<T> : MonoBehaviour, ISpawnable where T : Component
{
    [SerializeField] private float m_DespawnTime = 3f; // Time before despawning
    [SerializeField] private ComponentPoolSO<T> m_Pool;

    public void StartDespawnTimer(float time)
    {
        Invoke(nameof(Despawn), time);
    }

    protected void Despawn()
    {
        if (m_Pool != null)
            m_Pool.Despawn(GetComponent<T>());
    }

    protected abstract void OnSpawn(object data);
    protected abstract void OnDespawn();

    void ISpawnable.OnSpawn(object data)
    {
        CancelInvoke(nameof(Despawn));
        OnSpawn(data);
        Invoke(nameof(Despawn), m_DespawnTime);
    }

    void ISpawnable.OnDespawn()
    {
        CancelInvoke(nameof(Despawn));
        OnDespawn();
    }
}

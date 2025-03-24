using SAS.Pool;
using SAS.WeaponSystem.Components;
using System.Linq;
using UnityEngine;

public class Projectile : Poolable, ISpawnable, IDamageable
{
    [SerializeField] private float m_Speed = 10;
    [SerializeField] private float m_Damage = 10;
    [SerializeField] private string[] m_CollisionTags = { "Player" };
    [SerializeField] private ComponentPoolSO<ParticleSystem> m_HitVFXPool;


    private Rigidbody _rigidbody;
    private Transform _attacker;

    event System.Action<float> IDamageable.OnDamageTaken
    {
        add { }
        remove { }
    }

    void ISpawnable.OnSpawn(object data)
    {
        _rigidbody = GetComponent<Rigidbody>();
        if (data is ProjectileSpawnInfo spawnInfo)
            SpawnProjectile(spawnInfo, this.transform);
        else
            Debug.LogError("Projectile.OnSpawn received invalid spawn data.");
    }

    void ISpawnable.OnDespawn()
    {
    }

    private void SpawnProjectile(ProjectileSpawnInfo spawnInfo, Transform projectile)
    {
        if (spawnInfo.Transform == null)
        {
            Debug.LogError("Projectile spawn failed: Transform is null.");
            return;
        }

        Transform characterTransform = spawnInfo.Transform;
        _attacker = characterTransform;
        Vector3 spawnPosition = characterTransform.position + (characterTransform.rotation * spawnInfo.Offset);
        projectile.position = spawnPosition;

        Vector3 direction = characterTransform.forward + spawnInfo.Direction;

        // Set projectile rotation to face the computed direction
        projectile.rotation = Quaternion.LookRotation(direction);

        // Ignore collision between the projectile and the character
        Collider[] characterColliders = characterTransform.root.GetComponentsInChildren<Collider>();
        Collider projectileCollider = projectile.GetComponent<Collider>();

        if (projectileCollider != null)
        {
            foreach (var characterCollider in characterColliders)
            {
                Physics.IgnoreCollision(projectileCollider, characterCollider);
            }
        }
        else
            Debug.LogWarning("Projectile does not have a collider, skipping collision ignore.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (m_CollisionTags.Contains(other.tag))
        {
            var damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
                damageable.Damage(new DamageInfo(m_Damage, _attacker.gameObject));
        }
        m_HitVFXPool.Spawn(this.transform.position);
        Despawn();
    }

    void FixedUpdate()
    {
        _rigidbody.MovePosition(transform.position + transform.forward * m_Speed * Time.deltaTime);
    }

    void IDamageable.Damage(IDamageInfo damageInfo)
    {
        m_HitVFXPool.Spawn(this.transform.position);
        Despawn();
    }
}

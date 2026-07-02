using System;
using UnityEngine;

namespace SAS.WeaponSystem.Components
{
    [Serializable]
    public class AttackProjectileSpawner : AttackData
    {
        // This is an array as each attack can spawn multiple projectiles.
        [field: SerializeField] public ProjectileSpawnInfo[] SpawnInfos { get; private set; }
    }

    [Serializable]
    public struct ProjectileSpawnInfo
    {
        // Offset from the players transform
        [field: SerializeField] public Vector3 Offset { get; private set; }

        // Direction that the projectile spawns in, relative to the facing direction of the player
        [field: SerializeField] public Vector3 Direction { get; private set; }
        public Transform Transform { get; private set; }

        public void SetTransform(Transform transform)
        {
            Transform = transform;
        }

        // The data to be passed to the projectile when it is spawned
        //[field: SerializeField] public DamageDataPackage DamageData { get; private set; }
        //[field: SerializeField] public KnockBackDataPackage KnockBackData { get; private set; }
        //[field: SerializeField] public PoiseDamageDataPackage PoiseDamageData { get; private set; }
    }
}
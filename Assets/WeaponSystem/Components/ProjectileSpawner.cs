using System;
using UnityEngine;
using SAS.WeaponSystem;
using SAS.StateMachineCharacterController;
using SAS.Pool;
using SAS.Utilities.TagSystem;
namespace SAS.WeaponSystem.Components
{
    public class ProjectileSpawner : WeaponComponent<ProjectileSpawnerData, AttackProjectileSpawner>
    {
        // Event fired off for each projectile before we call the Init() function on that projectile to allow other components to also pass through some data
        public event Action<Projectile> OnSpawnProjectile;

        [FieldRequiresParent] private ICharacter _character;
        [FieldRequiresParent] private IEventDispatcher _eventDispatcher;
        private ComponentPoolSO<Poolable> _objectPool;

        // Weapon Action Animation Event is used to trigger firing the projectiles
        private void HandleAttackAction()
        {
            foreach (var projectileSpawnInfo in CurrentAttackData.SpawnInfos)
            {
                projectileSpawnInfo.SetTransform(_character.Transform);
                _objectPool.Spawn(projectileSpawnInfo);
            }
        }

        public override void Init()
        {
            this.Initialize();
            base.Init();
            _objectPool = Data.ObjectPool;
            _objectPool.Initialize(4);
            _eventDispatcher.Subscribe(Data.AnimationEventName, HandleAttackAction);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _eventDispatcher.Unsubscribe(Data.AnimationEventName, HandleAttackAction);
        }

        private void OnDrawGizmosSelected()
        {
            if (Data == null || !Application.isPlaying)
                return;

            foreach (var item in Data.GetAllAttackData())
            {
                foreach (var point in item.SpawnInfos)
                {
                    var pos = transform.position + (Vector3)point.Offset;

                    Gizmos.DrawWireSphere(pos, 0.2f);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(pos, pos + (Vector3)point.Direction.normalized);
                    Gizmos.color = Color.white;
                }
            }
        }
    }
}
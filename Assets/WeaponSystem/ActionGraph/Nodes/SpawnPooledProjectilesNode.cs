using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SAS.Pool;
using SAS.StateMachineCharacterController;
using SAS.WeaponSystem.Components;
using UnityEngine;

namespace SAS.ActionGraph.WeaponSystem
{
    [Serializable]
    public class WeaponPooledProjectileData
    {
        public ComponentPoolSO<Poolable> objectPool;
        public ProjectileSpawnInfo[] spawnInfos = new ProjectileSpawnInfo[0];
        public int prewarmCount = 4;
    }

    [NodeBinding(typeof(SpawnPooledProjectilesNode))]
    [Serializable]
    public class WeaponPooledProjectileProvider : ActionDataProvider<WeaponPooledProjectileData>, IIndexedActionDataProvider
    {
    }

    [ActionNodeMenu("Weapon/Spawn Pooled Projectiles")]
    public class SpawnPooledProjectilesNode : ActionNode<WeaponPooledProjectileData>
    {
        private static readonly HashSet<int> InitializedPools = new HashSet<int>();

        public SpawnPooledProjectilesNode(ActionDataProvider<WeaponPooledProjectileData> dataProvider) : base(dataProvider)
        {
        }

        public override void Init(ActionContext context)
        {
            WeaponPooledProjectileData[] allData = _dataProvider.GetAllData();
            if (allData == null)
                return;

            for (int i = 0; i < allData.Length; i++)
            {
                WeaponPooledProjectileData data = allData[i];
                if (data == null || data.objectPool == null)
                    continue;

                int instanceId = data.objectPool.GetInstanceID();
                if (!InitializedPools.Add(instanceId))
                    continue;

                data.objectPool.Initialize(Mathf.Max(0, data.prewarmCount));
            }
        }

        public override Task ExecuteAsync(ActionContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            WeaponContext weaponContext = WeaponNodeUtility.RequireWeaponContext(context);
            WeaponPooledProjectileData data = WeaponNodeUtility.GetAttackData(_dataProvider, weaponContext);
            if (data == null || data.objectPool == null || data.spawnInfos == null)
                return Task.CompletedTask;

            Transform spawnTransform = GetProjectileSourceTransform(weaponContext);
            if (spawnTransform == null)
                return Task.CompletedTask;

            for (int i = 0; i < data.spawnInfos.Length; i++)
            {
                ProjectileSpawnInfo spawnInfo = data.spawnInfos[i];
                spawnInfo.SetTransform(spawnTransform);
                data.objectPool.Spawn(spawnInfo);
            }

            return Task.CompletedTask;
        }

        private static Transform GetProjectileSourceTransform(WeaponContext weaponContext)
        {
            if (weaponContext.Owner != null)
            {
                ICharacter character = weaponContext.Owner.GetComponentInParent<ICharacter>();
                if (character != null)
                    return character.Transform;
            }

            return weaponContext.OriginTransform;
        }
    }
}
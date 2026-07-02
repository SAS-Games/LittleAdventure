using System;
using System.Threading;
using System.Threading.Tasks;

namespace SAS.ActionGraph.WeaponSystem
{
    [Serializable]
    public class WeaponDamageData
    {
        public float amount = 10f;
        public bool useOwnerDamageModifier = true;
    }

    [NodeBinding(typeof(DamageHitsNode))]
    [Serializable]
    public class WeaponDamageProvider : ActionDataProvider<WeaponDamageData>, IIndexedActionDataProvider
    {
    }

    [ActionNodeMenu("Weapon/Damage Hits")]
    public class DamageHitsNode : ActionNode<WeaponDamageData>
    {
        public DamageHitsNode(ActionDataProvider<WeaponDamageData> dataProvider) : base(dataProvider)
        {
        }

        public override Task ExecuteAsync(ActionContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var weaponContext = RequireWeaponContext(context);
            var data = WeaponNodeUtility.GetAttackData(_dataProvider, weaponContext);
            if (data == null)
                return Task.CompletedTask;

            float amount = data.amount;
            if (data.useOwnerDamageModifier && weaponContext.Owner != null)
            {
                var modifier = weaponContext.Owner.GetComponentInParent<IWeaponDamageModifier>();
                if (modifier != null)
                    amount *= modifier.GetDamageMultiplier();
            }

            for (int i = 0; i < weaponContext.Hits.Count; i++)
            {
                WeaponHit hit = weaponContext.Hits[i];
                if (hit.Collider == null)
                    continue;

                var damageable = hit.Collider.GetComponentInParent<IWeaponDamageable>();
                if (damageable != null)
                    damageable.Damage(new WeaponDamageInfo(amount, weaponContext.Owner, hit.Point));
            }

            return Task.CompletedTask;
        }

        private static WeaponContext RequireWeaponContext(ActionContext context)
        {
            var weaponContext = context as WeaponContext;
            if (weaponContext == null)
                throw new InvalidOperationException("Damage hits node requires WeaponContext.");

            return weaponContext;
        }
    }
}
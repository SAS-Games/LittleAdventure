using System;
using System.Threading;
using System.Threading.Tasks;
using SAS.StateMachineCharacterController;
using UnityEngine;

namespace SAS.ActionGraph.WeaponSystem
{
    [Serializable]
    public class WeaponApplyKnockbackToHitsData
    {
        public Vector3 angle = new Vector3(1f, 0f, 0.5f);
        public float strength = 2f;
    }

    [NodeBinding(typeof(ApplyKnockbackToHitsNode))]
    [Serializable]
    public class WeaponApplyKnockbackToHitsProvider : ActionDataProvider<WeaponApplyKnockbackToHitsData>,
        IIndexedActionDataProvider
    {
    }

    [ActionNodeMenu("Weapon/Apply Knockback To Hits")]
    public class ApplyKnockbackToHitsNode : ActionNode<WeaponApplyKnockbackToHitsData>
    {
        public ApplyKnockbackToHitsNode(ActionDataProvider<WeaponApplyKnockbackToHitsData> dataProvider) : base(dataProvider)
        {
        }

        public override Task ExecuteAsync(ActionContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var weaponContext = WeaponNodeUtility.RequireWeaponContext(context);
            var data = WeaponNodeUtility.GetAttackData(_dataProvider, weaponContext) ??
                       new WeaponApplyKnockbackToHitsData();

            Vector3 angle = data.angle;
            float strength = data.strength;
            ICharacter attacker = weaponContext.Owner != null
                ? weaponContext.Owner.GetComponentInParent<ICharacter>()
                : null;

            for (int i = 0; i < weaponContext.Hits.Count; i++)
            {
                Collider collider = weaponContext.Hits[i].Collider;
                if (collider == null)
                    continue;

                IKnockbackable knockbackable = collider.GetComponentInParent<IKnockbackable>();
                if (knockbackable != null)
                    knockbackable.PerformAction(attacker, angle, strength);
            }

            return Task.CompletedTask;
        }
    }
}
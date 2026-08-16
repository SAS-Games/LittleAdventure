using System;
using System.Threading;
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

    [ActionNodeMenu("Weapon/Apply Knockback To Hits", "Pushes every target collected by the most recent hit-box node away from the attacker.")]
    public class ApplyKnockbackToHitsNode : WeaponActionNode<WeaponApplyKnockbackToHitsData>
    {
        public ApplyKnockbackToHitsNode(ActionDataProvider<WeaponApplyKnockbackToHitsData> dataProvider) : base(dataProvider)
        {
        }

        public override async Awaitable ExecuteAsync(ActionContext context, CancellationToken token)
        {
            await Awaitable.MainThreadAsync();
            token.ThrowIfCancellationRequested();

            var weaponContext = RequireWeaponContext(context);
            var data = GetAttackData(weaponContext) ??
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

            return;
        }
    }
}

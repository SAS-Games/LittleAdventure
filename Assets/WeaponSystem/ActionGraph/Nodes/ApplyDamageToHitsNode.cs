using System;
using System.Threading;
using SAS.StateMachineCharacterController;
using SAS.WeaponSystem;
using UnityEngine;

namespace SAS.ActionGraph.WeaponSystem
{
[Serializable]
public class WeaponApplyDamageToHitsData
{
    public float amount = 10f;
    public bool useOwnerDamageModifier = true;
}

[NodeBinding(typeof(ApplyDamageToHitsNode))]
[Serializable]
public class WeaponApplyDamageToHitsProvider : ActionDataProvider<WeaponApplyDamageToHitsData>, IIndexedActionDataProvider
{
}

[ActionNodeMenu("Weapon/Apply Damage To Hits", "Applies damage to every target collected by the most recent hit-box node.")]
public class ApplyDamageToHitsNode : WeaponActionNode<WeaponApplyDamageToHitsData>
{
    public ApplyDamageToHitsNode(ActionDataProvider<WeaponApplyDamageToHitsData> dataProvider) : base(dataProvider)
    {
    }

    public override async Awaitable ExecuteAsync(ActionContext context, CancellationToken token)
    {
        await Awaitable.MainThreadAsync();
        token.ThrowIfCancellationRequested();

        var weaponContext = RequireWeaponContext(context);
        var data = GetAttackData(weaponContext) ?? new WeaponApplyDamageToHitsData();

        float amount = data.amount;
        if (data.useOwnerDamageModifier && weaponContext.Owner != null)
        {
            IDamageModifier modifier = weaponContext.Owner.GetComponentInParent<IDamageModifier>();
            if (modifier != null)
                amount *= modifier.GetDamageMultiplier();
        }

        for (int i = 0; i < weaponContext.Hits.Count; i++)
        {
            Collider collider = weaponContext.Hits[i].Collider;
            if (collider == null)
                continue;

            IDamageable damageable = collider.GetComponent<IDamageable>() ?? collider.GetComponentInParent<IDamageable>();
            UnityEngine.Debug.Log(amount);
            if (damageable != null)
                damageable.Damage(new DamageInfo(amount, weaponContext.Owner != null ? weaponContext.Owner.transform.root.gameObject : null));
        }

        return;
    }
}
}

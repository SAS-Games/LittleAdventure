using System;
using System.Threading;
using System.Threading.Tasks;
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

[ActionNodeMenu("Weapon/Apply Damage To Hits")]
public class ApplyDamageToHitsNode : ActionNode<WeaponApplyDamageToHitsData>
{
    public ApplyDamageToHitsNode(ActionDataProvider<WeaponApplyDamageToHitsData> dataProvider) : base(dataProvider)
    {
    }

    public override Task ExecuteAsync(ActionContext context, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var weaponContext = WeaponNodeUtility.RequireWeaponContext(context);
        var data = WeaponNodeUtility.GetAttackData(_dataProvider, weaponContext) ?? new WeaponApplyDamageToHitsData();

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

        return Task.CompletedTask;
    }
}
}



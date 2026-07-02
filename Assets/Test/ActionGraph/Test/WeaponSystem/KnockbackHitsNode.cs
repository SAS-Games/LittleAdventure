using System;
using System.Threading;
using System.Threading.Tasks;
using SAS.StateMachineCharacterController;
using UnityEngine;

namespace SAS.ActionGraph.WeaponSystem
{
[Serializable]
public class WeaponKnockbackData
{
    public Vector3 localDirection = Vector3.forward;
    public float force = 5f;
}

[NodeBinding(typeof(KnockbackHitsNode))]
[Serializable]
public class WeaponKnockbackProvider : ActionDataProvider<WeaponKnockbackData>, IIndexedActionDataProvider
{
}

[ActionNodeMenu("Weapon/Knockback Hits")]
public class KnockbackHitsNode : ActionNode<WeaponKnockbackData>
{
    public KnockbackHitsNode(ActionDataProvider<WeaponKnockbackData> dataProvider) : base(dataProvider)
    {
    }

    public override Task ExecuteAsync(ActionContext context, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var weaponContext = RequireWeaponContext(context);
        var data = WeaponNodeUtility.GetAttackData(_dataProvider, weaponContext);
        if (data == null)
            return Task.CompletedTask;

        Vector3 direction = ToWorldDirection(weaponContext, data.localDirection);
        for (int i = 0; i < weaponContext.Hits.Count; i++)
        {
            WeaponHit hit = weaponContext.Hits[i];
            if (hit.Collider == null)
                continue;

            var knockbackable = hit.Collider.GetComponentInParent<IWeaponKnockbackable>();
            if (knockbackable != null)
            {
                knockbackable.ApplyKnockback(new WeaponKnockbackInfo(direction, data.force, weaponContext.Owner, hit.Point));
                continue;
            }

            if (hit.Collider.attachedRigidbody != null)
                hit.Collider.attachedRigidbody.AddForce(direction * data.force, ForceMode.Impulse);
        }

        return Task.CompletedTask;
    }

    private static Vector3 ToWorldDirection(WeaponContext context, Vector3 localDirection)
    {
        Transform origin = context.OriginTransform;
        Vector3 direction = origin != null ? origin.TransformDirection(localDirection) : localDirection;
        return direction.sqrMagnitude > 0f ? direction.normalized : Vector3.forward;
    }

    private static WeaponContext RequireWeaponContext(ActionContext context)
    {
        var weaponContext = context as WeaponContext;
        if (weaponContext == null)
            throw new InvalidOperationException("Knockback hits node requires WeaponContext.");

        return weaponContext;
    }
}
}



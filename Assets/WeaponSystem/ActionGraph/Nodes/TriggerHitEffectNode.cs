using System;
using System.Threading;
using SAS.StateMachineCharacterController;
using SAS.WeaponSystem;
using UnityEngine;

namespace SAS.ActionGraph.WeaponSystem
{
[Serializable]
public class WeaponTriggerHitEffectData
{
    public string eventName = "PlaySlash";
    public bool onlyIfDamageable = true;
}

[NodeBinding(typeof(TriggerHitEffectNode))]
[Serializable]
public class WeaponTriggerHitEffectProvider : ActionDataProvider<WeaponTriggerHitEffectData>, IIndexedActionDataProvider
{
}

[ActionNodeMenu("Weapon/Trigger Hit Effect", "Raises a hit-effect event when the current attack struck a valid target.")]
public class TriggerHitEffectNode : WeaponActionNode<WeaponTriggerHitEffectData>
{
    public TriggerHitEffectNode(ActionDataProvider<WeaponTriggerHitEffectData> dataProvider) : base(dataProvider)
    {
    }

    public override async Awaitable ExecuteAsync(ActionContext context, CancellationToken token)
    {
        await Awaitable.MainThreadAsync();
        token.ThrowIfCancellationRequested();

        var weaponContext = RequireWeaponContext(context);
        var data = GetAttackData(weaponContext) ?? new WeaponTriggerHitEffectData();
        string eventName = data.eventName;

        if (string.IsNullOrEmpty(eventName) || weaponContext.Owner == null)
            return;

        for (int i = 0; i < weaponContext.Hits.Count; i++)
        {
            Collider collider = weaponContext.Hits[i].Collider;
            if (collider == null)
                continue;

            bool hasDamageable = collider.GetComponent<IDamageable>() != null || collider.GetComponentInParent<IDamageable>() != null;
            if (data.onlyIfDamageable && !hasDamageable)
                continue;

            EventDispatcher dispatcher = weaponContext.Owner.GetComponentInParent<EventDispatcher>();
            if (dispatcher != null)
                dispatcher.TriggerParamEvent(eventName, weaponContext.Owner.transform.position + Vector3.up * 0.5f);
            break;
        }

        return;
    }
}
}



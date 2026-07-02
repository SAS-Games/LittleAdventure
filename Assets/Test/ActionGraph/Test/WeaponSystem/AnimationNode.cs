using System;
using System.Threading;
using System.Threading.Tasks;
using SAS.StateMachineCharacterController;
using UnityEngine;

namespace SAS.ActionGraph.WeaponSystem
{
public enum WeaponAnimationMode
{
    PlayState,
    CrossFadeState,
    SetTrigger
}

[Serializable]
public class WeaponAnimationData
{
    public WeaponAnimationMode mode = WeaponAnimationMode.CrossFadeState;
    public string stateOrTriggerName = "Attack0";
    public int layer;
    public float transitionDuration = 0.05f;
    public float normalizedStartTime;
}

[NodeBinding(typeof(AnimationNode))]
[Serializable]
public class WeaponAnimationProvider : ActionDataProvider<WeaponAnimationData>, IIndexedActionDataProvider
{
}

[ActionNodeMenu("Weapon/Animation")]
public class AnimationNode : ActionNode<WeaponAnimationData>
{
    public AnimationNode(ActionDataProvider<WeaponAnimationData> dataProvider) : base(dataProvider)
    {
    }

    public override Task ExecuteAsync(ActionContext context, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var weaponContext = RequireWeaponContext(context);
        var data = WeaponNodeUtility.GetAttackData(_dataProvider, weaponContext);

        if (weaponContext.Animator == null || data == null || string.IsNullOrEmpty(data.stateOrTriggerName))
            return Task.CompletedTask;

        switch (data.mode)
        {
            case WeaponAnimationMode.PlayState:
                weaponContext.Animator.Play(data.stateOrTriggerName, data.layer, data.normalizedStartTime);
                break;

            case WeaponAnimationMode.CrossFadeState:
                weaponContext.Animator.CrossFadeInFixedTime(
                    data.stateOrTriggerName,
                    Mathf.Max(0f, data.transitionDuration),
                    data.layer,
                    data.normalizedStartTime);
                break;

            case WeaponAnimationMode.SetTrigger:
                weaponContext.Animator.SetTrigger(data.stateOrTriggerName);
                break;
        }

        return Task.CompletedTask;
    }

    private static WeaponContext RequireWeaponContext(ActionContext context)
    {
        var weaponContext = context as WeaponContext;
        if (weaponContext == null)
            throw new InvalidOperationException("Weapon animation node requires WeaponContext.");

        return weaponContext;
    }
}
}



using System;
using System.Threading;
using SAS.StateMachineCharacterController;
using SAS.WeaponSystem;
using UnityEngine;

namespace SAS.ActionGraph.WeaponSystem
{
[Serializable]
public class WeaponIndexedAnimationData
{
    public string statePrefix = "Attack";
    public int layer;
    public bool crossFade;
    public float transitionDuration = 0.05f;
    public float normalizedStartTime;
}

[NodeBinding(typeof(IndexedAnimationNode))]
[Serializable]
public class WeaponIndexedAnimationProvider : ActionDataProvider<WeaponIndexedAnimationData>, IIndexedActionDataProvider
{
}

[ActionNodeMenu("Weapon/Indexed Animation", "Plays the animation whose state name ends with the current combo attack index.")]
public class IndexedAnimationNode : WeaponActionNode<WeaponIndexedAnimationData>
{
    public IndexedAnimationNode(ActionDataProvider<WeaponIndexedAnimationData> dataProvider) : base(dataProvider)
    {
    }

    public override async Awaitable ExecuteAsync(ActionContext context, CancellationToken token)
    {
        await Awaitable.MainThreadAsync();
        token.ThrowIfCancellationRequested();

        var weaponContext = RequireWeaponContext(context);
        var data = GetAttackData(weaponContext);
        if (weaponContext.Animator == null || data == null)
            return;

        string stateName = $"{data.statePrefix}{weaponContext.CurrentAttackIndex}";
        if (data.crossFade)
        {
            weaponContext.Animator.CrossFadeInFixedTime(
                stateName,
                Mathf.Max(0f, data.transitionDuration),
                data.layer,
                data.normalizedStartTime);
        }
        else
        {
            weaponContext.Animator.Play(stateName, data.layer, data.normalizedStartTime);
        }

        return;
    }
}
}



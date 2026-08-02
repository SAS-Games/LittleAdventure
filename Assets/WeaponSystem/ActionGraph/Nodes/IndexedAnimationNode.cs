using System;
using System.Threading;
using System.Threading.Tasks;
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

[ActionNodeMenu("Weapon/Indexed Animation")]
public class IndexedAnimationNode : ActionNode<WeaponIndexedAnimationData>
{
    public IndexedAnimationNode(ActionDataProvider<WeaponIndexedAnimationData> dataProvider) : base(dataProvider)
    {
    }

    public override Task ExecuteAsync(ActionContext context, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var weaponContext = WeaponNodeUtility.RequireWeaponContext(context);
        var data = WeaponNodeUtility.GetAttackData(_dataProvider, weaponContext);
        if (weaponContext.Animator == null || data == null)
            return Task.CompletedTask;

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

        return Task.CompletedTask;
    }
}
}



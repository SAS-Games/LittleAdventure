using System;
using System.Threading;
using SAS.StateMachineCharacterController;
using SAS.WeaponSystem;
using UnityEngine;

namespace SAS.ActionGraph.WeaponSystem
{
[Serializable]
public class WeaponWaitAnimationExitData
{
    public int layer;
    public string stateTag = "Attack";
}

[NodeBinding(typeof(WaitAnimationExitNode))]
[Serializable]
public class WeaponWaitAnimationExitProvider : ActionDataProvider<WeaponWaitAnimationExitData>, IIndexedActionDataProvider
{
}

[ActionNodeMenu("Weapon/Wait Animation Exit", "Pauses graph execution until the tagged attack animation state has finished.")]
public class WaitAnimationExitNode : WeaponActionNode<WeaponWaitAnimationExitData>
{
    public WaitAnimationExitNode(ActionDataProvider<WeaponWaitAnimationExitData> dataProvider) : base(dataProvider)
    {
    }

    public override async Awaitable ExecuteAsync(ActionContext context, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var weaponContext = RequireWeaponContext(context);
        var data = GetAttackData(weaponContext) ?? new WeaponWaitAnimationExitData();
        await WeaponNodeUtility.WaitForAnimationExitAsync(weaponContext, data.layer, data.stateTag, token);
    }
}
}



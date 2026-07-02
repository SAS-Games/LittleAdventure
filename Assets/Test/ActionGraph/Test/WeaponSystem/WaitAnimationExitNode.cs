using System;
using System.Threading;
using System.Threading.Tasks;
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

[ActionNodeMenu("Weapon/Wait Animation Exit")]
public class WaitAnimationExitNode : ActionNode<WeaponWaitAnimationExitData>
{
    public WaitAnimationExitNode(ActionDataProvider<WeaponWaitAnimationExitData> dataProvider) : base(dataProvider)
    {
    }

    public override async Task ExecuteAsync(ActionContext context, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var weaponContext = WeaponNodeUtility.RequireWeaponContext(context);
        var data = WeaponNodeUtility.GetAttackData(_dataProvider, weaponContext) ?? new WeaponWaitAnimationExitData();
        string stateTag = data.stateTag;

        if (weaponContext.Animator == null)
            return;

        bool enteredAttackState = false;
        while (true)
        {
            token.ThrowIfCancellationRequested();

            AnimatorStateInfo stateInfo = weaponContext.Animator.GetCurrentAnimatorStateInfo(data.layer);
            bool inAttackState = stateInfo.IsTag(stateTag);

            if (inAttackState)
                enteredAttackState = true;
            else if (enteredAttackState)
                return;

            await Awaitable.NextFrameAsync();
        }
    }
}
}



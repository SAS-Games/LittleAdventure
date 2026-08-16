using System;
using System.Threading;
using SAS.StateMachineCharacterController;
using SAS.WeaponSystem;
using UnityEngine;

namespace SAS.ActionGraph.WeaponSystem
{
[Serializable]
public class ComboBeginCurrentAttackData
{
}

[NodeBinding(typeof(ComboBeginCurrentAttackNode))]
[Serializable]
public class ComboBeginCurrentAttackProvider : ActionDataProvider<ComboBeginCurrentAttackData>
{
}

[ActionNodeMenu("Weapon/Combo Begin Current Attack", "Starts the current combo step and clears hit data from the previous attack.")]
public class ComboBeginCurrentAttackNode : WeaponActionNode<ComboBeginCurrentAttackData>
{
    public ComboBeginCurrentAttackNode(ActionDataProvider<ComboBeginCurrentAttackData> dataProvider) : base(dataProvider)
    {
    }

    public override async Awaitable ExecuteAsync(ActionContext context, CancellationToken token)
    {
        await Awaitable.MainThreadAsync();
        token.ThrowIfCancellationRequested();

        var weaponContext = RequireWeaponContext(context);
        int index = Mathf.Max(0, weaponContext.CurrentAttackIndex);
        weaponContext.BeginAttack(index);
        return;
    }
}
}


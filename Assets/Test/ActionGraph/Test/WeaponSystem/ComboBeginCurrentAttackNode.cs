using System;
using System.Threading;
using System.Threading.Tasks;
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

[ActionNodeMenu("Weapon/Combo Begin Current Attack")]
public class ComboBeginCurrentAttackNode : ActionNode<ComboBeginCurrentAttackData>
{
    public ComboBeginCurrentAttackNode(ActionDataProvider<ComboBeginCurrentAttackData> dataProvider) : base(dataProvider)
    {
    }

    public override Task ExecuteAsync(ActionContext context, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var weaponContext = WeaponNodeUtility.RequireWeaponContext(context);
        int index = Mathf.Max(0, weaponContext.CurrentAttackIndex);
        weaponContext.BeginAttack(index);
        return Task.CompletedTask;
    }
}
}


using System;
using System.Threading;
using System.Threading.Tasks;
using SAS.StateMachineCharacterController;
using SAS.WeaponSystem;
using UnityEngine;

namespace SAS.ActionGraph.WeaponSystem
{
[Serializable]
public class ComboAdvanceIfInputAcceptedData
{
    public int comboCount = 3;
}

[NodeBinding(typeof(ComboAdvanceIfInputAcceptedNode))]
[Serializable]
public class ComboAdvanceIfInputAcceptedProvider : ActionDataProvider<ComboAdvanceIfInputAcceptedData>
{
}

[ActionNodeMenu("Weapon/Combo Advance If Input Accepted")]
public class ComboAdvanceIfInputAcceptedNode : ActionNode<ComboAdvanceIfInputAcceptedData>
{
    public ComboAdvanceIfInputAcceptedNode(ActionDataProvider<ComboAdvanceIfInputAcceptedData> dataProvider) : base(dataProvider)
    {
    }

    public override Task ExecuteAsync(ActionContext context, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var weaponContext = WeaponNodeUtility.RequireWeaponContext(context);
        var data = _selector.GetNext() ?? new ComboAdvanceIfInputAcceptedData();
        if (!weaponContext.ComboInputAccepted)
            return Task.CompletedTask;

        int comboCount = Mathf.Max(1, data.comboCount);
        int nextIndex = weaponContext.CurrentAttackIndex + 1;
        if (nextIndex >= comboCount)
        {
            weaponContext.ComboInputAccepted = false;
            return Task.CompletedTask;
        }

        weaponContext.CurrentAttackIndex = nextIndex;
        return Task.CompletedTask;
    }
}
}


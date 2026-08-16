using System;
using System.Threading;
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

[ActionNodeMenu("Weapon/Combo Advance If Input Accepted", "Advances to the next combo attack only when the input window accepted a buffered attack.")]
public class ComboAdvanceIfInputAcceptedNode : WeaponActionNode<ComboAdvanceIfInputAcceptedData>
{
    public ComboAdvanceIfInputAcceptedNode(ActionDataProvider<ComboAdvanceIfInputAcceptedData> dataProvider) : base(dataProvider)
    {
    }

    public override async Awaitable ExecuteAsync(ActionContext context, CancellationToken token)
    {
        await Awaitable.MainThreadAsync();
        token.ThrowIfCancellationRequested();

        var weaponContext = RequireWeaponContext(context);
        var data = _selector.GetNext() ?? new ComboAdvanceIfInputAcceptedData();
        if (!weaponContext.ComboInputAccepted)
            return;

        int comboCount = Mathf.Max(1, data.comboCount);
        int nextIndex = weaponContext.CurrentAttackIndex + 1;
        if (nextIndex >= comboCount)
        {
            weaponContext.ComboInputAccepted = false;
            return;
        }

        weaponContext.CurrentAttackIndex = nextIndex;
        return;
    }
}
}


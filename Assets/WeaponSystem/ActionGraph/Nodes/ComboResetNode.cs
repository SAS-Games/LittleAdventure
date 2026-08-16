using System;
using System.Threading;
using SAS.StateMachineCharacterController;
using SAS.WeaponSystem;
using UnityEngine;

namespace SAS.ActionGraph.WeaponSystem
{
[Serializable]
public class ComboResetData
{
}

[NodeBinding(typeof(ComboResetNode))]
[Serializable]
public class ComboResetProvider : ActionDataProvider<ComboResetData>
{
}

[ActionNodeMenu("Weapon/Combo Reset", "Returns the combo to its first attack and clears buffered input and hit state.")]
public class ComboResetNode : WeaponActionNode<ComboResetData>
{
    public ComboResetNode(ActionDataProvider<ComboResetData> dataProvider) : base(dataProvider)
    {
    }

    public override async Awaitable ExecuteAsync(ActionContext context, CancellationToken token)
    {
        await Awaitable.MainThreadAsync();
        token.ThrowIfCancellationRequested();

        var weaponContext = RequireWeaponContext(context);
        weaponContext.ResetCombo();
        return;
    }
}
}


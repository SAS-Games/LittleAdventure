using System;
using System.Threading;
using System.Threading.Tasks;
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

[ActionNodeMenu("Weapon/Combo Reset")]
public class ComboResetNode : ActionNode<ComboResetData>
{
    public ComboResetNode(ActionDataProvider<ComboResetData> dataProvider) : base(dataProvider)
    {
    }

    public override Task ExecuteAsync(ActionContext context, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var weaponContext = WeaponNodeUtility.RequireWeaponContext(context);
        weaponContext.ResetCombo();
        return Task.CompletedTask;
    }
}
}


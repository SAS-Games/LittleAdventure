using System;
using System.Threading;
using System.Threading.Tasks;
using SAS.StateMachineCharacterController;
using UnityEngine;

namespace SAS.ActionGraph.WeaponSystem
{
[Serializable]
public class WeaponComboData
{
    public int comboCount = 1;
    public bool resetGraphWhenWrapped = true;
}

[NodeBinding(typeof(AdvanceComboNode))]
[Serializable]
public class WeaponComboProvider : ActionDataProvider<WeaponComboData>
{
}

[ActionNodeMenu("Weapon/Advance Combo")]
public class AdvanceComboNode : ActionNode<WeaponComboData>
{
    public AdvanceComboNode(ActionDataProvider<WeaponComboData> dataProvider) : base(dataProvider)
    {
    }

    public override Task ExecuteAsync(ActionContext context, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var weaponContext = RequireWeaponContext(context);
        var data = _selector.GetNext();
        if (data != null && weaponContext.Weapon != null)
            weaponContext.Weapon.AdvanceCombo(data.comboCount, data.resetGraphWhenWrapped);

        return Task.CompletedTask;
    }

    private static WeaponContext RequireWeaponContext(ActionContext context)
    {
        var weaponContext = context as WeaponContext;
        if (weaponContext == null)
            throw new InvalidOperationException("Advance combo node requires WeaponContext.");

        return weaponContext;
    }
}
}



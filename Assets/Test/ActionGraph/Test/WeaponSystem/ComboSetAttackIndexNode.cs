using System;
using System.Threading;
using System.Threading.Tasks;
using SAS.StateMachineCharacterController;
using SAS.WeaponSystem;
using UnityEngine;

namespace SAS.ActionGraph.WeaponSystem
{
[Serializable]
public class ComboSetAttackIndexData
{
    public int attackIndex;
}

[NodeBinding(typeof(ComboSetAttackIndexNode))]
[Serializable]
public class ComboSetAttackIndexProvider : ActionDataProvider<ComboSetAttackIndexData>
{
}

[ActionNodeMenu("Weapon/Combo Set Attack Index")]
public class ComboSetAttackIndexNode : ActionNode<ComboSetAttackIndexData>
{
    public ComboSetAttackIndexNode(ActionDataProvider<ComboSetAttackIndexData> dataProvider) : base(dataProvider)
    {
    }

    public override Task ExecuteAsync(ActionContext context, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var weaponContext = WeaponNodeUtility.RequireWeaponContext(context);
        var data = _selector.GetNext();
        int index = Mathf.Max(0, data?.attackIndex ?? 0);

        weaponContext.ComboWeapon?.SetCurrentAttackIndex(index);
        weaponContext.BeginAttack(index);
        return Task.CompletedTask;
    }
}
}


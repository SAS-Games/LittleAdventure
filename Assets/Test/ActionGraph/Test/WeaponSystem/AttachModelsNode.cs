using System;
using System.Threading;
using System.Threading.Tasks;
using SAS.StateMachineCharacterController;
using SAS.WeaponSystem;
using UnityEngine;

namespace SAS.ActionGraph.WeaponSystem
{
[Serializable]
public class WeaponAttachModelsData
{
    public string leftSocketPath;
    public string rightSocketPath;
    public GameObject leftWeapon;
    public GameObject rightWeapon;
}

[NodeBinding(typeof(AttachModelsNode))]
[Serializable]
public class WeaponAttachModelsProvider : ActionDataProvider<WeaponAttachModelsData>
{
}

[ActionNodeMenu("Weapon/Attach Models")]
public class AttachModelsNode : ActionNode<WeaponAttachModelsData>
{
    private GameObject _leftWeaponInstance;
    private GameObject _rightWeaponInstance;

    public AttachModelsNode(ActionDataProvider<WeaponAttachModelsData> dataProvider) : base(dataProvider)
    {
    }

    public override void Init(ActionContext context)
    {
        AttachWeapons(context);
        _selector.Reset();
    }

    public override Task ExecuteAsync(ActionContext context, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        AttachWeapons(context);
        return Task.CompletedTask;
    }

    private void AttachWeapons(ActionContext context)
    {
        var weaponContext = WeaponNodeUtility.RequireWeaponContext(context);
        var data = _selector.GetNext();
        if (data == null || weaponContext.Owner == null)
            return;

        Transform root = weaponContext.Owner.transform.root;

        if (_leftWeaponInstance == null && data.leftWeapon != null)
            _leftWeaponInstance = WeaponNodeUtility.AttachChild(data.leftWeapon, root, data.leftSocketPath);

        if (_rightWeaponInstance == null && data.rightWeapon != null)
            _rightWeaponInstance = WeaponNodeUtility.AttachChild(data.rightWeapon, root, data.rightSocketPath);
    }
}
}



using SAS.StateMachineGraph;
using SAS.Utilities.TagSystem;
using SAS.WeaponSystem;
using UnityEngine;

public class AttackAction : IAwaitableStateAction
{
    [FieldRequiresChild] private IWeapon _weapon;
    public bool IsCompleted => !_weapon.IsInUse;

    void IStateAction.OnInitialize(Actor actor, Tag tag, string key)
    {
        actor.Initialize(this);
    }

    void IStateAction.Execute(ActionExecuteEvent executeEvent)
    {
        _weapon.Enter();
    }
}

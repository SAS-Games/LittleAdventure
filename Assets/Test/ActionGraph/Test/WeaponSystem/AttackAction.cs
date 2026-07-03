using SAS.Core.TagSystem;
using SAS.StateMachineGraph;
using SAS.WeaponSystem;

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

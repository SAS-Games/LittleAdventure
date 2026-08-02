using SAS.Core.TagSystem;
using SAS.ActionGraph.WeaponSystem;
using SAS.StateMachineGraph;

public class AttackAction : IAwaitableStateAction
{
    [FieldRequiresChild] private IAttackWeaponController _weaponController;
    public bool IsCompleted => !_weaponController.IsInUse;

    void IStateAction.OnInitialize(Actor actor, Tag tag, string key)
    {
        actor.Initialize(this);
    }

    void IStateAction.Execute(ActionExecuteEvent executeEvent)
    {
        _weaponController.Enter();
    }
}

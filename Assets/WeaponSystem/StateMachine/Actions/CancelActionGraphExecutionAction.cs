using SAS.Core.TagSystem;
using SAS.StateMachineGraph;

public class CancelActionGraphExecutionAction : IStateAction
{
    [FieldRequiresChild] private IActionGraphExecutionController _executionController;

    void IStateAction.OnInitialize(Actor actor, Tag tag, string key)
    {
        actor.Initialize(this);
    }

    void IStateAction.Execute(ActionExecuteEvent executeEvent)
    {
        _executionController?.CancelExecution();
    }
}

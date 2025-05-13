using EnemySystem;
using SAS.StateMachineGraph;
using SAS.Utilities.TagSystem;
using UnityEngine;

public abstract class UpdateThreatLevel : IStateAction
{
    [FieldRequiresSelf] protected IHasTarget _targetHolder;

    void IStateAction.OnInitialize(Actor actor, Tag tag, string key)
    {
        actor.Initialize(this);
    }

    void IStateAction.Execute(ActionExecuteEvent executeEvent)
    {
        var threatLevel = _targetHolder.Target.Transform?.GetComponent<IThreatLevel>();
        if (threatLevel != null)
        {
            int delta = GetThreatDelta();
            threatLevel.Value.Value = Mathf.Max(0, threatLevel.Value.Value + delta);
        }
    }

    protected abstract int GetThreatDelta();
}
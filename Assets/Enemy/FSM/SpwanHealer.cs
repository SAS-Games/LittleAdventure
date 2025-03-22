using EnemySystem;
using SAS.StateMachineGraph;
using SAS.Utilities.TagSystem;
using UnityEngine;

public class SpwanHealer : IStateAction
{
    [FieldRequiresSelf] private Enemy _character;
    void IStateAction.OnInitialize(Actor actor, Tag tag, string key)
    {
        actor.Initialize(this);
    }

    void IStateAction.Execute(ActionExecuteEvent executeEvent)
    {
        _character.DropItem(_character.transform.position);
    }


}

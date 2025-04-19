using EnemySystem;
using SAS.StateMachineCharacterController;
using SAS.StateMachineGraph;
using SAS.Utilities.TagSystem;
using UnityEngine;
using UnityEngine.AI;

namespace EnemySystem
{
    public class SetTargetSelf : IStateAction
    {
        [FieldRequiresSelf] private NavMeshAgent _agent;
        [FieldRequiresSelf] private ICharacter _character;

        void IStateAction.OnInitialize(Actor actor, Tag tag, string key)
        {
            actor.Initialize(this);
        }

        void IStateAction.Execute(ActionExecuteEvent executeEvent)
        {
            _agent.SetDestination(_character.Transform.position);
        }

    }
}

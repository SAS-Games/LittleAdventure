using SAS.StateMachineGraph;
using SAS.Utilities.TagSystem;
using UnityEngine.AI;

namespace EnemySystem
{
    public class Follow : IStateAction
    {
        [FieldRequiresSelf] private NavMeshAgent _agent;
        [FieldRequiresSelf] private IHasTarget _targetHolder;

        void IStateAction.OnInitialize(Actor actor, Tag tag, string key)
        {
            actor.Initialize(this);
        }

        void IStateAction.Execute(ActionExecuteEvent executeEvent)
        {
            _agent.SetDestination(_targetHolder.Target.position);
        }

    }
}

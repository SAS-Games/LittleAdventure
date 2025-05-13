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
            if (_targetHolder.Target == null || !_targetHolder.Target.IsActive)
                return;

            _agent.SetDestination(_targetHolder.Target.Position);
        }
    }
}
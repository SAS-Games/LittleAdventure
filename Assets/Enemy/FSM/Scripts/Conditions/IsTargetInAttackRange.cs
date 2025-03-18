using SAS.StateMachineGraph;
using SAS.Utilities.BlackboardSystem;
using SAS.Utilities.TagSystem;
using UnityEngine;
using UnityEngine.AI;

namespace EnemySystem
{
    public class IsTargetInAttackRange : ICustomCondition
    {
        [FieldRequiresSelf] private NavMeshAgent _navMeshAgent;
        [FieldRequiresSelf] private Enemy _enemy;
        private BlackboardKey _rangeKey = default;
        private Actor _actor;

        void ICustomCondition.OnInitialize(Actor actor)
        {
            actor.Initialize(this);
            _rangeKey = actor.GetOrRegisterKey(EnemyBlackboardKey.ChaseRange);
            _actor = actor;

        }

        bool ICustomCondition.Evaluate()
        {
            var distance = Vector3.Distance(_enemy.transform.position, _enemy.Target.position);
            return distance < _navMeshAgent.stoppingDistance;
        }



        void ICustomCondition.OnStateEnter()
        {
        }

        void ICustomCondition.OnStateExit()
        {
        }
    }
}
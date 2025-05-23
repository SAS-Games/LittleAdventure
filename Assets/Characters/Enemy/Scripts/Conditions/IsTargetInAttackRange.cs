using SAS.StateMachineCharacterController;
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
        [FieldRequiresSelf] private ICharacter _character;
        private IHasTarget _targetHolder;

        private BlackboardKey _rangeKey = default;
        private Actor _actor;

        void ICustomCondition.OnInitialize(Actor actor)
        {
            actor.Initialize(this);
            _targetHolder = _character as IHasTarget;
            _rangeKey = actor.GetOrRegisterKey(EnemyBlackboardKey.ChaseRange);
            _actor = actor;

        }

        bool ICustomCondition.Evaluate()
        {
            if (_targetHolder.Target == null || !_targetHolder.Target.IsActive)
                return false;
            var distance = Vector3.Distance(_character.Transform.position, _targetHolder.Target.Position);
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
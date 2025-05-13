using SAS.StateMachineCharacterController;
using SAS.StateMachineGraph;
using SAS.Utilities.BlackboardSystem;
using UnityEngine;

namespace EnemySystem
{
    public class IsTargetInRange : ICustomCondition
    {
        private ICharacter _character;
        private IHasTarget _targetHolder;
        private BlackboardKey _rangeKey = default;
        private Actor _actor;

        void ICustomCondition.OnInitialize(Actor actor)
        {
            _character = actor.GetComponent<ICharacter>();
            _targetHolder = _character as IHasTarget;
            _rangeKey = actor.GetOrRegisterKey(EnemyBlackboardKey.ChaseRange);
            _actor = actor;

        }

        bool ICustomCondition.Evaluate()
        {
            if (_targetHolder.Target == null || !_targetHolder.Target.IsActive)
                return false;
            return Vector3.Distance(_character.Transform.position, _targetHolder.Target.Position) < _actor.GetValue<float>(_rangeKey);
        }



        void ICustomCondition.OnStateEnter()
        {
        }

        void ICustomCondition.OnStateExit()
        {
        }
    }
}
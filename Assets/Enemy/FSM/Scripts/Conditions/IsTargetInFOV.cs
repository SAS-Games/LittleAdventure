using EnemySystem;
using SAS.StateMachineCharacterController;
using SAS.StateMachineGraph;
using SAS.Utilities.BlackboardSystem;
using UnityEngine;

namespace EnemySystem
{
    public class IsTargetInFOV : ICustomCondition
    {
        private ICharacter _character;
        private IHasTarget _targetHolder;
        private BlackboardKey _FOVKey = default;
        private Actor _actor;

        void ICustomCondition.OnInitialize(Actor actor)
        {
            _character = actor.GetComponent<ICharacter>();
            _targetHolder = _character as IHasTarget;
            _FOVKey = actor.GetOrRegisterKey(EnemyBlackboardKey.FOV);
            _actor = actor;

        }

        bool ICustomCondition.Evaluate()
        {
            Vector3 dirToPlayer = (_targetHolder.Target.position - _character.Transform.position).normalized;
            return (Vector3.Angle(_character.Transform.forward, dirToPlayer) < _actor.GetValue<float>(_FOVKey) / 2);
        }



        void ICustomCondition.OnStateEnter()
        {
        }

        void ICustomCondition.OnStateExit()
        {
        }
    }
}
using EnemySystem;
using SAS.StateMachineCharacterController;
using SAS.StateMachineGraph;
using SAS.Utilities.BlackboardSystem;
using UnityEngine;

namespace EnemySystem
{
    public class HasClearLineOfSight : ICustomCondition
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
            if (_targetHolder.Target == null || !_targetHolder.Target.IsActive)
                return false;

            Vector3 dirToPlayer = (_targetHolder.Target.Position - _character.Transform.position).normalized;
            float distanceToPlayer = Vector3.Distance(_character.Transform.position, _targetHolder.Target.Position);

            // Check if there are obstacles blocking the view
            if (!Physics.Raycast(_character.Transform.position, dirToPlayer, distanceToPlayer, _targetHolder.VisibilityBlockers))
            {
                return true; // No obstacle means clear line of sight
            }
            return false;
        }



        void ICustomCondition.OnStateEnter()
        {
        }

        void ICustomCondition.OnStateExit()
        {
        }
    }
}
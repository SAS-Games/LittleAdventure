using SAS.StateMachineCharacterController;
using SAS.StateMachineGraph;
using SAS.Utilities.TagSystem;
using UnityEngine;
using UnityEngine.AI;

namespace EnemySystem
{
    public class LookAtTarget : IStateAction
    {
        [FieldRequiresSelf] private NavMeshAgent _agent;
        [FieldRequiresSelf] private ICharacter _character;
        private IHasTarget _targetHolder;

        void IStateAction.OnInitialize(Actor actor, Tag tag, string key)
        {
            actor.Initialize(this);
            _targetHolder = _character as IHasTarget;
        }

        void IStateAction.Execute(ActionExecuteEvent executeEvent)
        {
            if (_targetHolder.Target == null)
                return;

            // Get direction to the target
            Vector3 direction = _targetHolder.Target.position - _character.Transform.position;
            direction.y = 0; // Ignore Y-axis to prevent tilting

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                _character.Transform.rotation = targetRotation;
            }
        }
    }
}

using EnemySystem;
using SAS.StateMachineGraph;
using SAS.Utilities.TagSystem;
using UnityEngine;
using UnityEngine.AI;

namespace EnemySystem
{
    public class LookAtTarget : IStateAction
    {
        [FieldRequiresSelf] private NavMeshAgent _agent;
        [FieldRequiresSelf] private Enemy _enemy;

        void IStateAction.OnInitialize(Actor actor, Tag tag, string key)
        {
            actor.Initialize(this);
        }

        void IStateAction.Execute(ActionExecuteEvent executeEvent)
        {
            if (_enemy.Target == null)
                return;

            // Get direction to the target
            Vector3 direction = _enemy.Target.position - _enemy.transform.position;
            direction.y = 0; // Ignore Y-axis to prevent tilting

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                _enemy.transform.rotation = targetRotation;
            }
        }
    }
}

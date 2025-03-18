using EnemySystem;
using SAS.StateMachineGraph;
using SAS.Utilities.BlackboardSystem;
using UnityEngine;

namespace EnemySystem
{
    public class HasClearLineOfSight : ICustomCondition
    {
        private Enemy _enemy;
        private BlackboardKey _FOVKey = default;
        private Actor _actor;

        void ICustomCondition.OnInitialize(Actor actor)
        {
            _enemy = actor.GetComponent<Enemy>();
            _FOVKey = actor.GetOrRegisterKey(EnemyBlackboardKey.FOV);
            _actor = actor;

        }

        bool ICustomCondition.Evaluate()
        {
            Vector3 dirToPlayer = (_enemy.Target.position - _enemy.transform.position).normalized;
            float distanceToPlayer = Vector3.Distance(_enemy.transform.position, _enemy.Target.position);

            // Check if there are obstacles blocking the view
            if (!Physics.Raycast(_enemy.transform.position, dirToPlayer, distanceToPlayer, _enemy.ObstacleMask))
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
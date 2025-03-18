using EnemySystem;
using SAS.StateMachineGraph;
using SAS.Utilities.BlackboardSystem;
using UnityEngine;

namespace EnemySystem
{
    public class IsTargetInFOV : ICustomCondition
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
            return (Vector3.Angle(_enemy.transform.forward, dirToPlayer) < _actor.GetValue<float>(_FOVKey) / 2);
        }



        void ICustomCondition.OnStateEnter()
        {
        }

        void ICustomCondition.OnStateExit()
        {
        }
    }
}
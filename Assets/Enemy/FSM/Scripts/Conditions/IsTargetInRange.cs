using SAS.StateMachineGraph;
using SAS.Utilities.BlackboardSystem;
using UnityEngine;

namespace EnemySystem
{
    public class IsTargetInRange : ICustomCondition
    {
        private Enemy _enemy;
        private BlackboardKey _rangeKey = default;
        private Actor _actor;

        void ICustomCondition.OnInitialize(Actor actor)
        {
            _enemy = actor.GetComponent<Enemy>();
            _rangeKey = actor.GetOrRegisterKey(EnemyBlackboardKey.ChaseRange);
            _actor = actor;

        }

        bool ICustomCondition.Evaluate()
        {
            return Vector3.Distance(_enemy.transform.position, _enemy.Target.position) < _actor.GetValue<float>(_rangeKey);
        }



        void ICustomCondition.OnStateEnter()
        {
        }

        void ICustomCondition.OnStateExit()
        {
        }
    }
}
using SAS.StateMachineGraph;
using UnityEngine;

namespace EnemySystem
{
    public static class EnemyBlackboardKey
    {
        public const string FOV = "FOV";
        public const string ChaseRange = "ChaseRange";

    }
    public class Enemy : MonoBehaviour
    {
        [field: SerializeField] public Transform Target { get; set; } //todo: need to set it programatically
        [field: SerializeField] public LayerMask ObstacleMask { get; private set; }
        [SerializeField] private string m_DeadStateName = "Dead";

        public void OnDeath()
        {
            GetComponent<Actor>().SetState(m_DeadStateName);
        }
    }
}

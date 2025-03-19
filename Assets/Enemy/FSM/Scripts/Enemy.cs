using SAS.StateMachineCharacterController;
using SAS.StateMachineGraph;
using UnityEngine;
using UnityEngine.UIElements;

namespace EnemySystem
{
    public static class EnemyBlackboardKey
    {
        public const string FOV = "FOV";
        public const string ChaseRange = "ChaseRange";

    }
    public class Enemy : MonoBehaviour, ICharacter
    {
        [field: SerializeField] public Transform Target { get; set; } //todo: need to set it programatically
        [field: SerializeField] public LayerMask ObstacleMask { get; private set; }
        Vector3 ICharacter.Position => transform.position;
        Vector3 ICharacter.Forward => transform.forward;

        [SerializeField] private string m_DeadStateName = "Dead";

        public void OnDeath()
        {
            GetComponent<Actor>().SetState(m_DeadStateName);
        }
    }
}

using SAS.Pool;
using SAS.StateMachineCharacterController;
using SAS.StateMachineGraph;
using UnityEngine;

namespace EnemySystem
{
    public static class EnemyBlackboardKey
    {
        public const string FOV = "FOV";
        public const string ChaseRange = "ChaseRange";
    }

    public interface IHasTarget
    {
        Transform Target { get; }
        LayerMask VisibilityBlockers { get; }
    }

    public class Enemy : MonoBehaviour, ICharacter, IHasTarget, ISpawnable
    {
        [field: SerializeField] public LayerMask VisibilityBlockers { get; private set; }
        [SerializeField] private string m_DeadStateTriggerName = "Dead";
        [SerializeField] private string m_SpawnTriggerName = "Spawn";
        [SerializeField] private SpawnablePoolSO m_HealerObjectPool;

        Vector3 ICharacter.Position => transform.position;
        Vector3 ICharacter.Forward => transform.forward;
        Transform ICharacter.Transform => transform;
        Transform IHasTarget.Target => _target.Transform;

        private ICharacter _target;

        private void Awake()
        {
            m_HealerObjectPool.Initialize(4);
        }

        void OnEnable()
        {
            if (_target == null)
                _target = FindAnyObjectByType<FSMCharacterController>() as ICharacter;
        }

        public void OnDeath()
        {
            GetComponent<Actor>().SetTrigger(m_DeadStateTriggerName);
        }

        public GameObject DropItem(Vector3 position)
        {
            var healer = m_HealerObjectPool.Spawn(position).gameObject;
            GetComponent<Poolable>().Despawn();
            return healer;
        }

        void ISpawnable.OnSpawn(object data)
        {
            GetComponent<IHealthPresenter>().HealthModel.Reset();
            GetComponent<Actor>().SetTrigger(m_SpawnTriggerName);
        }

        void ISpawnable.OnDespawn()
        {
        }
    }

}

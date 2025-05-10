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
        [field: SerializeField] public TargetingProfileSO TargetingProfile { get; private set; }

        public Vector3 Position => transform.position;
        public Vector3 Forward => transform.forward;
        public Transform Transform => transform;
        Transform IHasTarget.Target => _target.Transform;

        private ITarget _target;

        private void Awake()
        {
            m_HealerObjectPool.Initialize(4);
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

        public void SetTarget(ITarget bestTarget)
        {
            _target = bestTarget;
        }
    }
}
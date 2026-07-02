using UnityEngine;

namespace SAS.WeaponSystem.Components
{
    public abstract class WeaponComponent : MonoBehaviour
    {
        protected Weapon _weapon;

        protected IEventDispatcher _animationEventDispatcher;

        protected float attackStartTime => _weapon.AttackEndTime;

        protected bool isAttackActive;

        public virtual void Init()
        {
            _weapon = GetComponent<Weapon>();
            _animationEventDispatcher = GetComponentInParent<EventDispatcher>();
        }

        protected virtual void Start()
        {
            if (_weapon == null)
                _weapon = GetComponent<Weapon>();

            _weapon.OnEnter += HandleEnter;
            _weapon.OnExit += HandleExit;
        }

        protected virtual void HandleEnter()
        {
            isAttackActive = true;
        }

        protected virtual void HandleExit()
        {
            isAttackActive = false;
        }

        protected virtual void OnDestroy()
        {
            _weapon.OnEnter -= HandleEnter;
            _weapon.OnExit -= HandleExit;
        }
    }

    public abstract class WeaponComponent<T1, T2> : WeaponComponent where T1 : ComponentData<T2> where T2 : AttackData
    {
        protected T1 Data { get; private set; }
        protected T2 CurrentAttackData => Data.GetAttackData(_weapon.CurrentAttackCounter);

        public override void Init()
        {
            base.Init();

            Data = _weapon.Data.GetData<T1>();
        }
    }
}
using UnityEngine;

namespace SAS.WeaponSystem.Components
{
    public abstract class WeaponComponent : MonoBehaviour
    {
        protected Weapon _weapon;

        protected IEventDispatcher _animationEventDispatcher;

        protected float attackStartTime => _weapon.AttackStartTime;

        protected bool isAttackActive;

        public virtual void Init()
        {
            _weapon = GetComponent<Weapon>();
            _animationEventDispatcher = GetComponentInParent<EventDispatcher>();
        }

        protected virtual void Start()
        {
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
        protected T1 data;
        protected T2 currentAttackData;

        protected override void HandleEnter()
        {
            base.HandleEnter();

            currentAttackData = data.GetAttackData(_weapon.CurrentAttackCounter);
        }

        public override void Init()
        {
            base.Init();

            data = _weapon.Data.GetData<T1>();
        }
    }
}
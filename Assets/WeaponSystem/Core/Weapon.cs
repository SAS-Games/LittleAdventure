using System;
using UnityEngine;

namespace SAS.WeaponSystem
{
    public class Weapon : MonoBehaviour, IWeapon
    {
        public const string Tag = "";
        public float AttackStartTime { get; internal set; }
        public WeaponDataSO Data { get; private set; }

        private int currentAttackCounter;
        public int CurrentAttackCounter
        {
            get => currentAttackCounter;
            private set => currentAttackCounter = value >= Data.NumberOfAttacks ? 0 : value;
        }

        public bool CanEnterAttack { get; set; }
        public Action OnEnter { get; internal set; }
        public Action OnExit { get; internal set; }

        public bool IsInUse { get; private set; }
        private TimeNotifier attackCounterResetTimeNotifier;

        void IWeapon.Init()
        {
            //attackCounterResetTimeNotifier = new TimeNotifier();
            GetComponentInParent<IEventDispatcher>().Subscribe("OnAttackAnimationEnd", OnAttackAnimationEnd);
        }

        void IWeapon.Enter()
        {
            AttackStartTime = Time.time;
            //attackCounterResetTimeNotifier.Disable();
            Debug.Log("Enter", Tag);
            IsInUse = true;
            OnEnter?.Invoke();
        }

        void IWeapon.Exit()
        {
            Debug.Log("Exit", Tag);
            CurrentAttackCounter++;
            OnExit?.Invoke();
            IsInUse = false;
        }

        public void SetData(WeaponDataSO data)
        {
            Data = data;

            if (Data is null)
                return;

            ResetAttackCounter();
        }

        private void ResetAttackCounter()
        {
            print("Reset Attack Counter");
            CurrentAttackCounter = 0;
        }

        private void OnAttackAnimationEnd()
        {
            (this as IWeapon).Exit();
        }

        private void OnEnable()
        {
           // attackCounterResetTimeNotifier.OnNotify += ResetAttackCounter;
        }

        private void OnDisable()
        {
           // attackCounterResetTimeNotifier.OnNotify -= ResetAttackCounter;
        }
    }
}

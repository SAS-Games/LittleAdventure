using SAS.TimerSystem;
using System;
using System.Threading;
using UniRx;
using UnityEngine;

namespace SAS.WeaponSystem
{
    public class Weapon : MonoBehaviour, IWeapon
    {
        [SerializeField] private float m_AttackCounterResetCooldown;

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
        public Animator Animator { get; private set; }

        private CountdownTimer _attackCounterResetTimer = new CountdownTimer(1);
        private CancellationTokenSource _cts;

        private void Awake()
        {
            _attackCounterResetTimer = new CountdownTimer();
            Animator = GetComponentInParent<Animator>();
        }

        void IWeapon.Enter()
        {
            Debug.Log($"Enter{Time.time}", Tag);
            AttackStartTime = Time.time;
            Animator.SetInteger("Counter", currentAttackCounter);
            //_attackCounterResetTimer.Pause();
            WaitForAttackAnimFinish(Animator);
            IsInUse = true;
            OnEnter?.Invoke();
        }

        void IWeapon.Exit()
        {
           // Debug.Log($"Exit{Time.time}", Tag);
            CurrentAttackCounter++;
            _attackCounterResetTimer.Start(m_AttackCounterResetCooldown);
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

        private void OnEnable()
        {
            _attackCounterResetTimer.OnTimerStop += ResetAttackCounter;
        }

        private void OnDisable()
        {
            _attackCounterResetTimer.OnTimerStop -= ResetAttackCounter;
        }

        protected async void WaitForAttackAnimFinish(Animator animator)
        {
            _cts?.Cancel(); 
            _cts?.Dispose(); 
            _cts = new CancellationTokenSource(); 

            try
            {
                await Awaitable.NextFrameAsync();

                await animator.WhenStateExit("Attack").ToTask(_cts.Token);

                (this as IWeapon).Exit();
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Previous attack animation wait canceled.");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}

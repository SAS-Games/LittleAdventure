using SAS.TimerSystem;
using System;
using System.Threading;
using UniRx;
using UnityEngine;

namespace SAS.WeaponSystem
{
    public class Weapon : MonoBehaviour, IWeapon
    {
        public event Action<bool> OnCurrentInputChange;

        [field: SerializeField] public float AttackCounterResetCooldown { get; set; }

        public const string Tag = "";
        public float AttackEndTime { get; internal set; }
        public WeaponDataSO Data { get; private set; }

        private int _currentAttackCounter;

        public int CurrentAttackCounter
        {
            get => _currentAttackCounter;
            set => _currentAttackCounter = value >= Data.NumberOfAttacks ? 0 : value;
        }


        public bool CanEnterAttack { get; set; }
        public Action OnEnter { get; internal set; }
        public Action OnExit { get; internal set; }

        public bool IsInUse { get; set; }
        public Animator Animator { get; private set; }

        private bool _currentInput;

        public bool CurrentInput
        {
            get => _currentInput;
            set
            {
                if (_currentInput != value)
                {
                    _currentInput = value;
                    OnCurrentInputChange?.Invoke(_currentInput);
                }
            }
        }

        public CountdownTimer attackCounterResetTimer = new CountdownTimer(1);
        private CancellationTokenSource _cts;


        private void Awake()
        {
            Animator = GetComponentInParent<Animator>();
        }

        void IWeapon.Enter()
        {
            IsInUse = true;
            WaitForAttackAnimFinish(Animator);
            OnEnter?.Invoke();
        }

        void IWeapon.Exit()
        {
            CurrentAttackCounter++;
            attackCounterResetTimer.Start(AttackCounterResetCooldown);
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
            AttackEndTime = Time.time;
        }

        private void OnEnable()
        {
            attackCounterResetTimer.OnTimerStop += ResetAttackCounter;
        }

        private void OnDisable()
        {
            attackCounterResetTimer.OnTimerStop -= ResetAttackCounter;
        }

        protected async void WaitForAttackAnimFinish(Animator animator)
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
            }

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
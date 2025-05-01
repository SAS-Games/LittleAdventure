using SAS.TimerSystem;
using System;
using System.Threading;
using UniRx;
using UnityEngine;

namespace SAS.WeaponSystem
{
    public class Weapon : MonoBehaviour, IWeapon
    {
        public const string TAG = "Weapon";
        public event Action<bool> OnCurrentInputChange;

        [field: SerializeField] public float AttackCounterResetCooldown { get; set; }
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
            Debug.Log($"Weapon Enter {Time.frameCount} {CurrentAttackCounter}", TAG);
            attackCounterResetTimer.Pause();
            IsInUse = true;
            WaitForAttackAnimFinish(Animator);
            OnEnter?.Invoke();
        }

        void IWeapon.Exit()
        {
            Debug.Log($"Weapon Exit {Time.frameCount}", TAG);
            CurrentAttackCounter++;
            IsInUse = false;
            OnExit?.Invoke();
            attackCounterResetTimer.Reset(AttackCounterResetCooldown);
            attackCounterResetTimer.Start();
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
            Debug.Log("Reset Attack Counter", TAG);
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

        private async void WaitForAttackAnimFinish(Animator animator)
        {
            _cts?.Cancel();
            _cts?.Dispose();

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            try
            {
                await Awaitable.NextFrameAsync(); // Let the state fully start
                await animator.WhenStateExit("Attack").ToTask(token);
            }
            catch (OperationCanceledException)
            {
                // Animation was cancelled — handled below
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                (this as IWeapon).Exit();
            }
        }


        public void InterruptAttack()
        {
            _cts?.Cancel(); // This will trigger the catch block and call Exit
            _cts?.Dispose();
            _cts = null;
        }

        private void OnDestroy()
        {
            attackCounterResetTimer.Dispose();
        }
    }
}
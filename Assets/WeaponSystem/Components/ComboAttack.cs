using SAS.StateMachineGraph;
using UnityEngine;

namespace SAS.WeaponSystem.Components
{
    public class ComboAttack : WeaponComponent<ComboComponentData, ComboAttackData>
    {
        private const string TAG = "Combo";
        private float _lastInputTime = -Mathf.Infinity;
        private bool _playNextAttack = false;
        private Actor _actor;

        protected override void Start()
        {
            base.Start();
            _actor = _weapon.GetComponentInParent<Actor>();
            _weapon.OnCurrentInputChange += HandleInputChange;
        }

        protected override void HandleEnter()
        {
            base.HandleEnter();
            _weapon.Animator.Play($"Attack{_weapon.CurrentAttackCounter}", 0, 0);
        }

        private void HandleInputChange(bool isPressed)
        {
            if (!isPressed) // Prevent input during active combo
                return;
            float timeSinceLastInput = Time.time - _lastInputTime;

            if (timeSinceLastInput < Data.InputDelay)
            {
                Debug.Log($"Input ignored. Delay not met: {timeSinceLastInput:F2}s", TAG);
                return;
            }

            AnimatorStateInfo animState = _weapon.Animator.GetCurrentAnimatorStateInfo(0);
            if (animState.IsTag(CurrentAttackData.StateTag))
            {
                _playNextAttack = true;
                if (animState.normalizedTime < CurrentAttackData.RequiredAnimationProgress)
                {
                    Debug.Log($"Input ignored. Animation at {animState.normalizedTime * 100:F1}%", TAG);
                    return;
                }

                _weapon.InterruptAttack();
            }

            _lastInputTime = Time.time;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _weapon.OnCurrentInputChange -= HandleInputChange;
        }

        protected override async void HandleExit()
        {
            base.HandleExit();
            if (_playNextAttack)
            {
                Debug.Log("Queued input detected — triggering the next animation in sequence.", TAG);
                _playNextAttack = false;
                await Awaitable.NextFrameAsync(); // delay by 1 frame
                _actor.SetBool("Attack", true);
                await Awaitable.NextFrameAsync(); // delay by 1 frame
                _actor.SetBool("Attack", false);
            }
        }
    }
}
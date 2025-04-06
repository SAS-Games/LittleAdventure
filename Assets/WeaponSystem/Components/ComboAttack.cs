using UnityEngine;

namespace SAS.WeaponSystem.Components
{
    public class ComboAttack : WeaponComponent<ComboComponentData, ComboAttackData>
    {
        private const string TAG = "Combo";
        private float _lastInputTime = -Mathf.Infinity;

        protected override void Start()
        {
            base.Start();
            _weapon.OnCurrentInputChange += HandleInputChange;
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
                if (animState.normalizedTime < CurrentAttackData.RequiredAnimationProgress)
                {
                    Debug.Log($"Input ignored. Animation at {animState.normalizedTime * 100:F1}%", TAG);
                    return;
                }

                _weapon.CurrentAttackCounter++;
                Debug.Log($"Combo continues at {animState.normalizedTime * 100:F1} NextAttackCounter: {_weapon.CurrentAttackCounter}", TAG);
            }

            _lastInputTime = Time.time;
            if (_weapon.CurrentAttackCounter < 0)
                _weapon.CurrentAttackCounter = 0;
            _weapon.Animator.Play($"Attack{_weapon.CurrentAttackCounter}", 0, 0);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _weapon.OnCurrentInputChange -= HandleInputChange;
        }

        protected override void HandleExit()
        {
            base.HandleExit();
            _weapon.CurrentAttackCounter--; //reset counter to its previous value if increased by onExit
        }
    }
}
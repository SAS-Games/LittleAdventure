using SAS.StateMachineCharacterController;
using SAS.Utilities.TagSystem;
using UnityEngine;

namespace SAS.WeaponSystem.Components
{
    public class Movement : WeaponComponent<MovementData, MovementAttackData>
    {
        [FieldRequiresParent] private IMovementVectorHandler movementVectorHandler;
        private Vector3 _direction = Vector3.zero;
        private float _duration; // Timer for movement duration
        private float _elapsedTime = 0f; // Tracks time passed

        public override void Init()
        {
            base.Init();
            this.Initialize();
        }

        protected override void HandleEnter()
        {
            base.HandleEnter();
            _direction = (movementVectorHandler as Component).transform.forward;
            _duration = CurrentAttackData.Duration; // Set movement duration
            _elapsedTime = 0f; // Reset timer
        }

        private void Update()
        {
            if (!isAttackActive || _elapsedTime >= _duration)
                return;

            movementVectorHandler.MovementVector = CurrentAttackData.Velocity * _direction;
            _elapsedTime += Time.deltaTime;

            if (_elapsedTime >= _duration)
            {
                movementVectorHandler.MovementVector = Vector3.zero; // Stop movement after duration
            }
        }
    }
}

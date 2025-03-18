using SAS.StateMachineCharacterController;
using System;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

namespace SAS.WeaponSystem
{
    public class FireCommand : IInputCommand
    {
        private Action<CallbackContext> _firePerformed;
        private Action<CallbackContext> _fireCanceled;
        private InputAction _inputAction;

        public FireCommand(FSMCharacterController fsmcontroller)
        {
            _firePerformed = _ => fsmcontroller.OnFire();
            _fireCanceled = _ => fsmcontroller.OnFireCanceled();
        }

        public void SetActive(InputConfig inputConfig, bool active)
        {
            if (active)
            {
                if (_inputAction != null)
                {
                    _inputAction.performed -= _firePerformed;
                    _inputAction.canceled -= _fireCanceled;
                    _inputAction.Disable();
                }

                _inputAction = inputConfig.GetInputAction("Attack");
                if (_inputAction != null)
                {
                    _inputAction.performed += _firePerformed;
                    _inputAction.canceled += _fireCanceled;
                    _inputAction.Enable();
                }
            }
            else if (_inputAction != null)
            {
                _inputAction.performed -= _firePerformed;
                _inputAction.canceled -= _fireCanceled;
                _inputAction.Disable();
                _inputAction = null;
            }
        }
    }
}

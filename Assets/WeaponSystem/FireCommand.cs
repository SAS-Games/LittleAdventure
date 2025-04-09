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
        private string _inputActionKey;

        public FireCommand(string inputActionKey, Weapon weapon, FSMCharacterController fsmController)
        {
            _inputActionKey = inputActionKey;
            _firePerformed = _ =>
            {
                weapon.CurrentInput = true;
                fsmController.OnFire();
            };
            _fireCanceled = _ =>
            {
                weapon.CurrentInput = false;
                fsmController.OnFireCanceled();
            };
        }

        public void AddFirePerformedCallback(Action<CallbackContext> callback) => _firePerformed += callback;
        public void RemoveFirePerformedCallback(Action<CallbackContext> callback) => _firePerformed -= callback;
        public void AddFireCanceledCallback(Action<CallbackContext> callback) => _fireCanceled += callback;
        public void RemoveFireCanceledCallback(Action<CallbackContext> callback) => _fireCanceled -= callback;

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

                _inputAction = inputConfig.GetInputAction(_inputActionKey);
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
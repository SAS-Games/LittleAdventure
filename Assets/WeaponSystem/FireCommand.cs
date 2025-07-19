using SAS.StateMachineCharacterController;
using UnityEngine.InputSystem;

namespace SAS.WeaponSystem
{
    public class FireCommand : ChainedInputCommand
    {
        protected override string InputActionName { get; }

        public FireCommand(string inputActionKey, Weapon weapon, FSMCharacterController fsmController)
        {
            InputActionName = inputActionKey;
            AddHandler(InputActionPhase.Performed, new ConditionalInputHandler(() => true, _ =>
            {
                weapon.CurrentInput = true;
                fsmController.OnFire();
            }));

            AddHandler(InputActionPhase.Canceled, new ConditionalInputHandler(() => true, _ =>
            {
                weapon.CurrentInput = false;
                fsmController.OnFireCanceled();
            }));
        }
    }
}
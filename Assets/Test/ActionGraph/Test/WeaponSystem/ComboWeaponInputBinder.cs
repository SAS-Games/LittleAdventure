using SAS.StateMachineCharacterController;
using UnityEngine;

namespace SAS.ActionGraph.WeaponSystem
{
public class ComboWeaponInputBinder : MonoBehaviour
{
    [SerializeField] private ComboWeapon weapon;
    [SerializeField] private string attackInputKey = "Attack";
    [SerializeField] private bool forwardToFsmController;

    private void Start()
    {
        if (weapon == null)
            weapon = GetComponent<ComboWeapon>();

        if (weapon == null)
        {
            Debug.LogWarning($"{nameof(ComboWeaponInputBinder)} needs an {nameof(ComboWeapon)}.", this);
            return;
        }

        string inputKey = !string.IsNullOrEmpty(weapon.AttackInputKey) ? weapon.AttackInputKey : attackInputKey;

        InputHandler inputHandler = GetComponentInParent<InputHandler>();
        if (inputHandler == null)
        {
            Debug.LogWarning($"{nameof(ComboWeaponInputBinder)} could not find an InputHandler in parents.", this);
            return;
        }

        FSMCharacterController fsmController = forwardToFsmController ? GetComponentInParent<FSMCharacterController>() : null;
        if (inputHandler.TryGetInputCommand(inputKey, out IInputCommand existingCommand))
        {
            if (existingCommand is ChainedInputCommand chainedCommand)
            {
                ComboWeaponInputCommand.AddWeaponHandlers(chainedCommand, weapon, fsmController);
                return;
            }

            Debug.LogWarning($"{nameof(ComboWeaponInputBinder)} found existing input command '{inputKey}', but it is not a {nameof(ChainedInputCommand)}.", this);
            return;
        }

        var command = new ComboWeaponInputCommand(inputKey, weapon, fsmController);
        inputHandler.RegisterInputCommand(inputKey, command, true);
    }
}
}

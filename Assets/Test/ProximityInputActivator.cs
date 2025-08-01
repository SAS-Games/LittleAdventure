using SAS.StateMachineCharacterController;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = SAS.Debug;

public abstract class ProximityInputActivator : MonoBehaviour
{
    [SerializeField] private string m_InputActionName = "Interact";
    private readonly Dictionary<IInputHandler, InputAction> _activeBindings = new();
    protected abstract void OnInputPerformed(InputAction.CallbackContext context);

    public virtual void OnPlayerEntered(GameObject gameObject)
    {
        var inputHandler = gameObject.GetComponent<IInputHandler>();
        if (inputHandler == null || inputHandler.PlayerInput == null)
            return;

        if (_activeBindings.ContainsKey(inputHandler))
            return;

        var inputAction = inputHandler.PlayerInput.actions[m_InputActionName];
        if (inputAction == null)
        {
            Debug.LogWarning($"Input action '{m_InputActionName}' not found on player.", nameof(ProximityInputActivator));
            return;
        }

        inputAction.performed += OnInputPerformed;
        _activeBindings[inputHandler] = inputAction;
    }

    public virtual void OnPlayerExited(GameObject gameObject)
    {
        var inputHandler = gameObject.GetComponent<IInputHandler>();
        if (inputHandler == null || inputHandler.PlayerInput == null)
            return;

        if (_activeBindings.TryGetValue(inputHandler, out var inputAction))
        {
            inputAction.performed -= OnInputPerformed;
            _activeBindings.Remove(inputHandler);
        }
    }
}

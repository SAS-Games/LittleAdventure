using System;
using SAS.StateMachineCharacterController;
using SAS.StateMachineGraph;
using UnityEngine;
using UnityEngine.InputSystem;

public class AttackDashHandler : MonoBehaviour
{
    [SerializeField] private string m_DashStateName = "Dash";
    [SerializeField] private string m_AttackStateName = "Attack";
    [SerializeField] private string m_DashInputName = "Dash";
    private Actor _actor;
    private IConditionalInputHandler _handler;

    void Start()
    {
        _actor = GetComponentInParent<Actor>();
        var dashInputCommand = GetComponentInParent<InputHandler>().GetCommand(m_DashInputName);
        _handler = new ConditionalInputHandler(() => _actor.CurrentStateName == m_AttackStateName, DashCallback);
        dashInputCommand.AddHandler(InputActionPhase.Performed, _handler, 1);
    }

    private void DashCallback(InputAction.CallbackContext context)
    {
        _actor.SetState(m_DashStateName);
    }

    private void OnDestroy()
    {
        var inputCommand = GetComponentInParent<InputHandler>()?.GetCommand(m_DashInputName);
        inputCommand?.RemoveHandler(InputActionPhase.Performed, _handler);
    }
}
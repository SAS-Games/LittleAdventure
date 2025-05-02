using SAS.StateMachineCharacterController;
using SAS.StateMachineGraph;
using UnityEngine;

public class AttackDashHandler : MonoBehaviour
{
    [SerializeField] private string m_DashStateName = "Dash";
    [SerializeField] private string m_AttackStateName = "Attack";
    [SerializeField] private string m_DashInputName = "Dash";
    private Actor _actor;

    void Start()
    {
        _actor = GetComponentInParent<Actor>();
        var inputCommand = GetComponentInParent<InputHandler>().GetCommand(m_DashInputName);
        (inputCommand as IInputCallbackRegistry).RegisterCallback(DashCallback);
    }

    private void DashCallback()
    {
        if (_actor.CurrentStateName == m_AttackStateName)
            _actor.SetState(m_DashStateName);
    }
}
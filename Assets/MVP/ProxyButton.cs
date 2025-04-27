using SAS.Utilities.TagSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public interface IProxyButton
{
    void OnClick();
}

public class ProxyButton : MonoBehaviour, IProxyButton, ServiceLocator.IService
{
    [SerializeField] private InputActionReference m_InputActionReference;
    [SerializeField] private UnityEvent m_OnClick;

    private void Awake()
    {
        m_InputActionReference.action.performed += OnInputPerformed;
        m_InputActionReference.action.Enable();
    }

    private void OnInputPerformed(InputAction.CallbackContext context)
    {
        OnClick();
    }

    public void OnClick()
    {
        m_OnClick.Invoke();
    }
}
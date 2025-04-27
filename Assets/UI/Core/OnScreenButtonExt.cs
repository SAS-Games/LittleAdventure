using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;

public class OnScreenButtonExt : OnScreenButton
{
    [SerializeField] private InputActionReference m_InputActionReference;

    protected override void OnEnable()
    {
        if (m_InputActionReference != null)
        {
            if (m_InputActionReference.action != null &&
                m_InputActionReference.action.controls.Count > 0)
            {
                controlPath = m_InputActionReference.action.name;
            }

            m_InputActionReference.action.Enable();

            base.OnEnable();
        }
    }
}
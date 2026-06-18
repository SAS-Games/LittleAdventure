using UnityEngine;
using Debug = SAS.Debug;

public class ActivatableProxyBinder : ProxyViewBinder<bool>
{
    [SerializeField] private GameObject m_Target;

    protected override void OnValueChanged(bool value)
    {
        if (m_Target != null)
            m_Target.SetActive(value);
        else
            Debug.LogWarning($"{nameof(ActivatableProxyBinder)} on {gameObject.name} has no target assigned.");
    }
}
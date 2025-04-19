using SAS.StateMachineGraph.Utilities;
using UnityEngine;

public class ComponentActivator : MonoBehaviour, IActivatable
{
    [SerializeField] private Component[] m_Components;

    void IActivatable.Activate()
    {
        SetActive(true);
    }

    void IActivatable.Deactivate()
    {
        SetActive(false);
    }

    private void SetActive(bool status)
    {
        foreach (var component in m_Components)
        {
            if (component is Behaviour behaviour)
                behaviour.enabled = status;
            else if (component is Collider collider)
                collider.enabled = status;
        }
    }
}

using System.Collections.Generic;
using SAS.Utilities.TagSystem;
using UnityEngine;

public sealed class EventBinderBehaviour : MonoBehaviour
{
    [FieldRequiresSelf] private IEventDispatcher _dispatcher;

    [SerializeReference, SerializeField] private List<EventHook> m_EventBindings = new();

    void Awake()
    {
        this.Initialize();
        foreach (var binding in m_EventBindings)
        {
            binding.Subscribe(_dispatcher);
        }
    }

    private void OnDestroy()
    {
        foreach (var binding in m_EventBindings)
        {
            binding.Unsubscribe(_dispatcher);
        }
    }
}
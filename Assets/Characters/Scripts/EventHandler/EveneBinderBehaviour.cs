using System.Collections.Generic;
using SAS.Utilities.TagSystem;
using UnityEngine;

public class EventBinderBehaviour : MonoBehaviour
{
    [FieldRequiresSelf] protected IEventDispatcher _dispatcher;

    [SerializeReference, SerializeField] protected List<EventHook> m_EventBindings = new();

    protected virtual void Awake()
    {
        this.Initialize();
        foreach (var binding in m_EventBindings)
        {
            binding.Subscribe(_dispatcher);
        }
    }
}
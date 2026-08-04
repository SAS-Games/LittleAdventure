using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public abstract class EventHook
{
    [SerializeField] protected string m_EventName;
    public abstract void Subscribe(IEventDispatcher dispatcher);
    public abstract void Unsubscribe(IEventDispatcher dispatcher);
}

[Serializable]
public class VoidEventHook : EventHook
{
    [SerializeField] private UnityEvent m_OnEventTrigger;

    public override void Subscribe(IEventDispatcher dispatcher)
    {
        dispatcher.Subscribe(m_EventName, m_OnEventTrigger.Invoke);
    }

    public override void Unsubscribe(IEventDispatcher dispatcher)
    {
        dispatcher.Unsubscribe(m_EventName, m_OnEventTrigger.Invoke);
    }
}

[Serializable]
public class BoolEventHook : EventHook
{
    [SerializeField] private UnityEvent<bool> m_OnEventTrigger;

    public override void Subscribe(IEventDispatcher dispatcher)
    {
        dispatcher.Subscribe<bool>(m_EventName, m_OnEventTrigger.Invoke);
    }

    public override void Unsubscribe(IEventDispatcher dispatcher)
    {
        dispatcher.Unsubscribe<bool>(m_EventName, m_OnEventTrigger.Invoke);
    }
}

[Serializable]
public class IntEventHook : EventHook
{
    [SerializeField] private UnityEvent<int> m_OnEventTrigger;

    public override void Subscribe(IEventDispatcher dispatcher)
    {
        dispatcher.Subscribe<int>(m_EventName, m_OnEventTrigger.Invoke);
    }

    public override void Unsubscribe(IEventDispatcher dispatcher)
    {
        dispatcher.Unsubscribe<int>(m_EventName, m_OnEventTrigger.Invoke);
    }
}

[Serializable]
public class Vector3EventHook : EventHook
{
    [SerializeField] private UnityEvent<Vector3> m_OnEventTrigger;

    public override void Subscribe(IEventDispatcher dispatcher)
    {
        dispatcher.Subscribe<Vector3>(m_EventName, m_OnEventTrigger.Invoke);
    }

    public override void Unsubscribe(IEventDispatcher dispatcher)
    {
        dispatcher.Unsubscribe<Vector3>(m_EventName, m_OnEventTrigger.Invoke);
    }
}
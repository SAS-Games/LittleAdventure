using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IEventDispatcher
{
    void Subscribe(string eventName, Action callback);
    void Subscribe<T>(string eventName, Action<T> callback);
    void Unsubscribe(string eventName, Action callback);
    void Unsubscribe<T>(string eventName, Action<T> callback);
    void TriggerEvent(string eventName);
    void TriggerParamEvent(CustomParam eventParam);
    void TriggerParamEvent<T>(string eventName, T value);
}

[System.Serializable]
public abstract class CustomParam : ScriptableObject
{
    [field: SerializeField] public string EventName { get; private set; }

    // Abstract method to retrieve the parameter value as an object
    public abstract object GetValue();
}

// Generic version for different data types
[System.Serializable]
public class CustomParam<T> : CustomParam
{
    [field: SerializeField] public T Param { get; set; }

    public override object GetValue() => Param;
}

public class EventDispatcher : MonoBehaviour, IEventDispatcher
{
    private Dictionary<string, Action> _parameterlessCallbacks = new();
    private Dictionary<string, Action<object>> _parameterizedCallbacks = new();

    public void TriggerParamEvent(CustomParam eventParam)
    {
        if (eventParam == null) return;

        string eventName = eventParam.EventName;
        object paramValue = eventParam.GetValue();

        if (_parameterizedCallbacks.TryGetValue(eventName, out var paramCallback))
            paramCallback?.Invoke(paramValue); // Invoke parameterized callback

        if (_parameterlessCallbacks.TryGetValue(eventName, out var noParamCallback))
            noParamCallback?.Invoke(); // Invoke parameterless callback
    }

    public void TriggerEvent(string eventName)
    {
        if (_parameterlessCallbacks.TryGetValue(eventName, out var callback))
            callback?.Invoke();
    }

    public void TriggerParamEvent<T>(string eventName, T value)
    {
        if (_parameterizedCallbacks.TryGetValue(eventName, out var paramCallback))
            paramCallback?.Invoke(value); // Invoke parameterized callback

        if (_parameterlessCallbacks.TryGetValue(eventName, out var noParamCallback))
            noParamCallback?.Invoke(); // Invoke parameterless callback
    }

    public void Subscribe(string eventName, Action callback)
    {
        if (!_parameterlessCallbacks.ContainsKey(eventName))
            _parameterlessCallbacks[eventName] = callback;
        else
            _parameterlessCallbacks[eventName] += callback;
    }


    public void Subscribe<T>(string eventName, Action<T> callback)
    {
        Action<object> wrapper = obj => callback((T)obj); // Ensure correct casting

        if (!_parameterizedCallbacks.ContainsKey(eventName))
            _parameterizedCallbacks[eventName] = wrapper;
        else
            _parameterizedCallbacks[eventName] += wrapper;
    }

    public void Unsubscribe(string eventName, Action callback)
    {
        if (_parameterlessCallbacks.ContainsKey(eventName))
        {
            _parameterlessCallbacks[eventName] -= callback;
            if (_parameterlessCallbacks[eventName] == null)
                _parameterlessCallbacks.Remove(eventName);
        }
    }

    public void Unsubscribe<T>(string eventName, Action<T> callback)
    {
        Action<object> wrapper = obj => callback((T)obj);

        if (_parameterizedCallbacks.ContainsKey(eventName))
        {
            _parameterizedCallbacks[eventName] -= wrapper;
            if (_parameterizedCallbacks[eventName] == null)
                _parameterizedCallbacks.Remove(eventName);
        }
    }
}
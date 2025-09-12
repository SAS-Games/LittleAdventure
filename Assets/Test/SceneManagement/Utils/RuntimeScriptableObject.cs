using UnityEngine;

/// <summary>
/// ScriptableObject that gives a unique runtime instance without modifying the asset itself.
/// </summary>
public abstract class RuntimeScriptableObject<T> : ScriptableObject where T : RuntimeScriptableObject<T>
{
    private T _runtimeInstance;

    /// <summary>
    /// Returns the runtime instance, lazy-created from the asset
    /// </summary>
    public T Instance
    {
        get
        {
            if (_runtimeInstance == null)
            {
                _runtimeInstance = Instantiate(this) as T;
                _runtimeInstance.OnInstanceCreated();
            }
            return _runtimeInstance;
        }
    }

    /// <summary>
    /// Called when a runtime instance is created
    /// </summary>
    protected virtual void OnInstanceCreated()
    {
    }

    /// <summary>
    /// Implicit conversion so you can use the asset directly as its runtime instance
    /// </summary>
    public static implicit operator T(RuntimeScriptableObject<T> original)
    {
        return original.Instance;
    }
}
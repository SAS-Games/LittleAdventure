using System;
using System.Collections.Generic;
using Ink.Runtime;

public delegate object InkExternalMethod(params object[] args);

public class InkExternalMethodRegistry
{
    private readonly Dictionary<string, InkExternalMethod> _methods = new();

    public void Register(string methodName, InkExternalMethod method)
    {
        _methods[methodName] = method;
    }

    public void Register<T>(string methodName, Func<T> func)
    {
        Register(methodName, Wrap(func));
    }

    public void Register<T>(string methodName, Action<T> action)
    {
        Register(methodName, Wrap(action));
    }

    public void Register(string methodName, Action action)
    {
        Register(methodName, Wrap(action));
    }

    public void Bind(Story story)
    {
        foreach (var pair in _methods)
        {
            story.BindExternalFunction(pair.Key, (object[] args) =>
            {
                try
                {
                    return pair.Value(args);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"Error invoking ink method '{pair.Key}': {e}");
                    return null;
                }
            });
        }
    }

    public void Unbind(Story story)
    {
        foreach (var pair in _methods)
        {
            story.UnbindExternalFunction(pair.Key);
        }
    }

    // Wrappers
    public static InkExternalMethod Wrap(Func<object> func)
        => (args) => func();

    public static InkExternalMethod Wrap<T>(Func<T> func)
        => (args) => func();

    public static InkExternalMethod Wrap<T1, TResult>(Func<T1, TResult> func)
        => (args) => func((T1)args[0]);

    public static InkExternalMethod Wrap<T1, T2, TResult>(Func<T1, T2, TResult> func)
        => (args) => func((T1)args[0], (T2)args[1]);

    public static InkExternalMethod Wrap(Action action)
        => (args) => { action(); return null; };

    public static InkExternalMethod Wrap<T>(Action<T> action)
        => (args) => { action((T)args[0]); return null; };

    public static InkExternalMethod Wrap<T1, T2>(Action<T1, T2> action)
        => (args) => { action((T1)args[0], (T2)args[1]); return null; };
}

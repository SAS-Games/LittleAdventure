using System;
using System.Collections.Generic;
using Ink.Runtime;

public delegate object InkExternalMethod(params object[] args);

public class InkExternalMethodRegistry
{
    private readonly Dictionary<string, Delegate> _methods = new();

    private void RegisterInternal(string name, Delegate method)
    {
        _methods[name] = method;
    }

    public void Register(Delegate method) => RegisterInternal(method.Method.Name, method);
    public void Register(string methodName, Delegate method) => RegisterInternal(methodName, method);

    public void Register<TResult>(Func<TResult> method) => RegisterInternal(method.Method.Name, method);
    public void Register<T1, TResult>(Func<T1, TResult> method) => RegisterInternal(method.Method.Name, method);
    public void Register<T1, T2, TResult>(Func<T1, T2, TResult> method) => RegisterInternal(method.Method.Name, method);
    public void Register<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> method) => RegisterInternal(method.Method.Name, method);
    public void Register<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> method) => RegisterInternal(method.Method.Name, method);

    public void Register<TResult>(string inkFunction, Func<TResult> method) => RegisterInternal(inkFunction, method);
    public void Register<T1, TResult>(string inkFunction, Func<T1, TResult> method) => RegisterInternal(inkFunction, method);
    public void Register<T1, T2, TResult>(string inkFunction, Func<T1, T2, TResult> method) => RegisterInternal(inkFunction, method);
    public void Register<T1, T2, T3, TResult>(string inkFunction, Func<T1, T2, T3, TResult> method) => RegisterInternal(inkFunction, method);
    public void Register<T1, T2, T3, T4, TResult>(string inkFunction, Func<T1, T2, T3, T4, TResult> method) => RegisterInternal(inkFunction, method);

    public void Register(Action method) => RegisterInternal(method.Method.Name, method);
    public void Register<T1>(Action<T1> method) => RegisterInternal(method.Method.Name, method);
    public void Register<T1, T2>(Action<T1, T2> method) => RegisterInternal(method.Method.Name, method);
    public void Register<T1, T2, T3>(Action<T1, T2, T3> method) => RegisterInternal(method.Method.Name, method);
    public void Register<T1, T2, T3, T4>(Action<T1, T2, T3, T4> method) => RegisterInternal(method.Method.Name, method);

    public void Register(string inkFunction, Action method) => RegisterInternal(inkFunction, method);
    public void Register<T1>(string inkFunction, Action<T1> method) => RegisterInternal(inkFunction, method);
    public void Register<T1, T2>(string inkFunction, Action<T1, T2> method) => RegisterInternal(inkFunction, method);
    public void Register<T1, T2, T3>(string inkFunction, Action<T1, T2, T3> method) => RegisterInternal(inkFunction, method);
    public void Register<T1, T2, T3, T4>(string inkFunction, Action<T1, T2, T3, T4> method) => RegisterInternal(inkFunction, method);

    public void Bind(Story story)
    {
        foreach (var pair in _methods)
        {
            var methodName = pair.Key;
            var method = pair.Value;
            var methodInfo = method.Method;
            int paramCount = methodInfo.GetParameters().Length;

            try
            {
                switch (paramCount)
                {
                    case 0:
                        story.BindExternalFunction(methodName, () => method.DynamicInvoke());
                        break;

                    case 1:
                        story.BindExternalFunction(methodName, (object arg1) => method.DynamicInvoke(arg1));
                        break;

                    case 2:
                        story.BindExternalFunction(methodName,
                            (object arg1, object arg2) => method.DynamicInvoke(arg1, arg2));
                        break;
                    case 3:
                        story.BindExternalFunction(methodName,
                            (object arg1, object arg2, object arg3) =>
                                method.DynamicInvoke(arg1, arg2, arg3));
                        break;
                    case 4:
                        story.BindExternalFunction(methodName,
                            (object arg1, object arg2, object arg3, object arg4) =>
                                method.DynamicInvoke(arg1, arg2, arg3, arg4));
                        break;
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Error binding method '{methodName}': {e}");
            }
        }
    }

    public void Unbind(Story story)
    {
        foreach (var pair in _methods)
        {
            story.UnbindExternalFunction(pair.Key);
        }
    }
}
using System;
using System.Collections.Generic;
using Ink.Runtime;

public delegate object InkExternalMethod(params object[] args);

public class InkExternalMethodRegistry
{
    private readonly Dictionary<string, Delegate> _methods = new();

    public void Register(string methodName, Delegate method)
    {
        _methods[methodName] = method;
    }
    
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
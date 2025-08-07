using System;

public static class InkMethodRegistryExt
{
    public static void Register(this InkExternalMethodRegistry registry, Action method)
    {
        registry.Register(method.Method.Name, InkExternalMethodRegistry.Wrap(method));
    }

    public static void Register<T>(this InkExternalMethodRegistry registry, Action<T> method)
    {
        registry.Register(method.Method.Name, InkExternalMethodRegistry.Wrap(method));
    }

    public static void Register<T>(this InkExternalMethodRegistry registry, Func<T> method)
    {
        registry.Register(method.Method.Name, InkExternalMethodRegistry.Wrap(method));
    }

    public static void Register<T1, TResult>(this InkExternalMethodRegistry registry, Func<T1, TResult> method)
    {
        registry.Register(method.Method.Name, InkExternalMethodRegistry.Wrap(method));
    }

    public static void Register<T1, T2, TResult>(this InkExternalMethodRegistry registry, Func<T1, T2, TResult> method)
    {
        registry.Register(method.Method.Name, InkExternalMethodRegistry.Wrap(method));
    }

    public static void Register<T1, T2>(this InkExternalMethodRegistry registry, Action<T1, T2> method)
    {
        registry.Register(method.Method.Name, InkExternalMethodRegistry.Wrap(method));
    }
}
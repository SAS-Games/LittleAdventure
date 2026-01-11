using System;
using System.Collections.Generic;
using SAS.Core.TagSystem;
using UnityEngine;


[DefaultExecutionOrder(-100)]
public class ServiceRegistry : MonoBehaviour, ICore
{
    private IServiceLocator _serviceLocator = new ServiceLocator();

    public void Init()
    {
        var services = SceneUtility.FindComponentsInScene<ServiceLocator.IService>(gameObject.scene.name);
        foreach (var service in services)
        {
            Add(service.GetType(), service, (service as Component).GetTag());
        }
    }

    public bool TryGet<T>(out T instance, Tag tag)
    {
        return _serviceLocator.TryGet<T>(out instance, tag);
    }

    public IEnumerable<T> GetAll<T>(Tag tag = default)
    {
        return _serviceLocator.GetAll<T>( tag);
    }

    private bool TryGet(Type type, out object instance, Tag tag = default)
    {
        return _serviceLocator.TryGet(type, out instance, tag);
    }


    public void Add<T>(object instance, Tag tag = default)
    {
        Add(typeof(T), instance, tag);
    }

    public void Add(Type type, object instance, Tag tag = default)
    {
        _serviceLocator.Add(type, instance, tag);
    }
}
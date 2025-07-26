using System;
using System.Collections.Generic;
using SAS.Utilities.TagSystem;
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

    public IEnumerable<T> GetAll<T>(Tag tag = Tag.None)
    {
        return _serviceLocator.GetAll<T>( tag);
    }

    private bool TryGet(Type type, out object instance, Tag tag = Tag.None)
    {
        return _serviceLocator.TryGet(type, out instance, tag);
    }


    public void Add<T>(object instance, Tag tag = Tag.None)
    {
        Add(typeof(T), instance, tag);
    }

    public void Add(Type type, object instance, Tag tag = Tag.None)
    {
        _serviceLocator.Add(type, instance, tag);
    }
}
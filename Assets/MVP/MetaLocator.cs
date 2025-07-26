using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SAS.StateMachineGraph.Utilities;
using SAS.Utilities.TagSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZLinq;
using Object = UnityEngine.Object;

interface IMetaLocator : IBindable
{
    void CacheLocalMeta(IContextBinder contextBinder);
    void AddHandlers(IEnumerable<MetaLocator.IHandler> handlers);
    Task<bool> InjectInto(Scene gameScene);
    void RemoveHandlers(IEnumerable<MetaLocator.IHandler> handlers);
}

public partial class MetaLocator : MonoBehaviour, IMetaLocator, IActivatable
{
    private static readonly Type[] UnityBaseTypes =
    {
            typeof(UnityEngine.MonoBehaviour),
            typeof(UnityEngine.Behaviour),
            typeof( UnityEngine.Component),
            typeof(UnityEngine.Object),
    };

    public interface IHandler
    {
        void OnMetaLoaded(MetaLocator metaLocator);
        void OnCoreLoaded(MetaLocator metaLocator);
    }

    public interface IReset
    {
        void Reset();
    }

    private List<IHandler> _handlers = new List<IHandler>();
    private Dictionary<Key, List<object>> _localMeta = new Dictionary<Key, List<object>>();
    private ICore _core;

    private void Awake()
    {
        foreach (var handler in _handlers)
            (handler as IReset)?.Reset();

        _handlers.Clear();
        _localMeta.Clear();

        var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var rootObject in rootObjects)
        {
            AddHandlers(rootObject.GetComponentsInChildren<IHandler>(true));
        }
    }

    void IMetaLocator.CacheLocalMeta(IContextBinder contextBinder)
    {
        foreach (var keyValue in contextBinder.GetAll())
        {
            if (!_localMeta.ContainsKey(keyValue.Key))
                _localMeta.Add(keyValue.Key, new List<object>() { keyValue.Value });
            else
                Debug.LogWarning($"An item with the same key has already been added. Key: {keyValue.Key}");
        }
    }

    public void AddHandlers(IEnumerable<IHandler> handlers)
    {
        foreach (var handler in handlers)
        {
            _handlers.Add(handler);
            handler.OnMetaLoaded(this);
        }
    }

    void IMetaLocator.RemoveHandlers(IEnumerable<IHandler> handlers)
    {
        foreach (var handler in handlers)
        {
            if (handler != null)
                _handlers.Remove(handler);
        }

        _handlers.RemoveAll(h => h == null);
    }

    public async Task<bool> InjectInto(Scene gameScene)
    {
        foreach (var root in gameScene.GetRootGameObjects())
        {
            AddHandlers(root.GetComponentsInChildren<IHandler>(true));
        }

        // Search all root objects for core and inject it if found.
        var roots = gameScene.GetRootGameObjects();
        foreach (var root in roots)
        {
            var core = root.GetComponent<ICore>();
            if (core != null)
            {
                _core = core;
                AddLocalToCore(core);

                var readyDependencyGroup = root.GetComponentInChildren<ReadyDependencyGroup>();
                if (readyDependencyGroup != null)
                    await readyDependencyGroup.WaitUntilReadyAsync();

                _core.Init();
                foreach (var handler in _handlers)
                    handler.OnCoreLoaded(this);

                return true;
            }
        }

        return false;
    }

    public bool TryGet<T>(out T instance, Tag tag = Tag.None)
    {
        if (_core as Object == null || !_core.TryGet(out instance, tag))
        {
            var key = GetKey(typeof(T), tag);
            if (!_localMeta.TryGetValue(key, out var result))
            {
                instance = default;
                Debug.LogError($"Required service of type {typeof(T).Name} with tag {tag} is not found");
                return false;
            }

            if (result.Count > 1)
                Debug.LogWarning($"There is more than one IService that implements{typeof(T).Name}");


            instance = (T)result[0];
            return true;
        }

        return true;
    }

    public IEnumerable<T> GetAll<T>(Tag tag = Tag.None)
    {
        if (_core as Object != null)
        {
            var coreResults = _core.GetAll<T>(tag);
            if (coreResults != null && coreResults.Any())
                return coreResults;
        }

        var key = GetKey(typeof(T), tag);
        if (_localMeta.TryGetValue(key, out var result))
            return result.OfType<T>();

        Debug.LogError($"Required service(s) of type {typeof(T).Name} with tag {tag} not found");
        return Enumerable.Empty<T>();
    }

    private void AddLocalToCore(ICore core)
    {
        foreach (var entry in _localMeta)
        {
            var service = entry.Value;
            core.Add(entry.Key.type, service, entry.Key.tag);
        }
    }

    public void Add<T>(T service, Tag tag = Tag.None)
    {
        AddToLocal(typeof(T), service, tag);
        if (_core as Object != null)
        {
            _core.Add<T>(service, tag);
        }
    }

    private void AddToLocal(Type type, object service, Tag tag = Tag.None)
    {
        var key = GetKey(type, tag);
        if (!_localMeta.TryGetValue(key, out var instanceList))
        {
            instanceList = new List<object>();
            _localMeta.Add(key, instanceList);
        }

        if (!instanceList.Contains(service))
            instanceList.Add(service);

        if (UnityBaseTypes.AsValueEnumerable().Any(stopType => stopType == type))
            return;

        var baseTypes = type.GetInterfaces();
        if (type.BaseType != null && !UnityBaseTypes.Any(ut => ut == type.BaseType))
            baseTypes = baseTypes.AsValueEnumerable().Prepend(type.BaseType).ToArray();

        foreach (var baseType in baseTypes)
            AddToLocal(baseType, service, tag);
    }

    private Key GetKey(Type type, Tag tag)
    {
        return new Key { type = type, tag = tag };
    }

    public void Activate()
    {
        enabled = true;
    }

    public void Deactivate()
    {
        enabled = false;
    }

    private void OnDestroy()
    {
        _localMeta.Clear();
        _handlers.Clear();
    }

    public void OnInstanceCreated()
    {
        Debug.Log("OnInstanceCreated MetaLocator");
    }
}
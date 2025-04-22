using SAS.StateMachineGraph.Utilities;
using SAS.Utilities;
using SAS.Utilities.TagSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class MetaLocator : MonoBehaviour, IActivatable, IIniter
{
    public interface IHandler
    {
        void OnMetaLoaded(MetaLocator metaLocator);
        void OnCoreLoaded(MetaLocator metaLocator);
    }
    public interface IReset
    {
        void Reset();
    }

    [SerializeField] BaseContextBinder m_ContextBinder;
    private List<IHandler> _handlers = new List<IHandler>();
    private Dictionary<Key, object> _localMeta = new Dictionary<Key, object>();
    private ICore _core;
    private IContextBinder _contextBinder => m_ContextBinder;

    public Task Init()
    {
        var tcs = new TaskCompletionSource<bool>();
        StaticCoroutine.Start(Init(tcs));
        return tcs.Task;
    }

    private IEnumerator Init(TaskCompletionSource<bool> tcs)
    {
        foreach (var handler in _handlers)
            (handler as IReset)?.Reset();
        
        _handlers.Clear();
        _localMeta.Clear();
        
        foreach (var keyValue in _contextBinder.GetAll())
        {
            if (!_localMeta.ContainsKey(keyValue.Key))
                _localMeta.Add(keyValue.Key, keyValue.Value);
            else
                Debug.Log($"An item with the same key has already been added. Key: {keyValue.Key}");
        }
        yield return null;
        var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var rootObject in rootObjects)
        {
            AddHandlers(rootObject.GetComponentsInChildren<IHandler>(true));
        }
        tcs.SetResult(true);
    }

    public void AddHandlers(IEnumerable<IHandler> handlers)
    {
        foreach (var handler in handlers)
        {
            _handlers.Add(handler);
            handler.OnMetaLoaded(this);
        }
    }

    public void RemoveHandlers(IEnumerable<IHandler> handlers)
    {
        foreach (var handler in handlers)
        {
            _handlers.Remove(handler);
        }
    }

    public bool InjectInto(Scene gameScene)
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
                _core.Init();
                foreach (var handler in _handlers)
                {
                    handler.OnCoreLoaded(this);
                }
                return true;
            }
        }

        return false;
    }

    public bool TryGet<T>(out T instance, Tag tag = Tag.None)
    {
        if (_core as UnityEngine.Object == null || !_core.TryGet(out instance, tag))
        {
            var key = GetKey(typeof(T), tag);
            if (!_localMeta.TryGetValue(key, out object result))
            {
                instance = default;
                Debug.LogError($"Required service of type {typeof(T).Name} with tag {tag} is not found");
                return false;
            }
            instance = (T)result;
            return true;
        }
        return true;
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
        if (_core as UnityEngine.Object != null)
        {
            _core.Add<T>(service, tag);
        }
    }

    private void AddToLocal(Type type, object service, Tag tag = Tag.None)
    {
        var key = GetKey(type, tag);
        if (!_localMeta.TryGetValue(key, out var instance))
            _localMeta.Add(key, service);

        var baseTypes = type.GetInterfaces();
        if (type.BaseType != null)
            baseTypes = baseTypes.Prepend(type.BaseType).ToArray();

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
}

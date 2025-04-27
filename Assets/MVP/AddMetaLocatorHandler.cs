using System.Collections.Generic;
using SAS.StateMachineGraph.Utilities;
using SAS.Utilities.TagSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AddMetaLocatorHandler : MonoBehaviour
{
    [Inject] private IMetaLocator _metaLocator;
    [SerializeField] BaseContextBinder m_ContextBinder;
    private List<MetaLocator.IHandler> _handlers = new List<MetaLocator.IHandler>();

    private void Start()
    {
        this.InjectFieldBindings();
        AddMetaLocator();
    }

    private void AddMetaLocator()
    {
        if (m_ContextBinder && !m_ContextBinder.IsCrossContextBinder)
            _metaLocator.CacheLocalMeta(m_ContextBinder);
        Scene scene = gameObject.scene;
        var rootObjects = scene.GetRootGameObjects();
        foreach (var rootObject in rootObjects)
        {
            var handlers = rootObject.GetComponentsInChildren<MetaLocator.IHandler>(true);
            if (handlers.Length > 0)
            {
                _handlers.AddRange(handlers);
                _metaLocator.AddHandlers(handlers);
            }
        }
    }

    void OnDestroy()
    {
        _metaLocator.RemoveHandlers(_handlers);
    }
}
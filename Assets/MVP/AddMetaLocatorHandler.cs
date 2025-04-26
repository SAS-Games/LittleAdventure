using SAS.Utilities.TagSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AddMetaLocatorHandler : MonoBehaviour
{
    [Inject] private IMetaLocator _metaLocator;
    [SerializeField] BaseContextBinder m_ContextBinder;
    private MetaLocator.IHandler[] _handlers;

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
            _handlers = rootObject.GetComponentsInChildren<MetaLocator.IHandler>(true);
            _metaLocator.AddHandlers(_handlers);
        }
    }
}
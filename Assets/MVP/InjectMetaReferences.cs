using SAS.SceneManagement;
using SAS.Utilities.TagSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InjectMetaReferences : MonoBehaviour
{
    [SerializeField] private string[] m_InjectionSceneGroups;
    [Inject] private IMetaLocator _metaLocator;
    private EventBinding<SceneGroupLoadedEvent> _sceneGroupLoadedEventBinding;
    

    void Start()
    {
        this.InjectFieldBindings();
        _sceneGroupLoadedEventBinding = new EventBinding<SceneGroupLoadedEvent>(OnSceneGroupLoaded);
        EventBus<SceneGroupLoadedEvent>.Register(_sceneGroupLoadedEventBinding);
    }

    void OnSceneGroupLoaded(SceneGroupLoadedEvent sceneGroupLoadedEvent)
    {
        var sceneGroup = sceneGroupLoadedEvent.sceneGroup;
        foreach (var groupName in m_InjectionSceneGroups)
        {
            if (groupName == sceneGroup.Name)
            {
                Inject(sceneGroup.GetActiveScene());
                return;
            }
        }
    }

    private void Inject(Scene scene)
    {
        if (scene.isLoaded)
            _metaLocator?.InjectInto(scene);
    }
}
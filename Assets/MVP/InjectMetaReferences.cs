using SAS.Utilities.TagSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InjectMetaReferences : MonoBehaviour
{
    [Inject] private IMetaLocator _metaLocator;

    void Start()
    {
        this.InjectFieldBindings();
        Inject();
    }

    private void Inject()
    {
        Scene scene = gameObject.scene;
        if (scene.isLoaded)
            _metaLocator.InjectInto(scene);
    }
}
using SAS.SceneManagement;
using SAS.Utilities.TagSystem;
using UnityEngine;

public class DeferredSceneFader : SceneFader
{
    [Inject] private ISceneLoader _sceneLoader;
    [SerializeField] private string m_SceneName;

    protected override void Awake()
    {
        base.Awake();
        this.Initialize();
    }
    async public override void SetActive(bool active)
    {
        if (active)
            base.SetActive(active);
        else
        {
            var readyDependencyGroup = SceneUtility.FindComponentInScene<ReadyDependencyGroup>(m_SceneName);
            if (readyDependencyGroup != null)
                await readyDependencyGroup.WaitUntilReadyAsync();
            base.SetActive(false);
        }
    }
}

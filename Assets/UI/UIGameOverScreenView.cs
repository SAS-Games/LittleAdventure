using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIGameOverScreenView : UIScreenView
{
    [SerializeField] private UIButton m_MainMenuButton;
    [SerializeField] private UIButton m_RestartButton;
    [FormerlySerializedAs("m_MainSceneGroupLoader")] [SerializeField] private SceneGroupLoadConfig mMainSceneGroupLoadConfig;
    [FormerlySerializedAs("m_GameSceneGroupLoader")] [SerializeField] private SceneGroupLoadConfig mGameSceneGroupLoadConfig;
    private EventBinding<GameOverEvent> _gameOverEventBinding;

    protected override void Awake()
    {
        base.Awake();
        gameObject.SetActive(false);
        _gameOverEventBinding = new EventBinding<GameOverEvent>(_ => gameObject.SetActive(true));
        EventBus<GameOverEvent>.Register(_gameOverEventBinding);
    }

    public override void OnButtonClick(UIButton button, BaseEventData eventData)
    {
        if (button == m_MainMenuButton)
            mMainSceneGroupLoadConfig.Load();
        else if (button == m_RestartButton)
            mGameSceneGroupLoadConfig.Load();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventBus<GameOverEvent>.Deregister(_gameOverEventBinding);
    }
}
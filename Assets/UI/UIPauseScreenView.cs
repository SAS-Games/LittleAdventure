using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIPauseScreenView : UIScreenView
{
    [SerializeField] private UIButton m_MainMenuButton;
    [SerializeField] private UIButton m_RestartButton;
    [FormerlySerializedAs("m_MainSceneGroupLoader")] [SerializeField] private SceneGroupLoadConfig mMainSceneGroupLoadConfig;
    [FormerlySerializedAs("m_GameSceneGroupLoader")] [SerializeField] private SceneGroupLoadConfig mGameSceneGroupLoadConfig;
    private EventBinding<GamePauseEvent> _pauseEventBinding;

    protected override void Awake()
    {
        base.Awake();
        gameObject.SetActive(false);
        _pauseEventBinding = new EventBinding<GamePauseEvent>(val => gameObject.SetActive(val.state));
        EventBus<GamePauseEvent>.Register(_pauseEventBinding);
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
        EventBus<GamePauseEvent>.Deregister(_pauseEventBinding);
    }
}
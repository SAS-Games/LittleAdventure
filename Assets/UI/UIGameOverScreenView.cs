using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIGameOverScreenView : UIScreenView
{
    [SerializeField] private UIButton m_MainMenuButton;
    [SerializeField] private UIButton m_RestartButton;
    [SerializeField] private SceneGroupLoader m_MainSceneGroupLoader;
    [SerializeField] private SceneGroupLoader m_GameSceneGroupLoader;
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
            m_MainSceneGroupLoader.Load();
        else if (button == m_RestartButton)
            m_GameSceneGroupLoader.Load();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventBus<GameOverEvent>.Deregister(_gameOverEventBinding);
    }
}
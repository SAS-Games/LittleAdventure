using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIGameOverScreenView : UIScreenView
{
    [SerializeField] private Button m_MainMenuButton;
    [SerializeField] private Button m_RestartButton;
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

    protected override void OnButtonClick(GameObject button, PointerEventData eventData)
    {
        if (button == m_MainMenuButton.gameObject)
            m_MainSceneGroupLoader.Load();
        else if (button == m_RestartButton.gameObject)
            m_GameSceneGroupLoader.Load();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventBus<GameOverEvent>.Deregister(_gameOverEventBinding);
    }
}
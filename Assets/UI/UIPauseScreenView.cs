using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIPauseScreenView : UIScreenView
{
    [SerializeField] private Button m_MainMenuButton;
    [SerializeField] private Button m_RestartButton;
    [SerializeField] private SceneGroupLoader m_MainSceneGroupLoader;
    [SerializeField] private SceneGroupLoader m_GameSceneGroupLoader;
    private EventBinding<GamePauseEvent> _pauseEventBinding;

    protected override void Awake()
    {
        base.Awake();
        gameObject.SetActive(false);
        _pauseEventBinding = new EventBinding<GamePauseEvent>(val => gameObject.SetActive(val.state));
        EventBus<GamePauseEvent>.Register(_pauseEventBinding);
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
        EventBus<GamePauseEvent>.Deregister(_pauseEventBinding);
    }
}
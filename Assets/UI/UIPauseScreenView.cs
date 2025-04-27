using UnityEngine.EventSystems;

public class UIPauseScreenView : UIBehaviour
{
    private EventBinding<GamePauseEvent> _pauseEventBinding;

    protected override void Awake()
    {
        base.Awake();
        gameObject.SetActive(false);
        _pauseEventBinding = new EventBinding<GamePauseEvent>(val => gameObject.SetActive(val.state));
        EventBus<GamePauseEvent>.Register(_pauseEventBinding);
    }

    private void OnDestroy()
    {
        EventBus<GamePauseEvent>.Deregister(_pauseEventBinding);
    }
}
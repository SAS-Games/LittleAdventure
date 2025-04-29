using SAS.StateMachineGraph;
using UnityEngine;

public class OnGamePauseHandler : MonoBehaviour
{
    private EventBinding<GamePauseEvent> _pauseEventBinding;

    protected void Awake()
    {
        _pauseEventBinding = new EventBinding<GamePauseEvent>(gamePause => OnGamePause(gamePause.state));
        EventBus<GamePauseEvent>.Register(_pauseEventBinding);
    }

    private void OnGamePause(bool status)
    {
        GetComponentInParent<Actor>().enabled = !status;
    }

    private void OnDestroy()
    {
        EventBus<GamePauseEvent>.Deregister(_pauseEventBinding);
    }
}

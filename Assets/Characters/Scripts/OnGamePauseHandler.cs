using System.Collections.Generic;
using SAS.StateMachineGraph.Utilities;
using SAS.Utilities.TagSystem;
using UnityEngine;

public class OnGamePauseHandler : MonoBehaviour
{
    [FieldRequiresChild] private IActivatable[] _activatables;
    [FieldRequiresChild] private Animator[] _animators;
    private EventBinding<GamePauseEvent> _pauseEventBinding;
    private Dictionary<Animator, float> _animatorSpeeds = new();

    protected void Awake()
    {
        this.Initialize();
        _pauseEventBinding = new EventBinding<GamePauseEvent>(gamePause => OnGamePause(gamePause.state));
        EventBus<GamePauseEvent>.Register(_pauseEventBinding);

        foreach (var animator in _animators)
        {
            if (!_animatorSpeeds.ContainsKey(animator))
                _animatorSpeeds[animator] = animator.speed;
        }
    }

    private void OnGamePause(bool status)
    {
        foreach (var activatable in _activatables)
        {
            if (!status)
                activatable.Activate();
            else
                activatable.Deactivate();
        }

        foreach (var animator in _animators)
        {
            if (status)
            {
                if (!_animatorSpeeds.ContainsKey(animator))
                    _animatorSpeeds[animator] = animator.speed;

                animator.speed = 0f;
            }
            else
            {
                if (_animatorSpeeds.TryGetValue(animator, out var originalSpeed))
                    animator.speed = originalSpeed;
            }
        }
    }

    private void OnDestroy()
    {
        EventBus<GamePauseEvent>.Deregister(_pauseEventBinding);
    }
}
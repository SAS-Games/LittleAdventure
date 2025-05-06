using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Interactions;

[InputControlLayout]
public class LoggingHoldInteraction : IInputInteraction
{
    public float duration = 1f;
    private float timer;

    public void Process(ref InputInteractionContext context)
    {
        if (context.timerHasExpired)
        {
            Debug.Log($"[Interaction] Hold completed at time: {Time.time}");
            context.PerformedAndStayPerformed();
        }
        else if (context.phase == InputActionPhase.Waiting)
        {
            context.Started();
            context.SetTimeout(duration);
            Debug.Log($"[Interaction] Hold started at time: {Time.time}");
        }
    }

    public void Reset()
    {
        Debug.Log("[Interaction] Reset called");
    }
}

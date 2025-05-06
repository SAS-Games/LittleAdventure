using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Processors;
using UnityEngine;
using UnityEngine.Scripting;

[InputControlLayout]
[Preserve]
public class LoggingProcessor : InputProcessor<Vector2>
{
    static LoggingProcessor()
    {
        // This registers the processor globally by its type name ("loggingprocessor")
        InputSystem.RegisterProcessor<LoggingProcessor>();
    }

    public override Vector2 Process(Vector2 value, InputControl control)
    {
        Debug.Log($"[Processor] Called. Raw input: {value}");
        return value * 0.5f; // Just modify it to show effect
    }
}

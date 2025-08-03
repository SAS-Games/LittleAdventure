using Ink.Runtime;
using UnityEngine.InputSystem;

public static class InkInputLabelSetter
{
    public static void SetControlLabelsFromPlayerInput(PlayerInput playerInput, Story story)
    {
        if (playerInput == null || story == null)
            return;

        var deviceType = InputDeviceUtils.GetActiveDevice(playerInput);

        switch (deviceType)
        {
            case InputDeviceUtils.InputDeviceType.DualShockGamepad:
                ApplyDualShockBindings(story);
                break;

            case InputDeviceUtils.InputDeviceType.XboxGamepad:
                ApplyXboxBindings(story);
                break;

            case InputDeviceUtils.InputDeviceType.KeyboardMouse:
                ApplyKeyboardBindings(story);
                break;

            case InputDeviceUtils.InputDeviceType.OtherGamepad:
                ApplyGenericGamepadBindings(story);
                break;

            default:
                ApplyKeyboardBindings(story);
                break;
        }
    }

    private static void ApplyDualShockBindings(Story story)
    {
        story.variablesState["dashButton"] = "R1";
        story.variablesState["attackButton"] = "⨉";
    }

    private static void ApplyXboxBindings(Story story)
    {
        story.variablesState["dashButton"] = "RB";
        story.variablesState["attackButton"] = "A";
    }

    private static void ApplyGenericGamepadBindings(Story story)
    {
        story.variablesState["dashButton"] = "RB";
        story.variablesState["attackButton"] = "South Button";
    }

    private static void ApplyKeyboardBindings(Story story)
    {
        story.variablesState["dashButton"] = "Shift";
        story.variablesState["attackButton"] = "Left Click";
    }
}
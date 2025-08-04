using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.InputSystem.DualShock;

public static class InputDeviceUtils
{
    public enum InputDeviceType
    {
        KeyboardMouse,
        XboxGamepad,
        DualShockGamepad,
        OtherGamepad,
        Unknown
    }

    public static InputDeviceType GetActiveDevice(PlayerInput playerInput)
    {
        if (playerInput == null)
            return InputDeviceType.Unknown;

        if (playerInput.GetDevice<Gamepad>() is Gamepad gamepad)
        {
            var desc = gamepad.description;
            var product = desc.product?.ToLower() ?? "";
            var manufacturer = desc.manufacturer?.ToLower() ?? "";

            if (manufacturer.Contains("sony") || product.Contains("dualshock") || product.Contains("ps5") ||
                product.Contains("ps4"))
                return InputDeviceType.DualShockGamepad;

            if (manufacturer.Contains("microsoft") || product.Contains("xbox") || product.Contains("xinput"))
                return InputDeviceType.XboxGamepad;

            return InputDeviceType.OtherGamepad;
        }
        else if (playerInput.GetDevice<Keyboard>() is  Keyboard)
                return InputDeviceType.KeyboardMouse;
        else if (playerInput.GetDevice<Mouse>() is Mouse )
                return InputDeviceType.KeyboardMouse;

        return InputDeviceType.Unknown;
    }
}
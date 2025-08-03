using UnityEngine.InputSystem;
using System.Linq;

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

        var device = playerInput.user.pairedDevices.FirstOrDefault();
        if (device == null)
            return InputDeviceType.Unknown;

        if (device is Keyboard || device is Mouse)
            return InputDeviceType.KeyboardMouse;

        if (device is Gamepad gamepad)
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

        return InputDeviceType.Unknown;
    }
}
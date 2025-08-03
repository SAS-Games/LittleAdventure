using System;
using UnityEngine;
using UnityEngine.InputSystem;

struct ControlSchemeChangedEvent : IEvent
{
    public int PlayerIndex { get; }
    public InputDevice InputDevice { get; }

    public ControlSchemeChangedEvent(int playerIndex, InputDevice inputDevice)
    {
        PlayerIndex = playerIndex;
        InputDevice = inputDevice;
    }
}

[RequireComponent(typeof(PlayerInput))]
public class ControlSchemeNotifier : MonoBehaviour
{
    private EventBinding<ControlSchemeChangedEvent> _controlSchemeChangedEventBinding;

    public event Action<InputDevice> OnControlSchemeChanged;

    private InputDevice _currentDevice;
    private PlayerInput _playerInput;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _currentDevice = _playerInput.devices.Count > 0 ? _playerInput.devices[0] : null;
    }

    private void OnEnable()
    {
        _playerInput.onControlsChanged += HandleControlsChanged;
    }

    private void OnDisable()
    {
        _playerInput.onControlsChanged -= HandleControlsChanged;
    }

    private void HandleControlsChanged(PlayerInput playerInput)
    {
        var newDevice = _playerInput.devices.Count > 0 ? _playerInput.devices[0] : null;
        if (newDevice != _currentDevice)
        {
            _currentDevice = newDevice;
            OnControlSchemeChanged?.Invoke(_currentDevice);
            EventBus<ControlSchemeChangedEvent>.Raise(new ControlSchemeChangedEvent(_playerInput.playerIndex, newDevice));
        }
    }
}
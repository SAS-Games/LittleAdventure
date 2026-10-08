using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Lightweight test camera. With no target it behaves as a free-fly camera;
/// assigning a target enables the original orbit camera controls.
/// </summary>
public sealed class CameraController : MonoBehaviour
{
    public Transform target;

    [Header("Orbit")]
    [SerializeField] private Vector3 m_targetOffset;
    [SerializeField] private float m_Distance = 5f;
    [SerializeField] private float m_MaxDistance = 100f;
    [SerializeField] private float m_MinDistance = 0.6f;
    [SerializeField] private float m_XSpeed = 200f;
    [SerializeField] private float m_YSpeed = 200f;
    [SerializeField] private int m_YMinLimit = -80;
    [SerializeField] private int m_YMaxLimit = 80;
    [SerializeField] private int m_ZoomRate = 40;
    [SerializeField] private float m_PanSpeed = 0.3f;
    [SerializeField] private float m_ZoomDampening = 5f;

    [Header("Free Fly")]
    [SerializeField] private float m_FlySpeed = 10f;
    [SerializeField] private float m_FastFlyMultiplier = 4f;
    [SerializeField] private float m_LookSensitivity = 0.15f;
    [SerializeField] private bool m_RequireRightMouseForMovement;
    [SerializeField] private bool m_LockCursorWhileLooking = true;
    [SerializeField, Range(1f, 179f)] private float m_MinFieldOfView = 30f;
    [SerializeField, Range(1f, 179f)] private float m_MaxFieldOfView = 90f;
    [SerializeField, Min(0f)] private float m_FieldOfViewStep = 4f;

    private float _xDegrees;
    private float _yDegrees;
    private float _currentDistance;
    private float _desiredDistance;
    private Quaternion _rotation;
    private bool _ownsCursorLock;
    private Camera _camera;

    private void Start()
    {
        _camera = GetComponent<Camera>();
        _rotation = transform.rotation;
        _xDegrees = transform.eulerAngles.y;
        _yDegrees = NormalizePitch(transform.eulerAngles.x);

        if (target == null)
            return;

        m_Distance = Vector3.Distance(transform.position, target.position);
        _currentDistance = m_Distance;
        _desiredDistance = m_Distance;
    }

    private void OnDisable()
    {
        ReleaseCursor();
    }

    private void LateUpdate()
    {
        Mouse mouse = Mouse.current;
        Keyboard keyboard = Keyboard.current;
        if (mouse == null || keyboard == null || IsTextInputFocused())
        {
            ReleaseCursor();
            return;
        }

        if (target == null)
            UpdateFreeFly(mouse, keyboard);
        else
            UpdateOrbit(mouse, keyboard);
    }

    private void UpdateFreeFly(Mouse mouse, Keyboard keyboard)
    {
        bool looking = mouse.rightButton.isPressed;
        UpdateCursor(looking);

        if (looking)
        {
            Vector2 delta = mouse.delta.ReadValue();
            _xDegrees += delta.x * m_LookSensitivity;
            _yDegrees -= delta.y * m_LookSensitivity;
            _yDegrees = ClampAngle(_yDegrees, m_YMinLimit, m_YMaxLimit);
            transform.rotation = Quaternion.Euler(_yDegrees, _xDegrees, 0f);
        }

        if (looking || !m_RequireRightMouseForMovement)
        {
            Vector3 move = Vector3.zero;
            if (keyboard.wKey.isPressed) move += transform.forward;
            if (keyboard.sKey.isPressed) move -= transform.forward;
            if (keyboard.aKey.isPressed) move -= transform.right;
            if (keyboard.dKey.isPressed) move += transform.right;
            if (keyboard.eKey.isPressed) move += Vector3.up;
            if (keyboard.cKey.isPressed || keyboard.qKey.isPressed) move -= Vector3.up;

            float multiplier = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed
                ? m_FastFlyMultiplier
                : 1f;
            if (move.sqrMagnitude > 0f)
                transform.position += move.normalized * (m_FlySpeed * multiplier * Time.unscaledDeltaTime);
        }

        float scroll = mouse.scroll.ReadValue().y;
        if (_camera != null && Mathf.Abs(scroll) > 0.01f)
        {
            float minFov = Mathf.Min(m_MinFieldOfView, m_MaxFieldOfView);
            float maxFov = Mathf.Max(m_MinFieldOfView, m_MaxFieldOfView);
            float wheelSteps = scroll / 120f;
            _camera.fieldOfView = Mathf.Clamp(
                _camera.fieldOfView - wheelSteps * m_FieldOfViewStep,
                minFov,
                maxFov);
        }
    }

    private void UpdateOrbit(Mouse mouse, Keyboard keyboard)
    {
        Vector2 mouseDelta = mouse.delta.ReadValue();
        float scroll = mouse.scroll.ReadValue().y;
        bool leftMouse = mouse.leftButton.isPressed;
        bool alt = keyboard.leftAltKey.isPressed || keyboard.rightAltKey.isPressed;
        bool ctrl = keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed;
        bool pan = keyboard.qKey.isPressed;
        float deltaTime = Time.unscaledDeltaTime;

        if (leftMouse && alt && ctrl)
        {
            _desiredDistance -= mouseDelta.y * deltaTime * m_ZoomRate * 0.125f * Mathf.Abs(_desiredDistance);
        }
        else if (leftMouse && alt)
        {
            _xDegrees += mouseDelta.x * m_XSpeed * 0.02f;
            _yDegrees -= mouseDelta.y * m_YSpeed * 0.02f;
            _yDegrees = ClampAngle(_yDegrees, m_YMinLimit, m_YMaxLimit);
            _rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.Euler(_yDegrees, _xDegrees, 0f),
                deltaTime * m_ZoomDampening);
            transform.rotation = _rotation;
        }
        else if (leftMouse && pan)
        {
            target.rotation = transform.rotation;
            target.Translate(Vector3.right * -mouseDelta.x * m_PanSpeed);
            target.Translate(transform.up * -mouseDelta.y * m_PanSpeed, Space.World);
        }

        if (Mathf.Abs(scroll) > 0.01f)
            _desiredDistance -= scroll / 120f * m_ZoomRate * 0.1f;

        _desiredDistance = Mathf.Clamp(_desiredDistance, m_MinDistance, m_MaxDistance);
        _currentDistance = Mathf.Lerp(_currentDistance, _desiredDistance, deltaTime * m_ZoomDampening);
        transform.position = target.position - (_rotation * Vector3.forward * _currentDistance + m_targetOffset);
    }

    private void UpdateCursor(bool looking)
    {
        if (!m_LockCursorWhileLooking)
            return;

        if (looking && !_ownsCursorLock)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _ownsCursorLock = true;
        }
        else if (!looking)
        {
            ReleaseCursor();
        }
    }

    private void ReleaseCursor()
    {
        if (!_ownsCursorLock)
            return;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _ownsCursorLock = false;
    }

    private static bool IsTextInputFocused()
    {
        GameObject selected = EventSystem.current?.currentSelectedGameObject;
        TMP_InputField input = selected != null ? selected.GetComponentInParent<TMP_InputField>() : null;
        return input != null && input.isFocused;
    }

    private static float NormalizePitch(float angle)
    {
        return angle > 180f ? angle - 360f : angle;
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        while (angle < -360f) angle += 360f;
        while (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
}

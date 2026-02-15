using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public Transform target;

    [SerializeField] private Vector3 m_targetOffset;
    [SerializeField] private float m_Distance = 5.0f;
    [SerializeField] private float m_MaxDistance = 100;
    [SerializeField] private float m_MinDistance = .6f;
    [SerializeField] private float m_XSpeed = 200.0f;
    [SerializeField] private float m_YSpeed = 200.0f;
    [SerializeField] private int m_YMinLimit = -80;
    [SerializeField] private int m_YMaxLimit = 80;
    [SerializeField] private int m_ZoomRate = 40;
    [SerializeField] private float m_PanSpeed = 0.3f;
    [SerializeField] private float m_ZoomDampening = 5.0f;

    // Fly mode
    [SerializeField] private float m_FlySpeed = 10f;
    [SerializeField] private float m_FastFlyMultiplier = 4f;
    [SerializeField] private float m_LookSensitivity = 0.15f;

    private float xDeg = 0.0f;
    private float yDeg = 0.0f;

    private float currentDistance;
    private float desiredDistance;

    private Quaternion currentRotation;
    private Quaternion desiredRotation;
    private Quaternion rotation;

    private Vector3 position;

    void Start() => Init();

    public void Init()
    {
        GameObject go = new GameObject("Fake Cam Target");
        go.transform.position = transform.position + (transform.forward * m_Distance);
        target = go.transform;

        m_Distance = Vector3.Distance(transform.position, target.position);
        currentDistance = m_Distance;
        desiredDistance = m_Distance;

        rotation = transform.rotation;
        currentRotation = rotation;
        desiredRotation = rotation;

        xDeg = transform.eulerAngles.y;
        yDeg = transform.eulerAngles.x;
    }

    void LateUpdate()
    {
        if (Mouse.current == null || Keyboard.current == null)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float scroll = Mouse.current.scroll.ReadValue().y;

        bool leftMouse = Mouse.current.leftButton.isPressed;
        bool rightMouse = Mouse.current.rightButton.isPressed;

        bool alt = Keyboard.current.leftAltKey.isPressed;
        bool ctrl = Keyboard.current.leftCtrlKey.isPressed;
        bool qKey = Keyboard.current.qKey.isPressed;

        // =========================================================
        // FLY MODE (RMB + WASD)
        // =========================================================
        if (rightMouse)
        {
            // Look
            xDeg += mouseDelta.x * m_XSpeed * m_LookSensitivity;
            yDeg -= mouseDelta.y * m_YSpeed * m_LookSensitivity;
            yDeg = ClampAngle(yDeg, m_YMinLimit, m_YMaxLimit);

            rotation = Quaternion.Euler(yDeg, xDeg, 0);
            transform.rotation = rotation;

            // Movement
            Vector3 move = Vector3.zero;

            if (Keyboard.current.wKey.isPressed) move += transform.forward;
            if (Keyboard.current.sKey.isPressed) move -= transform.forward;
            if (Keyboard.current.aKey.isPressed) move -= transform.right;
            if (Keyboard.current.dKey.isPressed) move += transform.right;
            if (Keyboard.current.eKey.isPressed) move += Vector3.up;
            if (Keyboard.current.cKey.isPressed) move -= Vector3.up;

            float speed = m_FlySpeed * (Keyboard.current.leftShiftKey.isPressed ? m_FastFlyMultiplier : 1f);

            if (move.sqrMagnitude > 0f) target.position += move.normalized * speed * Time.deltaTime;

            desiredRotation = rotation;

            UpdateCameraPosition();
            return; // IMPORTANT: skip orbit/zoom logic
        }

        // =========================================================
        // DRAG ZOOM (Alt + Ctrl + LMB)
        // =========================================================
        if (leftMouse && alt && ctrl)
        {
            desiredDistance -= mouseDelta.y * Time.deltaTime * m_ZoomRate * 0.125f * Mathf.Abs(desiredDistance);
        }
        // =========================================================
        // ORBIT (Alt + LMB)
        // =========================================================
        else if (leftMouse && alt)
        {
            xDeg += mouseDelta.x * m_XSpeed * 0.02f;
            yDeg -= mouseDelta.y * m_YSpeed * 0.02f;

            yDeg = ClampAngle(yDeg, m_YMinLimit, m_YMaxLimit);

            desiredRotation = Quaternion.Euler(yDeg, xDeg, 0);
            currentRotation = transform.rotation;

            rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime * m_ZoomDampening);

            transform.rotation = rotation;
        }
        // =========================================================
        // PAN (Q + LMB)
        // =========================================================
        else if (leftMouse && qKey)
        {
            target.rotation = transform.rotation;

            target.Translate(Vector3.right * -mouseDelta.x * m_PanSpeed);
            target.Translate(transform.up * -mouseDelta.y * m_PanSpeed, Space.World);
        }

        // =========================================================
        // SCROLL ZOOM
        // =========================================================
        desiredDistance -= scroll * Time.deltaTime * m_ZoomRate * Mathf.Abs(desiredDistance);

        desiredDistance = Mathf.Clamp(desiredDistance, m_MinDistance, m_MaxDistance);

        currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * m_ZoomDampening);

        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        position = target.position - (rotation * Vector3.forward * currentDistance + m_targetOffset);

        transform.position = position;
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360) angle += 360;
        if (angle > 360) angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}
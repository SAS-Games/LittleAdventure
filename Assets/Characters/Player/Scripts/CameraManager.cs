using System.Collections;
using SAS.StateMachineCharacterController;
using Unity.Cinemachine;
using UnityEngine;
using Debug = SAS.Debug;

public class CameraManager : MonoBehaviour, IObjectSpawnedListener
{
    [SerializeField] private Camera m_MainCamera;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public Transform CinemachineCameraTarget;

    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;

    // cinemachine
    private CameraLookControls _cameraLookControls;
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private bool _isRMBPressed;
    private const float _threshold = 0.01f;

    private bool _cameraMovementLock = false;
    private CinemachineBrain cinemachineBrain;


    private void Awake()
    {
        _cameraLookControls = new CameraLookControls();
        cinemachineBrain = GetComponent<CinemachineBrain>();
    }

    private void OnEnable()
    {
        _cameraLookControls.Mouse.Enable();
        _cameraLookControls.Mouse.MouseControlCamera.performed += _ => OnEnableMouseControlCamera();
        _cameraLookControls.Mouse.MouseControlCamera.canceled += _ => OnDisableMouseControlCamera();
        StartCoroutine(CheckForCameraChanges());
    }

    private void OnDisable()
    {
        _cameraLookControls.Mouse.MouseControlCamera.performed -= _ => OnEnableMouseControlCamera();
        _cameraLookControls.Mouse.MouseControlCamera.canceled -= _ => OnDisableMouseControlCamera();
    }

    IEnumerator CheckForCameraChanges()
    {
        ICinemachineCamera activeCamera = cinemachineBrain.ActiveVirtualCamera;

        while (activeCamera == null)
        {
            yield return null; // Check every 0.1 seconds
            activeCamera = cinemachineBrain.ActiveVirtualCamera;
        }

        var cinemachineCamera = activeCamera as CinemachineCamera;
        ;
        while (cinemachineCamera.Target.TrackingTarget == null)
        {
            yield return null;
        }

        CinemachineCameraTarget = cinemachineCamera.Target.TrackingTarget;
        _cinemachineTargetYaw = CinemachineCameraTarget.rotation.eulerAngles.y;
    }

    private void OnEnableMouseControlCamera()
    {
        _isRMBPressed = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        StartCoroutine(DisableMouseControlForFrame());
    }

    IEnumerator DisableMouseControlForFrame()
    {
        _cameraMovementLock = true;
        yield return new WaitForEndOfFrame();
        _cameraMovementLock = false;
    }

    private void OnDisableMouseControlCamera()
    {
        _isRMBPressed = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnCameraMove(Vector2 cameraMovement, bool isDeviceMouse)
    {
        if (_cameraMovementLock)
            return;

        if (isDeviceMouse && !_isRMBPressed)
            return;

        // if there is an input and camera position is not fixed
        if (cameraMovement.sqrMagnitude >= _threshold)
        {
            _cinemachineTargetYaw += cameraMovement.x;
            _cinemachineTargetPitch += cameraMovement.y;
        }
    }

    private void LateUpdate()
    {
        var cameraMovement = _cameraLookControls.Mouse.RotateCamera.ReadValue<Vector2>();
        var isDeviceMouse = _cameraLookControls.Mouse.RotateCamera.activeControl?.device.name == "Mouse";
        OnCameraMove(cameraMovement, isDeviceMouse);
        if (CinemachineCameraTarget == null)
            return;
        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);
        CinemachineCameraTarget.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }


    void IObjectSpawnedListener.OnSpawn(GameObject gameObject)
    {
        //todo: this must be removed from here.
        var cameraLookAt = gameObject.GetComponent<ICameraLookAt>();
        CinemachineCameraTarget = cameraLookAt.Target;
    }

    void IObjectSpawnedListener.OnDespawn(GameObject gameObject)
    {
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}
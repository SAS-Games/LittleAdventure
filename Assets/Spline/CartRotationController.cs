using SAS.Utilities.TagSystem;
using UnityEngine;

public class CartRotationController : MonoBehaviour
{
    [SerializeField] private Transform m_Target;
    [SerializeField] private float maxTiltAngle = 25f;
    [SerializeField] private float rotationSpeed = 5f;
    [Inject] private IPlayerSetupModel _playerSetup;

    private void Awake()
    {
        this.Initialize();
    }

    private void Update()
    {
        float totalTilt = 0f;
        int tiltCount = 0;

        foreach (var player in _playerSetup.Players)
        {
            if (player.Input == null)
                continue;

            // Simulate tilt from left stick's X axis (replace if you support gyro later)
            Vector2 stick = player.Input.actions["Movement"].ReadValue<Vector2>();
            totalTilt += stick.x;
            tiltCount++;
        }

        if (tiltCount == 0)
            return;

        float averageTilt = totalTilt / tiltCount;

        // Convert average tilt to a target rotation angle
        float targetZ = -averageTilt * maxTiltAngle; // Negative to tilt correctly

        Quaternion targetRotation = Quaternion.Euler(0, 0, targetZ);
        m_Target.localRotation = Quaternion.Slerp(m_Target.localRotation, targetRotation, Time.deltaTime * rotationSpeed * tiltCount);
    }
}

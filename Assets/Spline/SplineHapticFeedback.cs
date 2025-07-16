using UnityEngine;
using UnityEngine.InputSystem;

public interface ISplineFeedback
{
    void EvaluateFeedback(Vector3 position, Vector3 direction);
}


[RequireComponent(typeof(SplineFollow))]
public class SplineHapticFeedback : MonoBehaviour, ISplineFeedback
{
    [SerializeField, Range(0f, 10f)] private float m_BumpinessSensitivity = 10f;
    [SerializeField, Range(0f, 90f)] private float m_SteepnessSensitivity = 45f;
    [SerializeField, Range(0f, 1f)] private float m_BumpinessJerkThreshold = 0.01f;
    [SerializeField, Range(0f, 1f)] private float m_BumpinessSmoothing = 0.2f;
    [SerializeField, Range(0f, 1f)] private float m_LeftMotorWeight = 1f;
    [SerializeField, Range(0f, 1f)] private float m_RightMotorWeight = 1f;

    private SplineFollow _splineFollow;
    private float _prevDeltaY;
    private float _prevY;
    private float _smoothedBumpiness;
    private Vector3 _prevTangent;

    // Add this helper method
    private float CalculateSlopeAngle(Vector3 direction)
    {
        // Remove vertical component to get horizontal direction
        Vector3 horizontal = new Vector3(direction.x, 0, direction.z);

        // Avoid division by zero
        if (horizontal.magnitude < 0.001f)
            return direction.y >= 0 ? 90f : -90f;

        // Calculate signed angle using dot product with up vector
        float angle = Mathf.Atan2(direction.y, horizontal.magnitude) * Mathf.Rad2Deg;
        return angle; // Can be positive (uphill) or negative (downhill)
    }

    private float CalculateVerticalBumpiness(float currentY)
    {
        float deltaY = currentY - _prevY;
        float deltaDeltaY = deltaY - _prevDeltaY;

        _prevDeltaY = deltaY;
        _prevY = currentY;

        // Only count if sudden enough
        float jerk = Mathf.Abs(deltaDeltaY);
        float rawBumpiness = jerk > m_BumpinessJerkThreshold ? jerk * m_BumpinessSensitivity : 0f;

        // Optional smoothing to reduce jitter
        _smoothedBumpiness = Mathf.Lerp(_smoothedBumpiness, rawBumpiness, m_BumpinessSmoothing);

        return Mathf.Clamp01(_smoothedBumpiness);
    }


    // Updated EvaluateFeedback()
    public void EvaluateFeedback(Vector3 position, Vector3 direction)
    {
        float bumpiness = CalculateVerticalBumpiness(position.y);

        // 2. STEEPNESS (High Frequency Motor)
        float slopeAngle = CalculateSlopeAngle(direction);
        float steepness = Mathf.Clamp01(slopeAngle / m_SteepnessSensitivity);

        _prevY = position.y;

        // 3. Apply motor weights
        float low = bumpiness * m_LeftMotorWeight;
        float high = steepness * m_RightMotorWeight;

       // SAS.Debug.Log($"Motor speed low frequency: {low} & high frequency: {high}", "SplineHapticFeedback");


        if (Gamepad.current != null)
            Gamepad.current.SetMotorSpeeds(0, high);
    }

    private void OnDisable()
    {
        if (Gamepad.current != null)
            Gamepad.current.SetMotorSpeeds(0f, 0f);
    }
}

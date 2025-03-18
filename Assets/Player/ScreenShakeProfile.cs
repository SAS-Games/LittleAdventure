using UnityEngine;

[CreateAssetMenu(menuName = "SAS/CameraShakeProfile")]
public class ScreenShakeProfile : ScriptableObject
{
    [Header("Impulse Source Settings")]
    [field: SerializeField] public float ImpulseDuration { get; private set; } = 0.2f;
    [field: SerializeField] public float ImpactForce { get; private set; } = 1f;
    [field: SerializeField] public Vector3 DefaultVelocity { get; private set; } = Vector3.down;
    [field: SerializeField] public AnimationCurve ImpulseCurve { get; private set; }

    [Header("Impulse Listener Settings")]
    [field: SerializeField] public float ListenerAmplitude { get; private set; }
    [field: SerializeField] public float ListenerFrequncy { get; private set; }
    [field: SerializeField] public float ListenerDuration { get; private set; }

}

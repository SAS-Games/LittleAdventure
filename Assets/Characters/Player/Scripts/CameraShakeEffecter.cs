using SAS.Utilities.TagSystem;
using Unity.Cinemachine;
using UnityEngine;

public class CameraShakeEffecter : MonoBehaviour
{
    [SerializeField] private string m_EventName;
    [SerializeField] private ScreenShakeProfile m_Profile;
    [FieldRequiresSelf] private IEventDispatcher _animationEventDispatcher;
    [FieldRequiresChild] private CinemachineImpulseSource _cinemachineImpulseSource;
    private CinemachineImpulseListener _cinemachineImpulseListener;

    private void Awake()
    {
        this.Initialize();
        _animationEventDispatcher.Subscribe(m_EventName, GenerateImpulse);
    }

    private void GenerateImpulse()
    {
        Setup();
        _cinemachineImpulseSource.GenerateImpulseWithForce(m_Profile.ImpactForce);
    }

    private void Setup()
    {
        var impulseDefinition = _cinemachineImpulseSource.ImpulseDefinition;
        impulseDefinition.ImpulseDuration = m_Profile.ImpulseDuration;
        impulseDefinition.CustomImpulseShape = m_Profile.ImpulseCurve;
        _cinemachineImpulseSource.DefaultVelocity = m_Profile.DefaultVelocity;

        if (_cinemachineImpulseListener)
        {
            _cinemachineImpulseListener.ReactionSettings.Duration = m_Profile.ListenerDuration;
            _cinemachineImpulseListener.ReactionSettings.AmplitudeGain = m_Profile.ListenerAmplitude;
            _cinemachineImpulseListener.ReactionSettings.FrequencyGain = m_Profile.ListenerFrequncy;
        }

    }
}

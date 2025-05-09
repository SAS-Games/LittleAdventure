using System;
using FMOD.Studio;
using FMODUnity;
using SAS.Utilities.TagSystem;
using UnityEngine;

public interface IBGMPlayer : IBindable
{
    void Progress(float progress);
}

public class LevelBGMPlayer : MonoBehaviour, IBGMPlayer
{
    [SerializeField] private EventReference m_EventReference;
    [SerializeField] private String m_ProgessParamKey = "Progress";
    [SerializeField] private String m_ThreatLevelParamKey = "Threat Level";
    private EventInstance m_EventInstance;
    private EventBinding<GamePauseEvent> _pauseEventBinding;
    private EventBinding<GlobalThreatLevelEvent> _globalThreatLevelEvent;

    private void Start()
    {
        _pauseEventBinding = new EventBinding<GamePauseEvent>(gamePause => OnGamePause(gamePause.state));
        _globalThreatLevelEvent =
            new EventBinding<GlobalThreatLevelEvent>(threatLevel =>
                OnThreatLevelUpdate(threatLevel.averageThreatLevel));
        EventBus<GamePauseEvent>.Register(_pauseEventBinding);
        EventBus<GlobalThreatLevelEvent>.Register(_globalThreatLevelEvent);
    }

    public void OnInstanceCreated()
    {
        m_EventInstance = RuntimeManager.CreateInstance(m_EventReference);
        m_EventInstance.start();
        m_EventInstance.set3DAttributes(gameObject.To3DAttributes());
        m_EventInstance.release();
    }

    public void Progress(float progress)
    {
        m_EventInstance.setParameterByName(m_ProgessParamKey, progress);
    }

    private void OnGamePause(bool status)
    {
    }

    private void OnThreatLevelUpdate(float threatLevel)
    {
        m_EventInstance.setParameterByName(m_ThreatLevelParamKey, threatLevel);
    }

    private void OnDestroy()
    {
        m_EventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        EventBus<GamePauseEvent>.Deregister(_pauseEventBinding);
        EventBus<GlobalThreatLevelEvent>.Deregister(_globalThreatLevelEvent);
    }
}
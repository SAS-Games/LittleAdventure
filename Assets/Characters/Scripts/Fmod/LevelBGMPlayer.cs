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
    private EventInstance m_EventInstance;

    public void OnInstanceCreated()
    {
        m_EventInstance = RuntimeManager.CreateInstance(m_EventReference);
        m_EventInstance.start();
        m_EventInstance.set3DAttributes(gameObject.To3DAttributes());
        m_EventInstance.release();
    }

    public void Progress(float progress)
    {
        m_EventInstance.setParameterByName("Progress", progress);
    }

    private void OnDestroy()
    {
        m_EventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
}
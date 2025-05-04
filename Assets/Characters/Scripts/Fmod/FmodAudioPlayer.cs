using FMODUnity;
using UnityEngine;

public class FmodAudioPlayer : MonoBehaviour
{
    public void PlayOneShot(EventReference eventRef)
    {
        RuntimeManager.PlayOneShot(eventRef);
    }

    public void PlayOneShot(string eventPath)
    {
        RuntimeManager.PlayOneShot(eventPath, transform.position);
    }
}
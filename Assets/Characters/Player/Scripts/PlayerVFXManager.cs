using SAS.Utilities.TagSystem;
using System;
using UnityEngine;
using UnityEngine.VFX;

public class PlayerVFXManager : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] m_BladeVFXs;
    [SerializeField] private VisualEffect m_FootStep;
    [SerializeField] private VisualEffect m_Slash;
    [SerializeField] private VisualEffect m_Heal;

    public void UpdateFootStep(bool state)
    {
        if (state)
            m_FootStep.Play();
        else
            m_FootStep.Stop();
    }

    public void PlayBlade(int index)
    {
        if (index > 0)
            m_BladeVFXs[index - 1].Stop();
        m_BladeVFXs[index].Play();
    }

    public void PlayHeal()
    {
        m_Heal.SendEvent("OnPlay");
    }

    public void PlaySlash(Vector3 position)
    {
        m_Slash.transform.position = position;
        m_Slash.Play();
    }
}
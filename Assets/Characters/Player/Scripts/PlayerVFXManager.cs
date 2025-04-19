using SAS.Utilities.TagSystem;
using System;
using UnityEngine;
using UnityEngine.VFX;
public class PlayerVFXManager : MonoBehaviour
{
    [SerializeField] private VisualEffect m_FootStep;
    [SerializeField] private ParticleSystem[] m_BladeVFXs;
    [SerializeField] private VisualEffect m_Slash;
    [SerializeField] private VisualEffect m_Heal;

    [FieldRequiresSelf] private IEventDispatcher _animationEventDispatcher;

    private void Awake()
    {
        this.Initialize();
        _animationEventDispatcher.Subscribe<bool>("UpdateFootStep", UpdateFootStep);
        _animationEventDispatcher.Subscribe<int>("PlayBlade", PlayBlade);
        _animationEventDispatcher.Subscribe<Vector3>("PlaySlash", PlaySlash);
        _animationEventDispatcher.Subscribe("Heal", PlayHeal);
    }

    private void UpdateFootStep(bool state)
    {
        if (state)
            m_FootStep.Play();
        else
            m_FootStep.Stop();
    }

    private void PlayBlade(int index)
    {
        if (index > 0)
            m_BladeVFXs[index - 1].Stop();
        m_BladeVFXs[index].Play();
    }

    private void PlayHeal()
    {
        m_Heal.SendEvent("OnPlay");
    }

    public void PlaySlash(Vector3 position)
    {
        m_Slash.transform.position = position;
        m_Slash.Play();
    }
}

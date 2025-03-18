using SAS.Utilities.TagSystem;
using System;
using UnityEngine;
using UnityEngine.VFX;
public class PlayerVFXManager : MonoBehaviour
{
    [SerializeField] private VisualEffect m_FootStep;
    [SerializeField] private ParticleSystem m_Blade01;
    [SerializeField] private VisualEffect m_Slash;

    [FieldRequiresSelf] private IEventDispatcher _animationEventDispatcher;

    private void Awake()
    {
        this.Initialize();
        _animationEventDispatcher.Subscribe<bool>("UpdateFootStep", UpdateFootStep);
        _animationEventDispatcher.Subscribe<int>("PlayBlade", PlayBlade);
        _animationEventDispatcher.Subscribe<Vector3>("PlaySlash", PlaySlash);
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
        if (index == 0)
            m_Blade01.Play();
    }

    public void PlaySlash(Vector3 position)
    {
        m_Slash.transform.position = position;
        m_Slash.Play();
    }
}

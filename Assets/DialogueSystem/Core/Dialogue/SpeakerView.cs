using SAS.Utilities.TagSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeakerView : MonoBehaviour
{
    [SerializeField] private TMP_Text m_DisplayNameText;
    [SerializeField] private Animator m_PortraitAnimator;
    [SerializeField] private Image m_Image;
    [SerializeField] private ImageKeyMapConfig m_ImageKeyMapConfig;
    [FieldRequiresSelf] private IAnimatorProcessor _animatorProcessor;

    void Awake()
    {
        this.Initialize();
    }

    public void SetName(string name)
    {
        if (!string.IsNullOrEmpty(name))
            m_DisplayNameText.text = name;
    }

    public void SetImage(string spriteName)
    {
        if (!string.IsNullOrEmpty(spriteName))
            SetImage(m_ImageKeyMapConfig.GetImage(spriteName));
    }

    public void SetImage(Sprite sprite)
    {
        if (sprite)
            m_Image.sprite = sprite;
    }

    public void SetAnimationState(string stateName)
    {
        if (!string.IsNullOrEmpty(stateName))
        {
            if (m_PortraitAnimator)
                m_PortraitAnimator.Play(stateName);
            _animatorProcessor?.Process(stateName);
        }
    }

    internal void SetDisplayValues(string speakerTag, string animationState)
    {
        SetName(speakerTag);
        SetImage(speakerTag);
        SetAnimationState(animationState);
    }
}
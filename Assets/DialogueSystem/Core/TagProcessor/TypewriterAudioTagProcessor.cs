using System.Collections.Generic;
using UnityEngine;

public class TypewriterAudioTagProcessor : BaseTagProcessor
{
    [SerializeField] private ITypewriterAudioEffect _typewriterAudioEffect;
    public override void Process(string tagValue, TagProcessContext context)
    {
        _typewriterAudioEffect.SetCurrentAudioInfo(tagValue);
    }
}
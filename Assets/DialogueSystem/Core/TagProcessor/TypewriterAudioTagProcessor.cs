using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypewriterAudioTagProcessor : BaseTagProcessor
{
    [SerializeField] private ITypewriterAudioEffect _typewriterAudioEffect;
    public override IEnumerable<string> SupportedKeys { get; } = new string[] { "audio" };

    public override void Process(string tagValue, TagProcessContext context)
    {
        if (!CanHandle(tagValue))
            return;
        _typewriterAudioEffect.SetCurrentAudioInfo(tagValue);
    }
}
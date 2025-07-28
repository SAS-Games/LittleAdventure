using System;
using System.Collections.Generic;
using UnityEngine;

public class SpeakerTagProcessor : BaseTagProcessor
{
    [Serializable]
    class Speaker
    {
        public string id;
        public SpeakerView view;
    }
    
    [SerializeField] private List<Speaker> speakers;
    public override IEnumerable<string> SupportedKeys { get; } = new string[] { "speaker" };

    public override void Process(string tagValue, TagProcessContext context)
    {
        if (!CanHandle(tagValue))
            return;
        
        foreach (var s in speakers)
            s.view.gameObject.SetActive(false);
        
        var parsed = context.MetaParser.Parse(tagValue);
        if (parsed.TryGetValue("id", out var speaker))
        {
            SpeakerView speakerView = null;//speakerViews[speaker];
            speakerView.gameObject.SetActive(true);
            foreach (var keyValue in parsed)
            {
                switch (keyValue.Key)
                {
                    case "name":
                        speakerView.SetName(keyValue.Value);
                        break;
                    case "image":
                        speakerView.SetImage(keyValue.Value);
                        break;
                    case "anim":
                        speakerView.SetAnimationState(keyValue.Value);
                        break;
                    default:
                        Debug.LogWarning($" {keyValue.Key}: is not currently being handled for speaker view ");
                        break;
                }
            }
        }
    }
}
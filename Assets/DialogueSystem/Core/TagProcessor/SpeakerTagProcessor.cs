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

    [SerializeField] private List<Speaker> m_Speakers;
    private Dictionary<string, SpeakerView> _speakersUi = new Dictionary<string, SpeakerView>();


    private void Awake()
    {
        foreach (var speaker in m_Speakers)
            _speakersUi.Add(speaker.id, speaker.view);
    }
    public override void Process(string tagValue, TagProcessContext context)
    {
        foreach (var s in m_Speakers)
            s.view.gameObject.SetActive(false);

        var parsed = context.MetaParser.Parse(tagValue);
        if (parsed.TryGetValue("id", out var speaker))
        {
            context.CurrentSpeakerId = speaker;
            SpeakerView speakerView = _speakersUi[speaker];
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
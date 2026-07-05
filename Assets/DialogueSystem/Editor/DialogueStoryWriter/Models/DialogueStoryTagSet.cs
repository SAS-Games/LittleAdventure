using System;
using System.Collections.Generic;

namespace SAS.DialogueSystem.EditorTools
{
    [Serializable]
    public class DialogueStoryTagSet
    {
        public bool useSpeaker = true;
        public string speakerId = "npc";
        public string speakerName = "KAIROS";
        public string portraitKey = string.Empty;
        public string speakerAnimation = "Talk";

        public bool useLocale;
        public string localeKey = string.Empty;

        public bool useLayout;
        public string layoutAnimation = string.Empty;

        public bool useAudio;
        public string audioId = string.Empty;

        public List<DialogueStoryCustomTag> customTags = new List<DialogueStoryCustomTag>();
    }

    [Serializable]
    public class DialogueStoryCustomTag
    {
        public string key = string.Empty;
        public string value = string.Empty;
    }
}

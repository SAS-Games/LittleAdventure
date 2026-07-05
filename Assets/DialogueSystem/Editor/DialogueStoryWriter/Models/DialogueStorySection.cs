using System;
using System.Collections.Generic;

namespace SAS.DialogueSystem.EditorTools
{
    public enum DialogueStorySectionType
    {
        Knot = 0,
        Stitch = 1,
        Function = 2
    }

    [Serializable]
    public class DialogueStorySection
    {
        public DialogueStorySectionType sectionType = DialogueStorySectionType.Knot;
        public string knotName = "start";
        public List<DialogueStoryEntry> entries = new List<DialogueStoryEntry>();
        public bool expanded = true;
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAS.DialogueSystem.EditorTools
{
    public enum DialogueStoryEntryType
    {
        Line = 0,
        Choice = 1,
        Divert = 2,
        ConditionalDivert = 5,
        RawInk = 3,
        End = 4,
        Gather = 6,
        Done = 7,
        Tag = 8,
        ConditionalLine = 9,
        TunnelDivert = 10,
        TunnelReturn = 11,
        ConditionalTunnelDivert = 12
    }

    [Serializable]
    public class DialogueStoryEntry
    {
        public DialogueStoryEntryType type = DialogueStoryEntryType.Line;
        public string text = string.Empty;
        public string targetKnot = string.Empty;
        public bool targetIsTunnel;
        public int choiceDepth = 1;
        public bool stickyChoice;
        public bool suppressChoiceText;
        public bool fallbackChoice;
        public string choiceConditionExpression = string.Empty;
        public int gatherDepth = 1;
        public string conditionExpression = "isCoop";
        public string elseTargetKnot = string.Empty;
        public string rawInk = string.Empty;
        [SerializeReference] public List<DialogueStoryEntry> bodyEntries = new List<DialogueStoryEntry>();
        public DialogueStoryTagSet tags = new DialogueStoryTagSet();
        public bool expanded = true;
    }
}

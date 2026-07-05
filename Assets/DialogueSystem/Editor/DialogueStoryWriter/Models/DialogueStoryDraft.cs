using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SAS.DialogueSystem.EditorTools
{
    [CreateAssetMenu(fileName = "DialogueStoryDraft", menuName = "Dialogue/Story Draft")]
    public class DialogueStoryDraft : ScriptableObject
    {
        public const string DefaultInkFolder = "Assets/DialogueSystem/DialougeAssets";

        public string outputFileName = "NewDialogue";
        public DefaultAsset outputFolder;
        public bool includeCommonInk = true;
        public string commonInkFile = "LA_common.ink";
        public List<string> includeFiles = new List<string>();
        public List<DialogueStoryCustomTag> globalTags = new List<DialogueStoryCustomTag>();
        public bool writeStartDivert = true;
        public string startKnot = string.Empty;
        public bool compileOnSave = true;
        public bool appendEndToLineOnlySections = true;
        public List<DialogueStorySection> sections = new List<DialogueStorySection>();
    }
}

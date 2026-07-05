using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SAS.DialogueSystem.EditorTools
{
    internal static class DialogueStorySidebarGUI
    {
        public static void Draw(DialogueStoryDraft draft, ref Vector2 sectionScroll, ref int selectedSection)
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(230f)))
            {
                GUILayout.Label("Knots / Stitches / Functions", EditorStyles.boldLabel);

                sectionScroll = EditorGUILayout.BeginScrollView(sectionScroll, GUI.skin.box);
                for (int i = 0; i < draft.sections.Count; i++)
                {
                    var section = draft.sections[i];
                    var label = GetSectionLabel(draft, i);
                    if (GUILayout.Toggle(selectedSection == i, label, "Button"))
                        selectedSection = i;
                }
                EditorGUILayout.EndScrollView();

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("+ Knot"))
                        AddSection(draft, DialogueStorySectionType.Knot, ref selectedSection);

                    using (new EditorGUI.DisabledScope(!CanAddStitch(draft, selectedSection)))
                    {
                        if (GUILayout.Button("+ Stitch"))
                            AddSection(draft, DialogueStorySectionType.Stitch, ref selectedSection);
                    }

                    if (GUILayout.Button("+ Fn"))
                        AddSection(draft, DialogueStorySectionType.Function, ref selectedSection);

                    using (new EditorGUI.DisabledScope(draft.sections.Count <= 1))
                    {
                        if (GUILayout.Button("-"))
                            RemoveSelectedSection(draft, ref selectedSection);
                    }
                }
            }
        }

        private static void AddSection(DialogueStoryDraft draft, DialogueStorySectionType sectionType, ref int selectedSection)
        {
            var section = new DialogueStorySection
            {
                sectionType = sectionType,
                knotName = DialogueStoryValidator.GetUniqueSectionName(draft, GetDefaultName(sectionType)),
                entries = new List<DialogueStoryEntry> { DialogueStoryValidator.CreateEntry(DialogueStoryEntryType.Line) }
            };

            if (sectionType == DialogueStorySectionType.Stitch)
            {
                var parentKnotIndex = FindParentKnotIndex(draft, selectedSection);
                if (parentKnotIndex < 0)
                    return;

                var insertIndex = FindEndOfKnotBlock(draft, parentKnotIndex);
                draft.sections.Insert(insertIndex, section);
                selectedSection = insertIndex;
                return;
            }

            draft.sections.Add(section);
            selectedSection = draft.sections.Count - 1;
        }

        private static void RemoveSelectedSection(DialogueStoryDraft draft, ref int selectedSection)
        {
            selectedSection = Mathf.Clamp(selectedSection, 0, draft.sections.Count - 1);
            draft.sections.RemoveAt(selectedSection);
            selectedSection = Mathf.Clamp(selectedSection, 0, draft.sections.Count - 1);
        }

        private static string GetSectionPrefix(DialogueStorySectionType sectionType)
        {
            switch (sectionType)
            {
                case DialogueStorySectionType.Stitch:
                    return "= ";
                case DialogueStorySectionType.Function:
                    return "fn ";
                default:
                    return "=== ";
            }
        }

        private static string GetSectionLabel(DialogueStoryDraft draft, int sectionIndex)
        {
            var section = draft.sections[sectionIndex];
            var name = string.IsNullOrWhiteSpace(section.knotName) ? "<unnamed>" : section.knotName;

            if (section.sectionType == DialogueStorySectionType.Stitch)
            {
                var parent = DialogueStoryValidator.TryGetParentKnotName(draft, sectionIndex, out var parentName)
                    ? parentName
                    : "No parent knot";
                return $"   = {name}  ({parent})";
            }

            return GetSectionPrefix(section.sectionType) + name;
        }

        private static bool CanAddStitch(DialogueStoryDraft draft, int selectedSection)
        {
            return FindParentKnotIndex(draft, selectedSection) >= 0;
        }

        private static int FindParentKnotIndex(DialogueStoryDraft draft, int selectedSection)
        {
            if (draft?.sections == null || draft.sections.Count == 0)
                return -1;

            selectedSection = Mathf.Clamp(selectedSection, 0, draft.sections.Count - 1);
            for (int i = selectedSection; i >= 0; i--)
            {
                var section = draft.sections[i];
                if (section == null)
                    continue;

                if (section.sectionType == DialogueStorySectionType.Function)
                    return -1;

                if (section.sectionType == DialogueStorySectionType.Knot)
                    return i;
            }

            return -1;
        }

        private static int FindEndOfKnotBlock(DialogueStoryDraft draft, int knotIndex)
        {
            var insertIndex = knotIndex + 1;
            while (insertIndex < draft.sections.Count &&
                   draft.sections[insertIndex].sectionType == DialogueStorySectionType.Stitch)
            {
                insertIndex++;
            }

            return insertIndex;
        }

        private static string GetDefaultName(DialogueStorySectionType sectionType)
        {
            switch (sectionType)
            {
                case DialogueStorySectionType.Stitch:
                    return "stitch";
                case DialogueStorySectionType.Function:
                    return "FunctionName()";
                default:
                    return "knot";
            }
        }
    }
}

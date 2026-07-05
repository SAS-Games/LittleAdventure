using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SAS.DialogueSystem.EditorTools
{
    internal static class DialogueStoryEntryGUI
    {
        public static void DrawEntryButtons(DialogueStorySection section)
        {
            DrawEntryButtons(section.entries, 1);
        }

        private static void DrawEntryButtons(IList<DialogueStoryEntry> entries, int choiceDepth)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add Line"))
                    entries.Add(DialogueStoryValidator.CreateEntry(DialogueStoryEntryType.Line));
                if (GUILayout.Button("Add Tag"))
                    entries.Add(DialogueStoryValidator.CreateEntry(DialogueStoryEntryType.Tag));
                if (GUILayout.Button("Add Choice"))
                    entries.Add(CreateChoiceEntry(choiceDepth));
                if (GUILayout.Button("Add Gather"))
                    entries.Add(DialogueStoryValidator.CreateEntry(DialogueStoryEntryType.Gather));
                if (GUILayout.Button("Add Divert"))
                    entries.Add(DialogueStoryValidator.CreateEntry(DialogueStoryEntryType.Divert));
                if (GUILayout.Button("Add Tunnel"))
                    entries.Add(DialogueStoryValidator.CreateEntry(DialogueStoryEntryType.TunnelDivert));
                if (GUILayout.Button("Add Conditional"))
                    entries.Add(DialogueStoryValidator.CreateEntry(DialogueStoryEntryType.ConditionalDivert));
                if (GUILayout.Button("Add Cond Tunnel"))
                    entries.Add(DialogueStoryValidator.CreateEntry(DialogueStoryEntryType.ConditionalTunnelDivert));
                if (GUILayout.Button("Add Return"))
                    entries.Add(DialogueStoryValidator.CreateEntry(DialogueStoryEntryType.TunnelReturn));
                if (GUILayout.Button("Add Raw Ink"))
                    entries.Add(DialogueStoryValidator.CreateEntry(DialogueStoryEntryType.RawInk));
                if (GUILayout.Button("Add End"))
                    entries.Add(DialogueStoryValidator.CreateEntry(DialogueStoryEntryType.End));
                if (GUILayout.Button("Add Done"))
                    entries.Add(DialogueStoryValidator.CreateEntry(DialogueStoryEntryType.Done));
            }
        }

        public static void DrawEntry(DialogueStorySection section, int index, Func<string, string, string> drawTargetField)
        {
            DrawEntry(section.entries, index, drawTargetField, 0);
        }

        private static void DrawEntry(IList<DialogueStoryEntry> entries, int index, Func<string, string, string> drawTargetField, int nestingLevel)
        {
            var entry = entries[index];
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (nestingLevel > 0)
                        GUILayout.Space(nestingLevel * 12f);

                    entry.expanded = EditorGUILayout.Foldout(entry.expanded, InkChoiceParser.GetEntryFoldoutLabel(entry, index), true);

                    if (GUILayout.Button("Up", GUILayout.Width(38f)) && index > 0)
                    {
                        Swap(entries, index, index - 1);
                        return;
                    }

                    if (GUILayout.Button("Down", GUILayout.Width(50f)) && index < entries.Count - 1)
                    {
                        Swap(entries, index, index + 1);
                        return;
                    }

                    if (GUILayout.Button("Copy", GUILayout.Width(46f)))
                    {
                        entries.Insert(index + 1, DialogueStoryValidator.CloneEntry(entry));
                        return;
                    }

                    if (GUILayout.Button("X", GUILayout.Width(24f)))
                    {
                        entries.RemoveAt(index);
                        return;
                    }
                }

                if (!entry.expanded)
                    return;

                entry.type = (DialogueStoryEntryType)EditorGUILayout.EnumPopup("Type", entry.type);

                switch (entry.type)
                {
                    case DialogueStoryEntryType.Line:
                        entry.text = EditorGUILayout.TextArea(entry.text, GUILayout.MinHeight(48f));
                        DialogueStoryTagGUI.DrawTags(entry.tags, true);
                        break;
                    case DialogueStoryEntryType.Tag:
                        DialogueStoryTagGUI.DrawTags(entry.tags, false);
                        EditorGUILayout.HelpBox("Writes a standalone Ink tag line, for example #IMAGE: dog.png or #CLEAR.", MessageType.None);
                        break;
                    case DialogueStoryEntryType.ConditionalLine:
                        entry.conditionExpression = EditorGUILayout.TextField("Condition", entry.conditionExpression);
                        entry.text = EditorGUILayout.TextArea(entry.text, GUILayout.MinHeight(48f));
                        DialogueStoryTagGUI.DrawTags(entry.tags, true);
                        EditorGUILayout.HelpBox("Writes simple conditional text: { condition: line }.", MessageType.None);
                        break;
                    case DialogueStoryEntryType.Choice:
                        entry.text = EditorGUILayout.TextArea(entry.text, GUILayout.MinHeight(38f));
                        entry.targetKnot = drawTargetField("Target", entry.targetKnot);
                        entry.targetIsTunnel = EditorGUILayout.Toggle("Target Is Tunnel", entry.targetIsTunnel);
                        entry.choiceDepth = EditorGUILayout.IntSlider("Choice Depth", Mathf.Max(1, entry.choiceDepth), 1, 6);
                        entry.stickyChoice = EditorGUILayout.Toggle("Sticky Choice (+)", entry.stickyChoice);
                        entry.fallbackChoice = EditorGUILayout.Toggle("Fallback Choice", entry.fallbackChoice);
                        entry.suppressChoiceText = EditorGUILayout.Toggle("Suppress Choice Text []", entry.suppressChoiceText);
                        entry.choiceConditionExpression = EditorGUILayout.TextField("Choice Condition", entry.choiceConditionExpression);
                        DialogueStoryTagGUI.DrawChoiceTags(entry.tags);
                        DrawChoiceBody(entry, drawTargetField, nestingLevel);
                        break;
                    case DialogueStoryEntryType.Gather:
                        entry.text = EditorGUILayout.TextArea(entry.text, GUILayout.MinHeight(38f));
                        entry.gatherDepth = EditorGUILayout.IntSlider("Gather Depth", Mathf.Max(1, entry.gatherDepth), 1, 6);
                        DialogueStoryTagGUI.DrawTags(entry.tags, true);
                        EditorGUILayout.HelpBox("Writes an Ink gather using '-'. Use it to bring choice branches back together.", MessageType.None);
                        break;
                    case DialogueStoryEntryType.Divert:
                        entry.targetKnot = drawTargetField("Target", entry.targetKnot);
                        break;
                    case DialogueStoryEntryType.TunnelDivert:
                        entry.targetKnot = drawTargetField("Target", entry.targetKnot);
                        EditorGUILayout.HelpBox("Writes -> target ->. Ink calls that knot like a subroutine, then returns to the next line after the tunnel returns.", MessageType.None);
                        break;
                    case DialogueStoryEntryType.ConditionalDivert:
                        entry.conditionExpression = EditorGUILayout.TextField("Condition", entry.conditionExpression);
                        entry.targetKnot = drawTargetField("If True", entry.targetKnot);
                        entry.elseTargetKnot = drawTargetField("Else", entry.elseTargetKnot);
                        EditorGUILayout.HelpBox("Example: Condition 'isCoop', If True 'coop_intro', Else 'solo_intro'.", MessageType.None);
                        break;
                    case DialogueStoryEntryType.ConditionalTunnelDivert:
                        entry.conditionExpression = EditorGUILayout.TextField("Condition", entry.conditionExpression);
                        entry.targetKnot = drawTargetField("If True", entry.targetKnot);
                        entry.elseTargetKnot = drawTargetField("Else", entry.elseTargetKnot);
                        EditorGUILayout.HelpBox("Writes conditional tunnel calls, for example { danger: -> trigger_alarm -> }.", MessageType.None);
                        break;
                    case DialogueStoryEntryType.RawInk:
                        entry.rawInk = EditorGUILayout.TextArea(entry.rawInk, GUILayout.MinHeight(74f));
                        break;
                    case DialogueStoryEntryType.TunnelReturn:
                        EditorGUILayout.HelpBox("Writes ->->. Use this inside a tunnel knot to return to the line after the tunnel call.", MessageType.None);
                        break;
                    case DialogueStoryEntryType.End:
                        EditorGUILayout.HelpBox("Writes -> END into the generated Ink.", MessageType.None);
                        break;
                    case DialogueStoryEntryType.Done:
                        EditorGUILayout.HelpBox("Writes -> DONE into the generated Ink.", MessageType.None);
                        break;
                }
            }
        }

        private static void Swap<T>(IList<T> list, int lhs, int rhs)
        {
            (list[lhs], list[rhs]) = (list[rhs], list[lhs]);
        }

        private static DialogueStoryEntry CreateChoiceEntry(int choiceDepth)
        {
            var entry = DialogueStoryValidator.CreateEntry(DialogueStoryEntryType.Choice);
            entry.choiceDepth = Mathf.Max(1, choiceDepth);
            return entry;
        }

        private static void DrawChoiceBody(DialogueStoryEntry entry, Func<string, string, string> drawTargetField, int nestingLevel)
        {
            entry.bodyEntries ??= new List<DialogueStoryEntry>();

            EditorGUILayout.Space(4f);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Choice Body", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Entries here are written indented under this choice. If Target is also set, it is written after the body entries.", MessageType.None);

                DrawEntryButtons(entry.bodyEntries, Mathf.Max(1, entry.choiceDepth + 1));

                if (entry.bodyEntries.Count == 0)
                {
                    EditorGUILayout.HelpBox("No body entries. The choice will use its inline Target, if one is assigned.", MessageType.None);
                    return;
                }

                for (int i = 0; i < entry.bodyEntries.Count; i++)
                    DrawEntry(entry.bodyEntries, i, drawTargetField, nestingLevel + 1);
            }
        }
    }
}

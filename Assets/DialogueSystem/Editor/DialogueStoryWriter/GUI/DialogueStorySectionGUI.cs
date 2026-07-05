using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SAS.DialogueSystem.EditorTools
{
    internal static class DialogueStorySectionGUI
    {
        public static void DrawSectionInspector(
            DialogueStoryDraft draft,
            ref int selectedSection,
            ref Vector2 mainScroll,
            ref Vector2 previewScroll,
            ref bool showSettings,
            ref bool showPreview)
        {
            selectedSection = Mathf.Clamp(selectedSection, 0, draft.sections.Count - 1);
            var section = draft.sections[selectedSection];

            mainScroll = EditorGUILayout.BeginScrollView(mainScroll);

            DrawSettings(draft, ref showSettings);
            DrawHierarchyValidation(draft);

            GUILayout.Space(6f);
            section.sectionType = (DialogueStorySectionType)EditorGUILayout.EnumPopup("Ink Header", section.sectionType);
            section.knotName = EditorGUILayout.TextField(GetSectionNameLabel(section.sectionType), section.knotName);

            GUILayout.Space(8f);
            DialogueStoryEntryGUI.DrawEntryButtons(section);

            GUILayout.Space(8f);
            for (int i = 0; i < section.entries.Count; i++)
                DialogueStoryEntryGUI.DrawEntry(section, i, (label, current) => DrawTargetField(draft, label, current));

            GUILayout.Space(10f);
            DrawPreview(draft, ref previewScroll, ref showPreview);

            EditorGUILayout.EndScrollView();
        }

        private static void DrawSettings(DialogueStoryDraft draft, ref bool showSettings)
        {
            showSettings = EditorGUILayout.Foldout(showSettings, "Output Settings", true);
            if (!showSettings)
                return;

            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                draft.outputFileName = EditorGUILayout.TextField("Ink File Name", draft.outputFileName);
                draft.outputFolder = (DefaultAsset)EditorGUILayout.ObjectField("Output Folder", draft.outputFolder, typeof(DefaultAsset), false);
                draft.includeCommonInk = EditorGUILayout.Toggle("Include Common Ink", draft.includeCommonInk);
                using (new EditorGUI.DisabledScope(!draft.includeCommonInk))
                    DrawIncludeFiles(draft);

                DrawGlobalTags(draft);

                draft.writeStartDivert = EditorGUILayout.Toggle("Write Start Divert", draft.writeStartDivert);
                using (new EditorGUI.DisabledScope(!draft.writeStartDivert))
                    draft.startKnot = DrawTargetField(draft, "Start Target", draft.startKnot);

                draft.compileOnSave = EditorGUILayout.Toggle("Compile On Save", draft.compileOnSave);
                draft.appendEndToLineOnlySections = EditorGUILayout.Toggle("Auto End Line-Only Sections", draft.appendEndToLineOnlySections);
            }
        }

        private static void DrawIncludeFiles(DialogueStoryDraft draft)
        {
            draft.includeFiles ??= new List<string>();

            if (draft.includeFiles.Count == 0)
            {
                draft.commonInkFile = EditorGUILayout.TextField("Common Ink File", draft.commonInkFile);
                if (GUILayout.Button("Use Multiple Includes"))
                    draft.includeFiles.Add(draft.commonInkFile);
                return;
            }

            GUILayout.Label("Include Files", EditorStyles.miniBoldLabel);
            for (int i = 0; i < draft.includeFiles.Count; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    draft.includeFiles[i] = EditorGUILayout.TextField(draft.includeFiles[i]);
                    if (GUILayout.Button("X", GUILayout.Width(24f)))
                    {
                        draft.includeFiles.RemoveAt(i);
                        i--;
                    }
                }
            }

            if (GUILayout.Button("Add Include"))
                draft.includeFiles.Add(string.Empty);
        }

        private static void DrawHierarchyValidation(DialogueStoryDraft draft)
        {
            var errors = DialogueStoryValidator.GetSectionHierarchyErrors(draft);
            foreach (var error in errors)
                EditorGUILayout.HelpBox(error, MessageType.Error);
        }

        private static void DrawGlobalTags(DialogueStoryDraft draft)
        {
            draft.globalTags ??= new List<DialogueStoryCustomTag>();

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Global Tags", EditorStyles.miniBoldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+", GUILayout.Width(24f)))
                    draft.globalTags.Add(new DialogueStoryCustomTag());
            }

            for (int i = 0; i < draft.globalTags.Count; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    draft.globalTags[i].key = EditorGUILayout.TextField(draft.globalTags[i].key);
                    draft.globalTags[i].value = EditorGUILayout.TextField(draft.globalTags[i].value);
                    if (GUILayout.Button("X", GUILayout.Width(24f)))
                    {
                        draft.globalTags.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        private static string DrawTargetField(DialogueStoryDraft draft, string label, string current)
        {
            var names = DialogueStoryValidator.GetSectionNames(draft);
            var currentIndex = Array.IndexOf(names, current);
            if (currentIndex < 0 && !string.IsNullOrWhiteSpace(current))
            {
                var extendedNames = new List<string>(names) { current };
                names = extendedNames.ToArray();
                currentIndex = names.Length - 1;
            }

            currentIndex = Mathf.Max(0, currentIndex);
            var newIndex = EditorGUILayout.Popup(label, currentIndex, names);
            var selected = names.Length == 0 ? string.Empty : names[newIndex];
            return selected == "<none>" ? string.Empty : selected;
        }

        private static string GetSectionNameLabel(DialogueStorySectionType sectionType)
        {
            switch (sectionType)
            {
                case DialogueStorySectionType.Stitch:
                    return "Stitch Name";
                case DialogueStorySectionType.Function:
                    return "Function Signature";
                default:
                    return "Knot Name";
            }
        }

        private static void DrawPreview(DialogueStoryDraft draft, ref Vector2 previewScroll, ref bool showPreview)
        {
            showPreview = EditorGUILayout.Foldout(showPreview, "Generated Ink Preview", true);
            if (!showPreview)
                return;

            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                previewScroll = EditorGUILayout.BeginScrollView(previewScroll, GUILayout.MinHeight(170f));
                EditorGUILayout.TextArea(DialogueInkBuilder.Build(draft), GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }
        }
    }
}

using UnityEditor;
using UnityEngine;

namespace SAS.DialogueSystem.EditorTools
{
    internal static class DialogueStoryToolbarGUI
    {
        public static void Draw(
            ref DialogueStoryDraft draft,
            ref int selectedSection,
            System.Action newDraft,
            System.Action importInk,
            System.Action saveInk,
            System.Action saveAndCompile,
            System.Action pingInk)
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                var newDraftValue = (DialogueStoryDraft)EditorGUILayout.ObjectField(draft, typeof(DialogueStoryDraft), false, GUILayout.MinWidth(220f));
                if (newDraftValue != draft)
                {
                    draft = newDraftValue;
                    selectedSection = 0;
                }

                if (GUILayout.Button("New Draft", EditorStyles.toolbarButton, GUILayout.Width(84f)))
                    newDraft?.Invoke();

                if (GUILayout.Button("Import Ink", EditorStyles.toolbarButton, GUILayout.Width(84f)))
                    importInk?.Invoke();

                using (new EditorGUI.DisabledScope(draft == null))
                {
                    if (GUILayout.Button("Save Ink", EditorStyles.toolbarButton, GUILayout.Width(74f)))
                        saveInk?.Invoke();

                    if (GUILayout.Button("Save + Compile", EditorStyles.toolbarButton, GUILayout.Width(106f)))
                        saveAndCompile?.Invoke();

                    if (GUILayout.Button("Ping Ink", EditorStyles.toolbarButton, GUILayout.Width(70f)))
                        pingInk?.Invoke();
                }
            }
        }

        public static void DrawEmptyState(System.Action createDraft)
        {
            GUILayout.FlexibleSpace();
            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
            {
                GUILayout.Label("Create or assign a Dialogue Story Draft.", EditorStyles.boldLabel);
                GUILayout.Label("Designers can write dialogue, choices, speaker data, localization keys, layout animation, and audio tags here. The tool generates .ink and can compile it through the Ink Unity Integration.");
                GUILayout.Space(8f);
                if (GUILayout.Button("Create Dialogue Story Draft", GUILayout.Height(32f)))
                    createDraft?.Invoke();
            }
            GUILayout.FlexibleSpace();
        }
    }
}

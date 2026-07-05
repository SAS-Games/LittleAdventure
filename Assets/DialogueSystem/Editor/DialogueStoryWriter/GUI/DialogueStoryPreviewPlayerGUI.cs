using UnityEditor;
using UnityEngine;

namespace SAS.DialogueSystem.EditorTools
{
    internal static class DialogueStoryPreviewPlayerGUI
    {
        public static void Draw(DialogueStoryDraft draft, DialogueStoryPreviewPlayer player, ref Vector2 scroll)
        {
            GUILayout.Space(10f);
            GUILayout.Label("Edit Mode Player", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(player.IsPlaying ? "Restart" : "Play", GUILayout.Width(80f)))
                    {
                        if (!player.Play(draft, out var error))
                            EditorUtility.DisplayDialog("Preview Compile Failed", error, "OK");
                    }

                    using (new EditorGUI.DisabledScope(!player.CanContinue))
                    {
                        if (GUILayout.Button("Continue", GUILayout.Width(80f)))
                            player.Continue();
                    }

                    using (new EditorGUI.DisabledScope(!player.IsPlaying))
                    {
                        if (GUILayout.Button("Stop", GUILayout.Width(80f)))
                            player.Stop();
                    }

                    GUILayout.FlexibleSpace();
                    GUILayout.Label(player.IsPlaying ? "Playing generated Ink in edit mode" : "Not playing", EditorStyles.miniLabel);
                }

                if (player.CompileMessages.Count > 0)
                {
                    foreach (var message in player.CompileMessages)
                        EditorGUILayout.HelpBox(message, MessageType.Warning);
                }

                scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.MinHeight(180f));
                foreach (var line in player.History)
                    DrawHistoryLine(line);
                EditorGUILayout.EndScrollView();

                if (player.Choices.Count > 0)
                {
                    GUILayout.Label("Choices", EditorStyles.miniBoldLabel);
                    for (int i = 0; i < player.Choices.Count; i++)
                    {
                        var choice = player.Choices[i];
                        if (GUILayout.Button($"{i + 1}. {choice.text}", GUILayout.MinHeight(24f)))
                            player.Choose(i);
                    }
                }
                else if (player.IsPlaying && !player.CanContinue)
                {
                    GUILayout.Label("No choices available. Story is waiting at an end state.", EditorStyles.miniLabel);
                }
            }
        }

        private static void DrawHistoryLine(DialogueStoryPreviewLine line)
        {
            var previousWordWrap = EditorStyles.label.wordWrap;
            EditorStyles.label.wordWrap = true;

            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                var style = line.isChoice ? EditorStyles.boldLabel : EditorStyles.label;
                if (!string.IsNullOrWhiteSpace(line.text))
                    EditorGUILayout.LabelField(line.text, style);

                if (line.tags != null && line.tags.Count > 0)
                    EditorGUILayout.LabelField("# " + string.Join("  # ", line.tags), EditorStyles.miniLabel);
            }

            EditorStyles.label.wordWrap = previousWordWrap;
        }
    }
}

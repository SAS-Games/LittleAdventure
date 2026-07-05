using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SAS.DialogueSystem.EditorTools
{
    internal static class DialogueStoryTagGUI
    {
        public static void DrawTags(DialogueStoryTagSet tags, bool allowSpeaker)
        {
            GUILayout.Space(4f);
            GUILayout.Label("Tags", EditorStyles.boldLabel);

            if (allowSpeaker)
            {
                tags.useSpeaker = EditorGUILayout.Toggle("Speaker", tags.useSpeaker);
                using (new EditorGUI.DisabledScope(!tags.useSpeaker))
                {
                    tags.speakerId = EditorGUILayout.TextField("Speaker Id", tags.speakerId);
                    tags.speakerName = EditorGUILayout.TextField("Display Name", tags.speakerName);
                    tags.portraitKey = EditorGUILayout.TextField("Portrait Key", tags.portraitKey);
                    tags.speakerAnimation = EditorGUILayout.TextField("Speaker Anim", tags.speakerAnimation);
                }
            }

            tags.useLocale = EditorGUILayout.Toggle("Localization", tags.useLocale);
            using (new EditorGUI.DisabledScope(!tags.useLocale))
                tags.localeKey = EditorGUILayout.TextField("Locale Key", tags.localeKey);

            tags.useLayout = EditorGUILayout.Toggle("Layout Animation", tags.useLayout);
            using (new EditorGUI.DisabledScope(!tags.useLayout))
                tags.layoutAnimation = EditorGUILayout.TextField("Layout State", tags.layoutAnimation);

            tags.useAudio = EditorGUILayout.Toggle("Typewriter Audio", tags.useAudio);
            using (new EditorGUI.DisabledScope(!tags.useAudio))
                tags.audioId = EditorGUILayout.TextField("Audio Id", tags.audioId);

            DrawCustomTags(tags);
        }

        public static void DrawChoiceTags(DialogueStoryTagSet tags)
        {
            tags.useSpeaker = false;
            tags.useLayout = false;
            tags.useAudio = false;

            GUILayout.Space(4f);
            GUILayout.Label("Choice Tags", EditorStyles.boldLabel);

            tags.useLocale = EditorGUILayout.Toggle("Localization", tags.useLocale);
            using (new EditorGUI.DisabledScope(!tags.useLocale))
                tags.localeKey = EditorGUILayout.TextField("Locale Key", tags.localeKey);

            DrawCustomTags(tags);
        }

        public static void DrawCustomTags(DialogueStoryTagSet tags)
        {
            tags.customTags ??= new List<DialogueStoryCustomTag>();

            GUILayout.Space(2f);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Custom Tags", EditorStyles.miniBoldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+", GUILayout.Width(24f)))
                    tags.customTags.Add(new DialogueStoryCustomTag());
            }

            for (int i = 0; i < tags.customTags.Count; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    tags.customTags[i].key = EditorGUILayout.TextField(tags.customTags[i].key);
                    tags.customTags[i].value = EditorGUILayout.TextField(tags.customTags[i].value);
                    if (GUILayout.Button("X", GUILayout.Width(24f)))
                    {
                        tags.customTags.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
    }
}

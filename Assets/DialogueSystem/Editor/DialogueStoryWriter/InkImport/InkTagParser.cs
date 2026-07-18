using System;
using System.Collections.Generic;

namespace SAS.DialogueSystem.EditorTools
{
    internal static class InkTagParser
    {
        public static DialogueStoryTagSet CreateEmptyTagSet()
        {
            return new DialogueStoryTagSet
            {
                useSpeaker = false,
                speakerId = string.Empty,
                speakerName = string.Empty,
                portraitKey = string.Empty,
                speakerAnimation = string.Empty,
                useLocale = false,
                localeKey = string.Empty,
                useLayout = false,
                layoutAnimation = string.Empty,
                useAudio = false,
                audioId = string.Empty,
                customTags = new List<DialogueStoryCustomTag>()
            };
        }

        public static void ParseTextAndTags(string line, out string text, out DialogueStoryTagSet tags)
        {
            tags = CreateEmptyTagSet();
            var firstTagIndex = line.IndexOf('#');
            if (firstTagIndex < 0)
            {
                text = line.Trim();
                return;
            }

            text = line.Substring(0, firstTagIndex).Trim();
            var tagBlock = line.Substring(firstTagIndex + 1);
            var rawTags = tagBlock.Split('#');

            foreach (var rawTag in rawTags)
                ParseTagIntoSet(rawTag.Trim(), tags);
        }

        public static void AddGlobalTags(DialogueStoryDraft draft, DialogueStoryTagSet tags)
        {
            tags.customTags ??= new List<DialogueStoryCustomTag>();

            foreach (var customTag in tags.customTags)
            {
                if (customTag == null || string.IsNullOrWhiteSpace(customTag.key))
                    continue;

                draft.globalTags.Add(new DialogueStoryCustomTag
                {
                    key = customTag.key,
                    value = customTag.value
                });
            }

            if (tags.useLocale && !string.IsNullOrWhiteSpace(tags.localeKey))
                draft.globalTags.Add(new DialogueStoryCustomTag { key = "locale", value = tags.localeKey });

            if (tags.useLayout && !string.IsNullOrWhiteSpace(tags.layoutAnimation))
                draft.globalTags.Add(new DialogueStoryCustomTag { key = "layout", value = tags.layoutAnimation });

            if (tags.useAudio && !string.IsNullOrWhiteSpace(tags.audioId))
                draft.globalTags.Add(new DialogueStoryCustomTag { key = "audio", value = tags.audioId });
        }

        private static void ParseTagIntoSet(string rawTag, DialogueStoryTagSet tags)
        {
            if (string.IsNullOrWhiteSpace(rawTag))
                return;

            var splitIndex = rawTag.IndexOf(':');
            if (splitIndex < 0)
            {
                tags.customTags.Add(new DialogueStoryCustomTag { key = rawTag.Trim() });
                return;
            }

            var key = rawTag.Substring(0, splitIndex).Trim();
            var value = rawTag.Substring(splitIndex + 1).Trim();

            if (key.Equals("speaker", StringComparison.OrdinalIgnoreCase))
            {
                ParseSpeakerTag(value, tags);
            }
            else if (key.Equals("speaker_name", StringComparison.OrdinalIgnoreCase))
            {
                tags.useSpeaker = true;
                tags.speakerName = value;
            }
            else if (key.Equals("portrait", StringComparison.OrdinalIgnoreCase) ||
                     key.Equals("speaker_portrait", StringComparison.OrdinalIgnoreCase))
            {
                tags.useSpeaker = true;
                tags.portraitKey = value;
            }
            else if (key.Equals("animation", StringComparison.OrdinalIgnoreCase) ||
                     key.Equals("speaker_animation", StringComparison.OrdinalIgnoreCase))
            {
                tags.useSpeaker = true;
                tags.speakerAnimation = value;
            }
            else if (key.Equals("locale", StringComparison.OrdinalIgnoreCase))
            {
                tags.useLocale = true;
                tags.localeKey = value;
            }
            else if (key.Equals("layout", StringComparison.OrdinalIgnoreCase))
            {
                tags.useLayout = true;
                tags.layoutAnimation = value;
            }
            else if (key.Equals("audio", StringComparison.OrdinalIgnoreCase))
            {
                tags.useAudio = true;
                tags.audioId = value;
            }
            else
            {
                tags.customTags.Add(new DialogueStoryCustomTag
                {
                    key = key,
                    value = value
                });
            }
        }

        private static void ParseSpeakerTag(string value, DialogueStoryTagSet tags)
        {
            tags.useSpeaker = true;
            tags.speakerId = value.Trim();
        }
    }
}

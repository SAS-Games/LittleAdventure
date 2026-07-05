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
                draft.globalTags.Add(new DialogueStoryCustomTag { key = "local", value = tags.localeKey });

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
            else if (key.Equals("local", StringComparison.OrdinalIgnoreCase) || key.Equals("locale", StringComparison.OrdinalIgnoreCase))
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
            foreach (var rawPart in value.Split(','))
            {
                var part = rawPart.Trim();
                var split = part.Split(new[] { "::" }, 2, StringSplitOptions.None);
                if (split.Length != 2)
                    continue;

                var key = split[0].Trim();
                var fieldValue = split[1].Trim();

                if (key.Equals("id", StringComparison.OrdinalIgnoreCase))
                    tags.speakerId = fieldValue;
                else if (key.Equals("name", StringComparison.OrdinalIgnoreCase))
                    tags.speakerName = fieldValue;
                else if (key.Equals("image", StringComparison.OrdinalIgnoreCase))
                    tags.portraitKey = fieldValue;
                else if (key.Equals("anim", StringComparison.OrdinalIgnoreCase))
                    tags.speakerAnimation = fieldValue;
            }
        }
    }
}

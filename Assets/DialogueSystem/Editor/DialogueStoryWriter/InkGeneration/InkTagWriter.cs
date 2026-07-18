using System.Collections.Generic;

namespace SAS.DialogueSystem.EditorTools
{
    internal static class InkTagWriter
    {
        public static string BuildTagSuffix(DialogueStoryTagSet tags)
        {
            if (tags == null)
                return string.Empty;

            var output = new List<string>();

            if (tags.useSpeaker)
            {
                AddCanonicalTag(output, "speaker", tags.speakerId);
                AddCanonicalTag(output, "speaker_name", tags.speakerName);
                AddCanonicalTag(output, "portrait", tags.portraitKey);
                AddCanonicalTag(output, "animation", tags.speakerAnimation);
            }

            if (tags.useLocale && !string.IsNullOrWhiteSpace(tags.localeKey))
                output.Add("locale:" + InkSanitizer.CleanTagValue(tags.localeKey));

            if (tags.useLayout && !string.IsNullOrWhiteSpace(tags.layoutAnimation))
                output.Add("layout:" + InkSanitizer.CleanTagValue(tags.layoutAnimation));

            if (tags.useAudio && !string.IsNullOrWhiteSpace(tags.audioId))
                output.Add("audio:" + InkSanitizer.CleanTagValue(tags.audioId));

            tags.customTags ??= new List<DialogueStoryCustomTag>();
            foreach (var customTag in tags.customTags)
            {
                if (customTag == null || string.IsNullOrWhiteSpace(customTag.key))
                    continue;

                output.Add(BuildCustomTagToken(customTag));
            }

            return output.Count == 0 ? string.Empty : " #" + string.Join(" #", output);
        }

        public static string BuildStandaloneTagLine(DialogueStoryTagSet tags)
        {
            var suffix = BuildTagSuffix(tags).Trim();
            return string.IsNullOrWhiteSpace(suffix) ? string.Empty : suffix;
        }

        public static string BuildCustomTagLine(DialogueStoryCustomTag tag)
        {
            var token = BuildCustomTagToken(tag);
            return string.IsNullOrWhiteSpace(token) ? string.Empty : "# " + token;
        }

        public static string BuildCustomTagToken(DialogueStoryCustomTag tag)
        {
            if (tag == null || string.IsNullOrWhiteSpace(tag.key))
                return string.Empty;

            var key = InkSanitizer.CleanTagKey(tag.key);
            var value = InkSanitizer.CleanTagValue(tag.value);
            return string.IsNullOrWhiteSpace(value) ? key : $"{key}:{value}";
        }

        public static bool HasAnyTag(DialogueStoryTagSet tags)
        {
            if (tags == null)
                return false;

            return tags.useSpeaker ||
                   tags.useLocale ||
                   tags.useLayout ||
                   tags.useAudio ||
                   (tags.customTags != null && tags.customTags.Count > 0);
        }

        private static void AddCanonicalTag(List<string> output, string key, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                output.Add($"{key}:{InkSanitizer.CleanTagValue(value)}");
        }
    }
}

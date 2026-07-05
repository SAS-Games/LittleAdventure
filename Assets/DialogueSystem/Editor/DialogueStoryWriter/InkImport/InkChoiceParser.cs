using System;
using System.Collections.Generic;
using UnityEditor;

namespace SAS.DialogueSystem.EditorTools
{
    internal static class InkChoiceParser
    {
        public static bool TryParseChoice(string[] lines, ref int index, out DialogueStoryEntry entry)
        {
            entry = null;
            var line = InkSanitizer.NormalizeImportedLine(lines[index]);
            if (string.IsNullOrWhiteSpace(line) || (line[0] != '*' && line[0] != '+'))
                return false;

            var baseIndent = InkLineParser.CountLeadingWhitespace(lines[index]);
            var isSticky = line[0] == '+';
            var depth = InkSanitizer.CountLeadingMarkers(line, line[0]);
            var rest = line.Substring(depth).Trim();
            var condition = ExtractLeadingChoiceCondition(ref rest);
            string target = string.Empty;
            var targetIsTunnel = false;

            if (TrySplitInlineTunnelDivert(rest, out var textPart, out var inlineTunnelTarget))
            {
                rest = textPart;
                target = inlineTunnelTarget;
                targetIsTunnel = true;
            }
            else if (TrySplitInlineDivert(rest, out textPart, out var inlineTarget))
            {
                rest = textPart;
                target = inlineTarget;
            }

            InkTagParser.ParseTextAndTags(rest, out var text, out var tags);
            var suppressText = TryUnwrapSuppressedChoiceText(text, out text);
            text = InkSanitizer.CleanChoiceImportToken(text);
            entry = new DialogueStoryEntry
            {
                type = DialogueStoryEntryType.Choice,
                text = text,
                targetKnot = InkSanitizer.CleanChoiceImportToken(target),
                targetIsTunnel = targetIsTunnel,
                choiceDepth = depth,
                stickyChoice = isSticky,
                suppressChoiceText = suppressText,
                fallbackChoice = string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(target),
                choiceConditionExpression = condition,
                tags = tags
            };

            if (TryReadChoiceBody(lines, ref index, baseIndent, entry))
            {
                PopulateImportedChoiceLocalization(entry);
                return true;
            }

            var nextIndex = InkLineParser.FindNextMeaningfulLine(lines, index + 1);
            if (string.IsNullOrWhiteSpace(entry.targetKnot) && nextIndex >= 0 && InkLineParser.TryParseDivertTarget(lines[nextIndex].Trim(), out target))
            {
                entry.targetKnot = InkSanitizer.CleanChoiceImportToken(target);
                index = nextIndex;
            }

            PopulateImportedChoiceLocalization(entry);

            return true;
        }

        private static bool TryReadChoiceBody(string[] lines, ref int index, int baseIndent, DialogueStoryEntry choiceEntry)
        {
            var bodyEntries = new List<DialogueStoryEntry>();
            var lastConsumedIndex = index;

            for (int i = index + 1; i < lines.Length; i++)
            {
                var rawLine = lines[i];
                var trimmed = InkSanitizer.NormalizeImportedLine(rawLine);

                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("//", StringComparison.Ordinal))
                    continue;

                if (InkLineParser.TryParseSectionHeader(trimmed, out _, out _))
                    break;

                if (InkLineParser.CountLeadingWhitespace(rawLine) <= baseIndent)
                    break;

                var bodyIndex = i;
                if (TryParseChoiceBodyEntry(lines, ref bodyIndex, out var bodyEntry))
                {
                    bodyEntries.Add(bodyEntry);
                    i = bodyIndex;
                    lastConsumedIndex = bodyIndex;
                }
            }

            if (bodyEntries.Count == 0)
                return false;

            choiceEntry.bodyEntries = bodyEntries;
            index = lastConsumedIndex;
            return true;
        }

        private static bool TryParseChoiceBodyEntry(string[] lines, ref int index, out DialogueStoryEntry entry)
        {
            entry = null;
            var rawLine = lines[index];
            var trimmed = InkSanitizer.NormalizeImportedLine(rawLine);

            if (InkLineParser.TryParseStandaloneTags(trimmed, out entry))
                return true;

            if (InkConditionalParser.TryReadConditionalDivert(lines, ref index, out entry))
                return true;

            if (InkLineParser.TryParseEnd(trimmed))
            {
                entry = new DialogueStoryEntry { type = DialogueStoryEntryType.End };
                return true;
            }

            if (InkLineParser.TryParseDone(trimmed))
            {
                entry = new DialogueStoryEntry { type = DialogueStoryEntryType.Done };
                return true;
            }

            if (InkLineParser.TryParseTunnelReturn(trimmed))
            {
                entry = new DialogueStoryEntry { type = DialogueStoryEntryType.TunnelReturn };
                return true;
            }

            if (InkLineParser.TryParseTunnelDivertTarget(trimmed, out var tunnelTarget))
            {
                entry = new DialogueStoryEntry
                {
                    type = DialogueStoryEntryType.TunnelDivert,
                    targetKnot = tunnelTarget
                };
                return true;
            }

            if (InkLineParser.TryParseDivertTarget(trimmed, out var target))
            {
                entry = new DialogueStoryEntry
                {
                    type = DialogueStoryEntryType.Divert,
                    targetKnot = target
                };
                return true;
            }

            if (TryParseChoice(lines, ref index, out entry))
                return true;

            if (InkLineParser.TryParseGather(trimmed, out entry))
                return true;

            if (InkLineParser.LooksLikeRawInk(trimmed))
            {
                entry = new DialogueStoryEntry
                {
                    type = DialogueStoryEntryType.RawInk,
                    rawInk = trimmed
                };
                return true;
            }

            InkTagParser.ParseTextAndTags(trimmed, out var text, out var tags);
            if (string.IsNullOrWhiteSpace(text))
                return false;

            entry = new DialogueStoryEntry
            {
                type = DialogueStoryEntryType.Line,
                text = text,
                tags = tags
            };
            return true;
        }

        public static string GetEntryFoldoutLabel(DialogueStoryEntry entry, int index)
        {
            var typeName = ObjectNames.NicifyVariableName(entry.type.ToString());
            var text = InkSanitizer.CleanPreviewText(entry.text, 44);

            if (entry.type == DialogueStoryEntryType.Choice && !string.IsNullOrWhiteSpace(text))
            {
                var choiceText = entry.suppressChoiceText ? $"[{text}]" : text;
                var arrow = entry.targetIsTunnel ? " ->" : string.Empty;
                return string.IsNullOrWhiteSpace(entry.targetKnot)
                    ? $"{index + 1}. Choice: {choiceText}"
                    : $"{index + 1}. Choice: {choiceText} -> {entry.targetKnot}{arrow}";
            }

            if ((entry.type == DialogueStoryEntryType.Line || entry.type == DialogueStoryEntryType.ConditionalLine) && !string.IsNullOrWhiteSpace(text))
                return $"{index + 1}. {typeName}: {text}";

            return $"{index + 1}. {typeName}";
        }

        public static void PopulateImportedChoiceLocalization(DialogueStoryEntry entry)
        {
            if (entry.tags == null)
                entry.tags = InkTagParser.CreateEmptyTagSet();

            NormalizeChoiceText(entry);

            entry.tags.useSpeaker = false;
            entry.tags.useLayout = false;
            entry.tags.useAudio = false;

            entry.tags.customTags ??= new List<DialogueStoryCustomTag>();
            foreach (var customTag in entry.tags.customTags)
            {
                if (customTag == null)
                    continue;

                customTag.key = InkSanitizer.CleanChoiceImportToken(customTag.key);
                customTag.value = InkSanitizer.CleanChoiceImportToken(customTag.value);
            }

            if (!string.IsNullOrWhiteSpace(entry.tags.localeKey))
            {
                entry.tags.localeKey = InkSanitizer.ToSnakeCaseKey(InkSanitizer.CleanChoiceImportToken(entry.tags.localeKey));
                return;
            }

            var keySource = !string.IsNullOrWhiteSpace(entry.targetKnot)
                ? entry.targetKnot
                : entry.text;
            var localeKey = InkSanitizer.ToSnakeCaseKey(keySource);

            if (string.IsNullOrWhiteSpace(localeKey))
                return;

            entry.tags.useLocale = true;
            entry.tags.localeKey = localeKey;
        }

        public static void NormalizeChoiceText(DialogueStoryEntry entry)
        {
            if (entry == null || entry.type != DialogueStoryEntryType.Choice)
                return;

            var text = InkSanitizer.CleanDialogueText(entry.text);
            if (string.IsNullOrWhiteSpace(text))
                return;

            var startsSuppressed = text.StartsWith("[", StringComparison.Ordinal);
            var endsSuppressed = text.EndsWith("]", StringComparison.Ordinal);

            if (startsSuppressed || endsSuppressed)
            {
                entry.text = InkSanitizer.CleanChoiceImportToken(text);
                entry.suppressChoiceText = true;
                return;
            }

            entry.text = text;
        }

        private static string ExtractLeadingChoiceCondition(ref string choiceText)
        {
            choiceText = choiceText.Trim();
            if (!choiceText.StartsWith("{", StringComparison.Ordinal))
                return string.Empty;

            var closeIndex = choiceText.IndexOf('}');
            if (closeIndex <= 0)
                return string.Empty;

            var condition = choiceText.Substring(1, closeIndex - 1).Trim();
            if (string.IsNullOrWhiteSpace(condition) || condition.Contains(":"))
                return string.Empty;

            choiceText = choiceText.Substring(closeIndex + 1).Trim();
            return condition;
        }

        private static bool TrySplitInlineDivert(string text, out string textPart, out string target)
        {
            textPart = text;
            target = string.Empty;

            var divertIndex = text.IndexOf("->", StringComparison.Ordinal);
            if (divertIndex < 0)
                return false;

            if (!InkLineParser.TryExtractDivertTarget(text.Substring(divertIndex), out target))
                return false;

            textPart = text.Substring(0, divertIndex).Trim();
            return true;
        }

        private static bool TrySplitInlineTunnelDivert(string text, out string textPart, out string target)
        {
            textPart = text;
            target = string.Empty;

            var divertIndex = text.IndexOf("->", StringComparison.Ordinal);
            if (divertIndex < 0)
                return false;

            if (!InkLineParser.TryExtractTunnelDivertTarget(text.Substring(divertIndex), out target))
                return false;

            textPart = text.Substring(0, divertIndex).Trim();
            return true;
        }

        private static bool TryUnwrapSuppressedChoiceText(string text, out string unwrappedText)
        {
            unwrappedText = InkSanitizer.CleanChoiceImportToken(text);
            text = text.Trim();

            if (text.Length < 2 || text[0] != '[' || text[text.Length - 1] != ']')
                return false;

            unwrappedText = InkSanitizer.CleanChoiceImportToken(text.Substring(1, text.Length - 2));
            return true;
        }
    }
}

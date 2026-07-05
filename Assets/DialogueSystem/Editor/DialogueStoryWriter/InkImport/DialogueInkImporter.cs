using System;
using System.Collections.Generic;

namespace SAS.DialogueSystem.EditorTools
{
    internal static class DialogueInkImporter
    {
        public static void ImportIntoDraft(DialogueStoryDraft draft, string inkText)
        {
            draft.includeCommonInk = false;
            draft.writeStartDivert = true;
            draft.includeFiles ??= new List<string>();
            draft.includeFiles.Clear();
            draft.globalTags ??= new List<DialogueStoryCustomTag>();
            draft.globalTags.Clear();

            var lines = InkSanitizer.NormalizeLineEndings(inkText).Split('\n');
            DialogueStorySection currentSection = null;

            for (int i = 0; i < lines.Length; i++)
            {
                var rawLine = lines[i];
                var trimmed = InkSanitizer.NormalizeImportedLine(rawLine);

                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("//", StringComparison.Ordinal))
                    continue;

                if (InkLineParser.TryParseInclude(trimmed, out var includePath))
                {
                    draft.includeCommonInk = true;
                    if (draft.includeFiles.Count == 0)
                        draft.commonInkFile = includePath;
                    draft.includeFiles.Add(includePath);
                    continue;
                }

                if (InkLineParser.TryParseSectionHeader(trimmed, out var sectionName, out var sectionType))
                {
                    currentSection = new DialogueStorySection
                    {
                        sectionType = sectionType,
                        knotName = sectionName,
                        entries = new List<DialogueStoryEntry>()
                    };
                    draft.sections.Add(currentSection);

                    if (string.IsNullOrWhiteSpace(draft.startKnot))
                        draft.startKnot = sectionName;

                    continue;
                }

                if (InkLineParser.TryParseStandaloneTags(trimmed, out var tagEntry))
                {
                    if (currentSection == null)
                        InkTagParser.AddGlobalTags(draft, tagEntry.tags);
                    else
                        currentSection.entries.Add(tagEntry);

                    continue;
                }

                if (currentSection == null && InkLineParser.TryParseDivertTarget(trimmed, out var rootTarget))
                {
                    if (!rootTarget.Equals("END", StringComparison.OrdinalIgnoreCase))
                    {
                        draft.writeStartDivert = true;
                        draft.startKnot = rootTarget;
                        continue;
                    }
                }

                currentSection ??= CreateImportedSection("start", draft);

                if (InkConditionalParser.TryReadConditionalDivert(lines, ref i, out var conditionalEntry))
                {
                    currentSection.entries.Add(conditionalEntry);
                    continue;
                }

                if (InkLineParser.TryParseEnd(trimmed))
                {
                    currentSection.entries.Add(new DialogueStoryEntry { type = DialogueStoryEntryType.End });
                    continue;
                }

                if (InkLineParser.TryParseDone(trimmed))
                {
                    currentSection.entries.Add(new DialogueStoryEntry { type = DialogueStoryEntryType.Done });
                    continue;
                }

                if (InkLineParser.TryParseTunnelReturn(trimmed))
                {
                    currentSection.entries.Add(new DialogueStoryEntry { type = DialogueStoryEntryType.TunnelReturn });
                    continue;
                }

                if (InkLineParser.TryParseTunnelDivertTarget(trimmed, out var tunnelTarget))
                {
                    currentSection.entries.Add(new DialogueStoryEntry
                    {
                        type = DialogueStoryEntryType.TunnelDivert,
                        targetKnot = tunnelTarget
                    });
                    continue;
                }

                if (InkLineParser.TryParseDivertTarget(trimmed, out var target))
                {
                    currentSection.entries.Add(new DialogueStoryEntry
                    {
                        type = DialogueStoryEntryType.Divert,
                        targetKnot = target
                    });
                    continue;
                }

                if (InkChoiceParser.TryParseChoice(lines, ref i, out var choiceEntry))
                {
                    currentSection.entries.Add(choiceEntry);
                    continue;
                }

                if (InkLineParser.TryParseGather(trimmed, out var gatherEntry))
                {
                    currentSection.entries.Add(gatherEntry);
                    continue;
                }

                if (InkLineParser.LooksLikeRawInk(trimmed))
                {
                    currentSection.entries.Add(new DialogueStoryEntry
                    {
                        type = DialogueStoryEntryType.RawInk,
                        rawInk = rawLine.TrimEnd()
                    });
                    continue;
                }

                InkTagParser.ParseTextAndTags(trimmed, out var text, out var tags);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    currentSection.entries.Add(new DialogueStoryEntry
                    {
                        type = DialogueStoryEntryType.Line,
                        text = text,
                        tags = tags
                    });
                }
            }
        }

        private static DialogueStorySection CreateImportedSection(string sectionName, DialogueStoryDraft draft)
        {
            var section = new DialogueStorySection
            {
                knotName = sectionName,
                entries = new List<DialogueStoryEntry>()
            };
            draft.sections.Add(section);
            return section;
        }
    }
}

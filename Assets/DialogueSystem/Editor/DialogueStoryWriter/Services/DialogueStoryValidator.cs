using System.Collections.Generic;

namespace SAS.DialogueSystem.EditorTools
{
    internal static class DialogueStoryValidator
    {
        public static void EnsureDraftShape(DialogueStoryDraft draft)
        {
            if (draft == null)
                return;

            draft.includeFiles ??= new List<string>();
            draft.globalTags ??= new List<DialogueStoryCustomTag>();

            if (draft.sections == null)
                draft.sections = new List<DialogueStorySection>();

            if (draft.sections.Count == 0)
                draft.sections.Add(new DialogueStorySection { knotName = "start" });

            foreach (var section in draft.sections)
            {
                section.entries ??= new List<DialogueStoryEntry>();

                foreach (var entry in section.entries)
                    EnsureEntryShape(entry);
            }
        }

        private static void EnsureEntryShape(DialogueStoryEntry entry)
        {
            if (entry == null)
                return;

            if (entry.tags == null)
                entry.tags = new DialogueStoryTagSet();

            entry.tags.customTags ??= new List<DialogueStoryCustomTag>();
            entry.bodyEntries ??= new List<DialogueStoryEntry>();

            if (entry.type == DialogueStoryEntryType.Choice)
                InkChoiceParser.PopulateImportedChoiceLocalization(entry);

            foreach (var childEntry in entry.bodyEntries)
                EnsureEntryShape(childEntry);
        }

        public static DialogueStoryEntry CreateEntry(DialogueStoryEntryType type)
        {
            var entry = new DialogueStoryEntry
            {
                type = type,
                text = type == DialogueStoryEntryType.Line || type == DialogueStoryEntryType.ConditionalLine ? "New dialogue line." : string.Empty,
                expanded = true
            };

            if (type == DialogueStoryEntryType.Tag || type == DialogueStoryEntryType.Choice)
                entry.tags = InkTagParser.CreateEmptyTagSet();

            return entry;
        }

        public static DialogueStoryEntry CloneEntry(DialogueStoryEntry source)
        {
            var sourceTags = source.tags ?? new DialogueStoryTagSet();
            var clone = new DialogueStoryEntry
            {
                type = source.type,
                text = source.text,
                targetKnot = source.targetKnot,
                targetIsTunnel = source.targetIsTunnel,
                choiceDepth = source.choiceDepth,
                stickyChoice = source.stickyChoice,
                suppressChoiceText = source.suppressChoiceText,
                fallbackChoice = source.fallbackChoice,
                choiceConditionExpression = source.choiceConditionExpression,
                gatherDepth = source.gatherDepth,
                conditionExpression = source.conditionExpression,
                elseTargetKnot = source.elseTargetKnot,
                rawInk = source.rawInk,
                bodyEntries = new List<DialogueStoryEntry>(),
                expanded = source.expanded,
                tags = new DialogueStoryTagSet
                {
                    useSpeaker = sourceTags.useSpeaker,
                    speakerId = sourceTags.speakerId,
                    speakerName = sourceTags.speakerName,
                    portraitKey = sourceTags.portraitKey,
                    speakerAnimation = sourceTags.speakerAnimation,
                    useLocale = sourceTags.useLocale,
                    localeKey = sourceTags.localeKey,
                    useLayout = sourceTags.useLayout,
                    layoutAnimation = sourceTags.layoutAnimation,
                    useAudio = sourceTags.useAudio,
                    audioId = sourceTags.audioId,
                    customTags = new List<DialogueStoryCustomTag>()
                }
            };

            sourceTags.customTags ??= new List<DialogueStoryCustomTag>();
            foreach (var customTag in sourceTags.customTags)
            {
                if (customTag == null)
                    continue;

                clone.tags.customTags.Add(new DialogueStoryCustomTag
                {
                    key = customTag.key,
                    value = customTag.value
                });
            }

            source.bodyEntries ??= new List<DialogueStoryEntry>();
            foreach (var childEntry in source.bodyEntries)
            {
                if (childEntry != null)
                    clone.bodyEntries.Add(CloneEntry(childEntry));
            }

            return clone;
        }

        public static string[] GetSectionNames(DialogueStoryDraft draft)
        {
            var names = new List<string> { "<none>" };
            var currentKnot = string.Empty;

            if (draft?.sections == null)
                return names.ToArray();

            foreach (var section in draft.sections)
            {
                if (string.IsNullOrWhiteSpace(section.knotName))
                    continue;

                if (section.sectionType == DialogueStorySectionType.Function)
                {
                    currentKnot = string.Empty;
                    continue;
                }

                if (section.sectionType == DialogueStorySectionType.Knot)
                {
                    currentKnot = section.knotName;
                    names.Add(section.knotName);
                }
                else if (!string.IsNullOrWhiteSpace(currentKnot))
                {
                    names.Add($"{currentKnot}.{section.knotName}");
                }
                else
                {
                    names.Add(section.knotName);
                }
            }

            return names.ToArray();
        }

        public static List<string> GetSectionHierarchyErrors(DialogueStoryDraft draft)
        {
            var errors = new List<string>();
            var currentKnot = string.Empty;

            if (draft?.sections == null)
                return errors;

            for (int i = 0; i < draft.sections.Count; i++)
            {
                var section = draft.sections[i];
                if (section == null)
                    continue;

                if (section.sectionType == DialogueStorySectionType.Knot)
                {
                    currentKnot = section.knotName;
                    continue;
                }

                if (section.sectionType == DialogueStorySectionType.Function)
                {
                    currentKnot = string.Empty;
                    continue;
                }

                if (section.sectionType == DialogueStorySectionType.Stitch && string.IsNullOrWhiteSpace(currentKnot))
                {
                    var stitchName = string.IsNullOrWhiteSpace(section.knotName) ? "<unnamed>" : section.knotName;
                    errors.Add($"Stitch '{stitchName}' at section {i + 1} has no parent knot. Move it below a knot or change it to a knot.");
                }
            }

            return errors;
        }

        public static bool TryGetParentKnotName(DialogueStoryDraft draft, int sectionIndex, out string parentKnotName)
        {
            parentKnotName = string.Empty;
            if (draft?.sections == null || sectionIndex < 0 || sectionIndex >= draft.sections.Count)
                return false;

            for (int i = sectionIndex - 1; i >= 0; i--)
            {
                var section = draft.sections[i];
                if (section == null)
                    continue;

                if (section.sectionType == DialogueStorySectionType.Function)
                    return false;

                if (section.sectionType == DialogueStorySectionType.Knot)
                {
                    parentKnotName = section.knotName;
                    return !string.IsNullOrWhiteSpace(parentKnotName);
                }
            }

            return false;
        }

        public static string GetUniqueSectionName(DialogueStoryDraft draft, string prefix)
        {
            var index = 1;
            var candidate = prefix;
            var names = new HashSet<string>();

            if (draft?.sections != null)
            {
                foreach (var section in draft.sections)
                    names.Add(section.knotName);
            }

            while (names.Contains(candidate))
            {
                index++;
                candidate = $"{prefix}_{index}";
            }

            return candidate;
        }
    }
}

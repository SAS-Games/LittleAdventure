using System.Text;
using UnityEngine;

namespace SAS.DialogueSystem.EditorTools
{
    internal static class InkEntryWriter
    {
        public static string BuildSectionHeader(DialogueStorySection section)
        {
            if (section.sectionType == DialogueStorySectionType.Function)
                return $"=== function {InkSanitizer.CleanFunctionSignature(section.knotName)} ===";

            var sectionName = InkSanitizer.SanitizeInkIdentifier(section.knotName);
            return section.sectionType == DialogueStorySectionType.Stitch
                ? $"= {sectionName}"
                : $"=== {sectionName} ===";
        }

        public static void WriteEntry(StringBuilder builder, DialogueStoryEntry entry)
        {
            WriteEntry(builder, entry, 0, 1);
        }

        private static void WriteEntry(StringBuilder builder, DialogueStoryEntry entry, int indentLevel, int minimumChoiceDepth)
        {
            switch (entry.type)
            {
                case DialogueStoryEntryType.Line:
                    if (!string.IsNullOrWhiteSpace(entry.text))
                        AppendIndentedLine(builder, indentLevel, $"{InkSanitizer.CleanDialogueText(entry.text)}{InkTagWriter.BuildTagSuffix(entry.tags)}");
                    break;
                case DialogueStoryEntryType.Tag:
                    var tagLine = InkTagWriter.BuildStandaloneTagLine(entry.tags);
                    if (!string.IsNullOrWhiteSpace(tagLine))
                        AppendIndentedLine(builder, indentLevel, tagLine);
                    break;
                case DialogueStoryEntryType.ConditionalLine:
                    WriteConditionalLine(builder, entry, indentLevel);
                    break;
                case DialogueStoryEntryType.Choice:
                    WriteChoice(builder, entry, indentLevel, minimumChoiceDepth);
                    break;
                case DialogueStoryEntryType.Gather:
                    var gatherMarker = new string('-', Mathf.Max(1, entry.gatherDepth));
                    if (!string.IsNullOrWhiteSpace(entry.text))
                        AppendIndentedLine(builder, indentLevel, $"{gatherMarker} {InkSanitizer.CleanDialogueText(entry.text)}{InkTagWriter.BuildTagSuffix(entry.tags)}");
                    else
                        AppendIndentedLine(builder, indentLevel, gatherMarker);
                    break;
                case DialogueStoryEntryType.Divert:
                    if (!string.IsNullOrWhiteSpace(entry.targetKnot))
                        AppendIndentedLine(builder, indentLevel, $"-> {InkSanitizer.SanitizeDivertTarget(entry.targetKnot)}");
                    break;
                case DialogueStoryEntryType.TunnelDivert:
                    if (!string.IsNullOrWhiteSpace(entry.targetKnot))
                        AppendIndentedLine(builder, indentLevel, $"-> {InkSanitizer.SanitizeDivertTarget(entry.targetKnot)} ->");
                    break;
                case DialogueStoryEntryType.ConditionalDivert:
                    WriteConditionalDivert(builder, entry, indentLevel);
                    break;
                case DialogueStoryEntryType.ConditionalTunnelDivert:
                    WriteConditionalTunnelDivert(builder, entry, indentLevel);
                    break;
                case DialogueStoryEntryType.RawInk:
                    if (!string.IsNullOrWhiteSpace(entry.rawInk))
                        AppendIndentedLines(builder, indentLevel, entry.rawInk.TrimEnd());
                    break;
                case DialogueStoryEntryType.TunnelReturn:
                    AppendIndentedLine(builder, indentLevel, "->->");
                    break;
                case DialogueStoryEntryType.End:
                    AppendIndentedLine(builder, indentLevel, "-> END");
                    break;
                case DialogueStoryEntryType.Done:
                    AppendIndentedLine(builder, indentLevel, "-> DONE");
                    break;
            }
        }

        public static bool IsTerminalEntry(DialogueStoryEntry entry)
        {
            switch (entry.type)
            {
                case DialogueStoryEntryType.Choice:
                case DialogueStoryEntryType.Divert:
                case DialogueStoryEntryType.End:
                case DialogueStoryEntryType.Done:
                case DialogueStoryEntryType.TunnelReturn:
                    return true;
                case DialogueStoryEntryType.TunnelDivert:
                    return false;
                case DialogueStoryEntryType.ConditionalDivert:
                    return !string.IsNullOrWhiteSpace(entry.targetKnot) && !string.IsNullOrWhiteSpace(entry.elseTargetKnot);
                case DialogueStoryEntryType.ConditionalTunnelDivert:
                    return false;
                default:
                    return false;
            }
        }

        private static void WriteConditionalLine(StringBuilder builder, DialogueStoryEntry entry, int indentLevel)
        {
            if (string.IsNullOrWhiteSpace(entry.conditionExpression) || string.IsNullOrWhiteSpace(entry.text))
                return;

            AppendIndentedLine(builder, indentLevel, $"{{ {InkSanitizer.CleanConditionExpression(entry.conditionExpression)}:");
            AppendIndentedLine(builder, indentLevel + 1, $"{InkSanitizer.CleanDialogueText(entry.text)}{InkTagWriter.BuildTagSuffix(entry.tags)}");
            AppendIndentedLine(builder, indentLevel, "}");
        }

        private static void WriteConditionalTunnelDivert(StringBuilder builder, DialogueStoryEntry entry, int indentLevel)
        {
            if (string.IsNullOrWhiteSpace(entry.conditionExpression) || string.IsNullOrWhiteSpace(entry.targetKnot))
                return;

            AppendIndentedLine(builder, indentLevel, $"{{ {InkSanitizer.CleanConditionExpression(entry.conditionExpression)}:");
            AppendIndentedLine(builder, indentLevel + 1, $"-> {InkSanitizer.SanitizeDivertTarget(entry.targetKnot)} ->");

            if (!string.IsNullOrWhiteSpace(entry.elseTargetKnot))
            {
                AppendIndentedLine(builder, indentLevel, "- else:");
                AppendIndentedLine(builder, indentLevel + 1, $"-> {InkSanitizer.SanitizeDivertTarget(entry.elseTargetKnot)} ->");
            }

            AppendIndentedLine(builder, indentLevel, "}");
        }

        private static void WriteConditionalDivert(StringBuilder builder, DialogueStoryEntry entry, int indentLevel)
        {
            if (string.IsNullOrWhiteSpace(entry.conditionExpression) || string.IsNullOrWhiteSpace(entry.targetKnot))
                return;

            AppendIndentedLine(builder, indentLevel, $"{{ {InkSanitizer.CleanConditionExpression(entry.conditionExpression)}:");
            AppendIndentedLine(builder, indentLevel + 1, $"-> {InkSanitizer.SanitizeDivertTarget(entry.targetKnot)}");

            if (!string.IsNullOrWhiteSpace(entry.elseTargetKnot))
            {
                AppendIndentedLine(builder, indentLevel, "- else:");
                AppendIndentedLine(builder, indentLevel + 1, $"-> {InkSanitizer.SanitizeDivertTarget(entry.elseTargetKnot)}");
            }

            AppendIndentedLine(builder, indentLevel, "}");
        }

        private static void WriteChoice(StringBuilder builder, DialogueStoryEntry entry, int indentLevel, int minimumChoiceDepth)
        {
            InkChoiceParser.NormalizeChoiceText(entry);

            var choiceDepth = Mathf.Max(minimumChoiceDepth, entry.choiceDepth);
            var bullet = new string(entry.stickyChoice ? '+' : '*', choiceDepth);
            var condition = string.IsNullOrWhiteSpace(entry.choiceConditionExpression)
                ? string.Empty
                : $" {{{InkSanitizer.CleanConditionExpression(entry.choiceConditionExpression)}}}";
            var hasBody = entry.bodyEntries != null && entry.bodyEntries.Count > 0;

            if (entry.fallbackChoice && string.IsNullOrWhiteSpace(entry.text))
            {
                if (!string.IsNullOrWhiteSpace(entry.targetKnot))
                {
                    var target = InkSanitizer.SanitizeDivertTarget(entry.targetKnot);
                    AppendIndentedLine(builder, indentLevel, entry.targetIsTunnel
                        ? $"{bullet}{condition} -> {target} ->"
                        : $"{bullet}{condition} -> {target}");
                }

                return;
            }

            if (string.IsNullOrWhiteSpace(entry.text))
                return;

            var choiceLine = $"{bullet}{condition} {BuildChoiceText(entry)}";
            if (!entry.suppressChoiceText)
                choiceLine += InkTagWriter.BuildTagSuffix(entry.tags);

            if (!hasBody && !string.IsNullOrWhiteSpace(entry.targetKnot))
            {
                choiceLine += entry.targetIsTunnel
                    ? $" -> {InkSanitizer.SanitizeDivertTarget(entry.targetKnot)} ->"
                    : $" -> {InkSanitizer.SanitizeDivertTarget(entry.targetKnot)}";
            }

            AppendIndentedLine(builder, indentLevel, choiceLine);

            if (!hasBody)
                return;

            foreach (var childEntry in entry.bodyEntries)
                WriteEntry(builder, childEntry, indentLevel + 1, choiceDepth + 1);

            if (string.IsNullOrWhiteSpace(entry.targetKnot))
                return;

            AppendIndentedLine(builder, indentLevel + 1, entry.targetIsTunnel
                ? $"-> {InkSanitizer.SanitizeDivertTarget(entry.targetKnot)} ->"
                : $"-> {InkSanitizer.SanitizeDivertTarget(entry.targetKnot)}");
        }

        private static string BuildChoiceText(DialogueStoryEntry entry)
        {
            var text = InkSanitizer.CleanDialogueText(entry.text);
            return entry.suppressChoiceText ? $"[{text}{InkTagWriter.BuildTagSuffix(entry.tags)}]" : text;
        }

        private static void AppendIndentedLine(StringBuilder builder, int indentLevel, string line)
        {
            if (indentLevel > 0)
                builder.Append(new string(' ', indentLevel * 4));

            builder.AppendLine(line);
        }

        private static void AppendIndentedLines(StringBuilder builder, int indentLevel, string text)
        {
            var lines = InkSanitizer.NormalizeLineEndings(text).Split('\n');
            foreach (var line in lines)
                AppendIndentedLine(builder, indentLevel, line.TrimEnd('\r'));
        }
    }
}

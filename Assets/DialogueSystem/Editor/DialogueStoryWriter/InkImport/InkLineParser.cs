using System;

namespace SAS.DialogueSystem.EditorTools
{
    internal static class InkLineParser
    {
        public static bool TryParseInclude(string line, out string includePath)
        {
            includePath = string.Empty;
            const string includePrefix = "INCLUDE ";
            if (!line.StartsWith(includePrefix, StringComparison.OrdinalIgnoreCase))
                return false;

            includePath = InkSanitizer.StripInlineComment(line.Substring(includePrefix.Length).Trim());
            return !string.IsNullOrWhiteSpace(includePath);
        }

        public static bool TryParseSectionHeader(string line, out string sectionName, out DialogueStorySectionType sectionType)
        {
            sectionName = string.Empty;
            sectionType = DialogueStorySectionType.Knot;

            line = InkSanitizer.NormalizeImportedLine(line);
            if (string.IsNullOrWhiteSpace(line) || line[0] != '=')
                return false;

            var separatorCount = 0;
            while (separatorCount < line.Length && line[separatorCount] == '=')
                separatorCount++;

            if (separatorCount == line.Length)
                return false;

            sectionType = separatorCount == 1 ? DialogueStorySectionType.Stitch : DialogueStorySectionType.Knot;
            sectionName = line.Substring(separatorCount).Trim();
            sectionName = InkSanitizer.StripInlineComment(sectionName);

            var tagIndex = sectionName.IndexOf('#');
            if (tagIndex >= 0)
                sectionName = sectionName.Substring(0, tagIndex).Trim();

            sectionName = sectionName.TrimEnd('=').Trim();
            if (sectionName.StartsWith("function ", StringComparison.OrdinalIgnoreCase))
            {
                sectionType = DialogueStorySectionType.Function;
                sectionName = InkSanitizer.CleanFunctionSignature(sectionName);
            }

            return !string.IsNullOrWhiteSpace(sectionName);
        }

        public static bool TryParseGather(string line, out DialogueStoryEntry entry)
        {
            entry = null;
            line = InkSanitizer.NormalizeImportedLine(line);

            if (!line.StartsWith("-", StringComparison.Ordinal) || line.StartsWith("- else", StringComparison.OrdinalIgnoreCase))
                return false;

            var depth = InkSanitizer.CountLeadingMarkers(line, '-');
            InkTagParser.ParseTextAndTags(line.Substring(depth).Trim(), out var text, out var tags);
            entry = new DialogueStoryEntry
            {
                type = DialogueStoryEntryType.Gather,
                text = text,
                gatherDepth = depth,
                tags = tags
            };
            return true;
        }

        public static bool TryParseStandaloneTags(string line, out DialogueStoryEntry entry)
        {
            entry = null;
            line = InkSanitizer.NormalizeImportedLine(line);
            if (!line.StartsWith("#", StringComparison.Ordinal))
                return false;

            InkTagParser.ParseTextAndTags(line, out var text, out var tags);
            if (!string.IsNullOrWhiteSpace(text) || !InkTagWriter.HasAnyTag(tags))
                return false;

            entry = new DialogueStoryEntry
            {
                type = DialogueStoryEntryType.Tag,
                tags = tags
            };
            return true;
        }

        public static bool TryParseEnd(string line)
        {
            return line.Equals("END", StringComparison.OrdinalIgnoreCase) ||
                   line.Equals("-> END", StringComparison.OrdinalIgnoreCase) ||
                   line.Equals("->END", StringComparison.OrdinalIgnoreCase);
        }

        public static bool TryParseDone(string line)
        {
            return line.Equals("DONE", StringComparison.OrdinalIgnoreCase) ||
                   line.Equals("-> DONE", StringComparison.OrdinalIgnoreCase) ||
                   line.Equals("->DONE", StringComparison.OrdinalIgnoreCase);
        }

        public static bool TryParseDivertTarget(string line, out string target)
        {
            target = string.Empty;
            line = line.Trim();
            if (!line.StartsWith("->", StringComparison.Ordinal) ||
                TryParseTunnelReturn(line) ||
                TryParseTunnelDivertTarget(line, out _))
                return false;

            return TryExtractDivertTarget(line, out target);
        }

        public static bool TryParseTunnelReturn(string line)
        {
            return line.Trim().Equals("->->", StringComparison.Ordinal);
        }

        public static bool TryParseTunnelDivertTarget(string line, out string target)
        {
            target = string.Empty;
            line = line.Trim();

            if (!line.StartsWith("->", StringComparison.Ordinal) ||
                line.Equals("->->", StringComparison.Ordinal) ||
                !line.EndsWith("->", StringComparison.Ordinal))
            {
                return false;
            }

            var inner = line.Substring(2, line.Length - 4).Trim();
            if (string.IsNullOrWhiteSpace(inner) || inner.Contains("->"))
                return false;

            target = InkSanitizer.CleanChoiceImportToken(inner);
            return !string.IsNullOrWhiteSpace(target);
        }

        public static bool TryExtractDivertTarget(string text, out string target)
        {
            target = string.Empty;
            if (TryExtractTunnelDivertTarget(text, out _))
                return false;

            var divertIndex = text.IndexOf("->", StringComparison.Ordinal);
            if (divertIndex < 0)
                return false;

            target = text.Substring(divertIndex + 2)
                .Replace("}", string.Empty)
                .Trim();

            var separatorIndex = target.IndexOfAny(new[] { ' ', '\t', '\r', '\n', '|' });
            if (separatorIndex >= 0)
                target = target.Substring(0, separatorIndex).Trim();

            target = InkSanitizer.CleanChoiceImportToken(target);
            return !string.IsNullOrWhiteSpace(target);
        }

        public static bool TryExtractTunnelDivertTarget(string text, out string target)
        {
            target = string.Empty;
            var divertIndex = text.IndexOf("->", StringComparison.Ordinal);
            if (divertIndex < 0)
                return false;

            var targetStart = divertIndex + 2;
            var returnIndex = text.IndexOf("->", targetStart, StringComparison.Ordinal);
            if (returnIndex < 0)
                return false;

            target = text.Substring(targetStart, returnIndex - targetStart).Trim();
            var separatorIndex = target.IndexOfAny(new[] { ' ', '\t', '\r', '\n', '|' });
            if (separatorIndex >= 0)
                target = target.Substring(0, separatorIndex).Trim();

            target = InkSanitizer.CleanChoiceImportToken(target);
            return !string.IsNullOrWhiteSpace(target);
        }

        public static bool HasTextBeforeFirstDivert(string text)
        {
            var divertIndex = text.IndexOf("->", StringComparison.Ordinal);
            return divertIndex > 0 && !string.IsNullOrWhiteSpace(text.Substring(0, divertIndex));
        }

        public static int FindNextMeaningfulLine(string[] lines, int startIndex)
        {
            for (int i = startIndex; i < lines.Length; i++)
            {
                var trimmed = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("//", StringComparison.Ordinal))
                    continue;

                return i;
            }

            return -1;
        }

        public static int CountLeadingWhitespace(string line)
        {
            if (string.IsNullOrEmpty(line))
                return 0;

            var count = 0;
            foreach (var character in line)
            {
                if (character == ' ')
                    count++;
                else if (character == '\t')
                    count += 4;
                else
                    break;
            }

            return count;
        }

        public static bool LooksLikeRawInk(string line)
        {
            return line.StartsWith("{", StringComparison.Ordinal) ||
                   line.StartsWith("->->", StringComparison.Ordinal) ||
                   line.StartsWith("-", StringComparison.Ordinal) ||
                   line.StartsWith("=", StringComparison.Ordinal) ||
                   line.StartsWith("~", StringComparison.Ordinal) ||
                   line.StartsWith("+", StringComparison.Ordinal) ||
                   line.StartsWith("[", StringComparison.Ordinal) ||
                   line.StartsWith("VAR ", StringComparison.OrdinalIgnoreCase) ||
                   line.StartsWith("LIST ", StringComparison.OrdinalIgnoreCase) ||
                   line.StartsWith("CONST ", StringComparison.OrdinalIgnoreCase);
        }
    }
}

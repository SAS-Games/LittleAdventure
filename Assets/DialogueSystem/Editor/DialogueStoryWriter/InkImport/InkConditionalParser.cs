using System;
using System.Text;

namespace SAS.DialogueSystem.EditorTools
{
    internal static class InkConditionalParser
    {
        public static bool TryReadConditionalDivert(string[] lines, ref int index, out DialogueStoryEntry entry)
        {
            entry = null;
            var firstLine = lines[index].Trim();
            if (!firstLine.StartsWith("{", StringComparison.Ordinal))
                return false;

            var block = new StringBuilder();
            var braceDepth = 0;
            var foundClose = false;
            var endIndex = index;

            for (int i = index; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                block.AppendLine(line);
                braceDepth += InkSanitizer.CountChar(line, '{');
                braceDepth -= InkSanitizer.CountChar(line, '}');

                if (line.Contains("}") && braceDepth <= 0)
                {
                    foundClose = true;
                    endIndex = i;
                    break;
                }
            }

            if (!foundClose)
                return false;

            var blockText = block.ToString().Trim();
            if (!TryParseConditionalDivertBlock(blockText, out entry) &&
                !TryParseConditionalLineBlock(blockText, out entry))
            {
                entry = new DialogueStoryEntry
                {
                    type = DialogueStoryEntryType.RawInk,
                    rawInk = blockText
                };
            }

            index = endIndex;
            return true;
        }

        private static bool TryParseConditionalDivertBlock(string blockText, out DialogueStoryEntry entry)
        {
            entry = null;
            var content = blockText.Trim();

            if (content.StartsWith("{", StringComparison.Ordinal))
                content = content.Substring(1);

            if (content.EndsWith("}", StringComparison.Ordinal))
                content = content.Substring(0, content.Length - 1);

            content = content.Trim();
            var colonIndex = content.IndexOf(':');
            if (colonIndex < 0)
                return false;

            var condition = content.Substring(0, colonIndex).Trim().TrimStart('-').Trim();
            var afterCondition = content.Substring(colonIndex + 1).Trim();
            if (string.IsNullOrWhiteSpace(condition))
                return false;

            string trueTarget = string.Empty;
            string elseTarget = string.Empty;
            var trueTargetIsTunnel = false;
            var elseTargetIsTunnel = false;

            if (afterCondition.Contains("|"))
            {
                var branches = afterCondition.Split(new[] { '|' }, 2);
                if (!TryReadBranchTarget(branches[0], out trueTarget, out trueTargetIsTunnel))
                    return false;

                if (branches.Length > 1)
                {
                    if (!TryReadBranchTarget(branches[1], out elseTarget, out elseTargetIsTunnel))
                        return false;
                }
            }
            else
            {
                var lines = InkSanitizer.NormalizeLineEndings(afterCondition).Split('\n');
                var inElse = false;

                foreach (var rawLine in lines)
                {
                    var line = rawLine.Trim();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    if (line.StartsWith("- else", StringComparison.OrdinalIgnoreCase))
                    {
                        inElse = true;
                        var elseColon = line.IndexOf(':');
                        if (elseColon >= 0)
                        {
                            if (!TryReadBranchTarget(line.Substring(elseColon + 1), out elseTarget, out elseTargetIsTunnel))
                                return false;
                        }

                        continue;
                    }

                    if (TryReadBranchTarget(line, out var target, out var targetIsTunnel))
                    {
                        if (inElse)
                        {
                            elseTarget = target;
                            elseTargetIsTunnel = targetIsTunnel;
                        }
                        else
                        {
                            trueTarget = target;
                            trueTargetIsTunnel = targetIsTunnel;
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(trueTarget))
                return false;

            if (!string.IsNullOrWhiteSpace(elseTarget) && trueTargetIsTunnel != elseTargetIsTunnel)
                return false;

            entry = new DialogueStoryEntry
            {
                type = trueTargetIsTunnel ? DialogueStoryEntryType.ConditionalTunnelDivert : DialogueStoryEntryType.ConditionalDivert,
                conditionExpression = condition,
                targetKnot = trueTarget,
                elseTargetKnot = elseTarget
            };
            return true;
        }

        private static bool TryReadBranchTarget(string text, out string target, out bool targetIsTunnel)
        {
            target = string.Empty;
            targetIsTunnel = false;
            text = text.Trim();

            if (string.IsNullOrWhiteSpace(text))
                return true;

            if (InkLineParser.HasTextBeforeFirstDivert(text))
                return false;

            if (InkLineParser.TryExtractTunnelDivertTarget(text, out target))
            {
                targetIsTunnel = true;
                return true;
            }

            return InkLineParser.TryExtractDivertTarget(text, out target);
        }

        private static bool TryParseConditionalLineBlock(string blockText, out DialogueStoryEntry entry)
        {
            entry = null;
            var content = blockText.Trim();

            if (content.StartsWith("{", StringComparison.Ordinal))
                content = content.Substring(1);

            if (content.EndsWith("}", StringComparison.Ordinal))
                content = content.Substring(0, content.Length - 1);

            content = content.Trim();
            var colonIndex = content.IndexOf(':');
            if (colonIndex < 0)
                return false;

            var condition = content.Substring(0, colonIndex).Trim().TrimStart('-').Trim();
            var lineText = content.Substring(colonIndex + 1).Trim();

            if (string.IsNullOrWhiteSpace(condition) ||
                string.IsNullOrWhiteSpace(lineText) ||
                lineText.Contains("->") ||
                lineText.Contains("- else") ||
                lineText.StartsWith("- else", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            InkTagParser.ParseTextAndTags(lineText, out var text, out var tags);
            if (string.IsNullOrWhiteSpace(text))
                return false;

            entry = new DialogueStoryEntry
            {
                type = DialogueStoryEntryType.ConditionalLine,
                conditionExpression = condition,
                text = text,
                tags = tags
            };
            return true;
        }
    }
}

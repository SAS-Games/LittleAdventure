using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace SAS.DialogueSystem.EditorTools
{
    internal static class InkSanitizer
    {
        public static string CleanDialogueText(string text)
        {
            return (text ?? string.Empty).Replace("\r", string.Empty).Replace("\n", " ").Trim();
        }

        public static string CleanPreviewText(string text, int maxLength)
        {
            text = CleanDialogueText(text);
            if (text.Length <= maxLength)
                return text;

            return text.Substring(0, Mathf.Max(0, maxLength - 3)).TrimEnd() + "...";
        }

        public static string CleanChoiceImportToken(string value)
        {
            value = CleanTagValue(value);
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().Trim('[', ']', '(', ')').Trim();
        }

        public static string NormalizeLineEndings(string value)
        {
            return (value ?? string.Empty).Replace("\r\n", "\n").Replace("\r", "\n");
        }

        public static string NormalizeImportedLine(string value)
        {
            return (value ?? string.Empty)
                .Replace("\uFEFF", string.Empty)
                .Replace("\u200B", string.Empty)
                .Trim();
        }

        public static string StripInlineComment(string value)
        {
            var commentIndex = value.IndexOf("//", StringComparison.Ordinal);
            return commentIndex < 0 ? value.Trim() : value.Substring(0, commentIndex).Trim();
        }

        public static int CountChar(string value, char target)
        {
            var count = 0;
            foreach (var character in value)
            {
                if (character == target)
                    count++;
            }

            return count;
        }

        public static int CountLeadingMarkers(string value, char marker)
        {
            var count = 0;
            while (count < value.Length && value[count] == marker)
                count++;

            return Mathf.Max(1, count);
        }

        public static string CleanSingleLine(string value)
        {
            return (value ?? string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty).Trim();
        }

        public static string CleanTagKey(string value)
        {
            return CleanTagValue(value).Replace(" ", string.Empty);
        }

        public static string ToSnakeCaseKey(string value)
        {
            value = CleanTagValue(value);
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var builder = new StringBuilder(value.Length);
            var wroteSeparator = false;

            foreach (var character in value)
            {
                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(char.ToLowerInvariant(character));
                    wroteSeparator = false;
                }
                else if (character == '_' || character == '-' || char.IsWhiteSpace(character) || character == '.')
                {
                    if (builder.Length > 0 && !wroteSeparator)
                    {
                        builder.Append('_');
                        wroteSeparator = true;
                    }
                }
            }

            while (builder.Length > 0 && builder[builder.Length - 1] == '_')
                builder.Length--;

            return builder.ToString();
        }

        public static string CleanTagValue(string value)
        {
            return (value ?? string.Empty)
                .Replace("\r", string.Empty)
                .Replace("\n", " ")
                .Replace("#", string.Empty)
                .Trim();
        }

        public static string CleanConditionExpression(string value)
        {
            return (value ?? string.Empty)
                .Replace("\r", string.Empty)
                .Replace("\n", " ")
                .Trim();
        }

        public static string CleanFunctionSignature(string value)
        {
            value = CleanSingleLine(value);
            if (string.IsNullOrWhiteSpace(value))
                return "FunctionName()";

            if (value.StartsWith("function ", StringComparison.OrdinalIgnoreCase))
                value = value.Substring("function ".Length).Trim();

            return value;
        }

        public static string SanitizeFileName(string value)
        {
            value = string.IsNullOrWhiteSpace(value) ? "NewDialogue" : value.Trim();
            foreach (var invalid in Path.GetInvalidFileNameChars())
                value = value.Replace(invalid.ToString(), string.Empty);
            return string.IsNullOrWhiteSpace(value) ? "NewDialogue" : value;
        }

        public static string SanitizeDivertTarget(string value)
        {
            value = string.IsNullOrWhiteSpace(value) ? "start" : value.Trim();
            if (value.Equals("END", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("DONE", StringComparison.OrdinalIgnoreCase))
            {
                return value.ToUpperInvariant();
            }

            var parts = value.Split('.');
            for (int i = 0; i < parts.Length; i++)
                parts[i] = SanitizeInkIdentifier(parts[i]);

            return string.Join(".", parts);
        }

        public static string SanitizeInkIdentifier(string value)
        {
            value = string.IsNullOrWhiteSpace(value) ? "start" : value.Trim();
            var builder = new StringBuilder(value.Length);

            foreach (var character in value)
            {
                if (char.IsLetterOrDigit(character) || character == '_')
                    builder.Append(character);
                else if (char.IsWhiteSpace(character) || character == '-')
                    builder.Append('_');
            }

            if (builder.Length == 0)
                builder.Append("start");

            if (char.IsDigit(builder[0]))
                builder.Insert(0, '_');

            return builder.ToString();
        }
    }
}

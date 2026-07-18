// #if UNITY_EDITOR
// using System;
// using System.Text;
// using System.Text.RegularExpressions;
// using System.Threading.Tasks;
// using OpenAI.Chat; // from official OpenAI .NET SDK
//
// namespace SAS.Localization.AI
// {
//     public class OpenAITranslationService
//     {
//         private readonly ChatClient _client;
//         private readonly string _model;
//         private readonly string _sourceLanguage;
//         private readonly string _targetLanguage;
//
//         // Simple regex to strip XML/RTF tags if you want to mimic video
//         private static readonly Regex s_tagRegex = new("<.*?>", RegexOptions.Singleline);
//
//         public OpenAITranslationService(
//             string apiKey,
//             string model,
//             string sourceLanguage,
//             string targetLanguage)
//         {
//             _client = new ChatClient(model: model, apiKey: apiKey);
//             _model = model;
//             _sourceLanguage = sourceLanguage;
//             _targetLanguage = targetLanguage;
//         }
//
//         public async Task<string> TranslateAsync(
//             string originalText,
//             string feelingContext,
//             string characterContext,
//             bool stripTags = true)
//         {
//             string cleanText = stripTags ? s_tagRegex.Replace(originalText, string.Empty) : originalText;
//
//             var systemMessage = new SystemChatMessage(
//                 "You are a professional game localization translator. " +
//                 "Your job is to translate game dialogue from " + _sourceLanguage +
//                 " to natural, fluent " + _targetLanguage +
//                 ". Preserve meaning, tone, and style. " +
//                 "Do NOT add explanations. Output ONLY the translated line.");
//
//             var userPrompt = BuildUserPrompt(cleanText, feelingContext, characterContext);
//
//             var userMessage = new UserChatMessage(userPrompt);
//
//             ChatCompletion completion = await _client.CompleteChatAsync(new ChatMessage[] { systemMessage, userMessage });
//
//             string result = completion.Content[0].Text?.Trim();
//             return result;
//         }
//
//         private string BuildUserPrompt(string text, string feeling, string character)
//         {
//             var sb = new StringBuilder();
//
//             sb.AppendLine("Translate the following game dialogue.");
//             sb.AppendLine();
//
//             if (!string.IsNullOrWhiteSpace(feeling))
//             {
//                 sb.AppendLine("Tone / emotion: " + feeling);
//             }
//
//             if (!string.IsNullOrWhiteSpace(character))
//             {
//                 sb.AppendLine("Character information: " + character);
//             }
//
//             sb.AppendLine();
//             sb.AppendLine("TEXT TO TRANSLATE (in " + _sourceLanguage + "):");
//             sb.AppendLine("-----");
//             sb.AppendLine(text);
//             sb.AppendLine("-----");
//             sb.AppendLine();
//             sb.AppendLine("Remember:");
//             sb.AppendLine("- Translate only the text between the dashes.");
//             sb.AppendLine("- Preserve style, personality, and emotional tone.");
//             sb.AppendLine("- Do NOT translate names or tags (if any).");
//             sb.AppendLine("- Output ONLY the translated text, nothing else.");
//
//             return sb.ToString();
//         }
//     }
// }
// #endif

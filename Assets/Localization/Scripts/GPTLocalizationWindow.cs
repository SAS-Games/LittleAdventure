#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEditor.Localization;
using SAS.Localization.AI;
using System.Linq;
using System.Globalization;

public class GPTLocalizationWindow : EditorWindow
{
    [Header("OpenAI")]
    private string _apiKey;
    private string _model = "gpt-4o-mini";

    [Header("Localization")]
    private Locale _sourceLocale;
    private Locale _targetLocale;
    private StringTableCollection _collection;

    [Header("Filters")]
    private string _keyRegex = ".*";
    private bool _onlyMissingInTarget = true;

    [Header("Rate limiting")]
    private int _requestsPerMinute = 20;
    private bool _mock = false;

    private List<TranslatableEntry> _entries = new();
    private Vector2 _scroll;
    private bool _isTranslating;

    [MenuItem("Tools/AI Localization/ChatGPT Translator")]
    public static void ShowWindow()
    {
        var wnd = GetWindow<GPTLocalizationWindow>("GPT Translator");
        wnd.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("GPT-Powered Localization", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        DrawOpenAISettings();
        EditorGUILayout.Space();
        DrawLocalizationSettings();
        EditorGUILayout.Space();
        DrawFilters();
        EditorGUILayout.Space();
        DrawButtons();
        EditorGUILayout.Space();
        DrawEntriesList();
    }

    private void DrawOpenAISettings()
    {
        EditorGUILayout.LabelField("OpenAI Settings", EditorStyles.boldLabel);
        _apiKey = EditorGUILayout.PasswordField("API Key", _apiKey);
        _model = EditorGUILayout.TextField("Model", _model);
        _requestsPerMinute = EditorGUILayout.IntField("Requests / Minute", _requestsPerMinute);
        _mock = EditorGUILayout.Toggle("Mock (no API calls)", _mock);
    }

    private void DrawLocalizationSettings()
    {
        EditorGUILayout.LabelField("Localization Settings", EditorStyles.boldLabel);
        _collection = (StringTableCollection)EditorGUILayout.ObjectField("Collection", _collection, typeof(StringTableCollection), false);
        _sourceLocale = (Locale)EditorGUILayout.ObjectField("Source Locale", _sourceLocale, typeof(Locale), false);
        _targetLocale = (Locale)EditorGUILayout.ObjectField("Target Locale", _targetLocale, typeof(Locale), false);
    }

    private void DrawFilters()
    {
        EditorGUILayout.LabelField("Filters", EditorStyles.boldLabel);
        _keyRegex = EditorGUILayout.TextField("Key regex", _keyRegex);
        _onlyMissingInTarget = EditorGUILayout.Toggle("Only missing in target", _onlyMissingInTarget);
    }

    private void DrawButtons()
    {
        using (new EditorGUI.DisabledScope(_collection == null || _sourceLocale == null || _targetLocale == null))
        {
            if (GUILayout.Button("Fetch Entries"))
                FetchEntries();
        }

        using (new EditorGUI.DisabledScope(_entries.Count == 0 || _isTranslating))
        {
            if (GUILayout.Button("Translate All"))
                TranslateAll();
        }
    }

    private void DrawEntriesList()
    {
        EditorGUILayout.LabelField($"Entries to translate: {_entries.Count}", EditorStyles.boldLabel);

        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        for (int i = 0; i < _entries.Count; i++)
        {
            var e = _entries[i];
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(e.KeyId, EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Source:", e.SourceText, EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("Feeling:", string.IsNullOrEmpty(e.Feeling) ? "-" : e.Feeling);
            EditorGUILayout.LabelField("Character:", string.IsNullOrEmpty(e.CharacterContext) ? "-" : e.CharacterContext);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Remove"))
            {
                _entries.RemoveAt(i);
                i--;
            }
            if (GUILayout.Button("Quick Translate") && !_isTranslating)
                QuickTranslate(e);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndScrollView();
    }

    private void FetchEntries()
    {
        _entries.Clear();

        if (_collection == null)
        {
            Debug.LogError("No StringTableCollection selected.");
            return;
        }

        var shared = _collection.SharedData;
        if (shared == null)
        {
            Debug.LogError("Collection has no shared data.");
            return;
        }

        StringTable sourceTable = _collection.GetTable(_sourceLocale.Identifier) as StringTable;
        StringTable targetTable = _collection.GetTable(_targetLocale.Identifier) as StringTable;

        if (sourceTable == null)
        {
            Debug.LogError("Source table not found for locale: " + _sourceLocale);
            return;
        }

        if (targetTable == null)
        {
            Debug.LogError("Target table not found for locale: " + _targetLocale);
            return;
        }

        Regex regex = new Regex(_keyRegex);

        foreach (var sharedEntry in shared.Entries)
        {
            string key = sharedEntry.Key;
            if (!regex.IsMatch(key))
                continue;

            var sourceEntry = sourceTable.GetEntry(sharedEntry.Id);
            if (sourceEntry == null || string.IsNullOrEmpty(sourceEntry.Value))
                continue;

            var targetEntry = targetTable.GetEntry(sharedEntry.Id);
            if (_onlyMissingInTarget && targetEntry != null && !string.IsNullOrEmpty(targetEntry.Value))
                continue;

            string feeling = "";
            string characterCtx = "";

            var feelingMeta = sourceEntry.MetadataEntries?.FirstOrDefault(m => m is FeelingMetadata) as FeelingMetadata;
            if (feelingMeta != null)
                feeling = feelingMeta.feeling;

            var characterMeta = sourceEntry.MetadataEntries?.FirstOrDefault(m => m is CharacterMetadata) as CharacterMetadata;
            if (characterMeta != null && characterMeta.Name != null)
                characterCtx = $"{characterMeta.Name}: {characterMeta.Description}";

            _entries.Add(new TranslatableEntry
            {
                KeyId = key,
                SharedId = sharedEntry.Id,
                SourceText = sourceEntry.Value,
                Feeling = feeling,
                CharacterContext = characterCtx
            });
        }

        Debug.Log($"Fetched {_entries.Count} entries to translate.");
    }

    private async void TranslateAll()
    {
        if (_entries.Count == 0)
            return;

        if (string.IsNullOrEmpty(_apiKey) && !_mock)
        {
            Debug.LogError("OpenAI API key is empty.");
            return;
        }

        _isTranslating = true;

        try
        {
            var sourceLanguageName = GetLanguageName(_sourceLocale);
            var targetLanguageName = GetLanguageName(_targetLocale);
            var service = new OpenAITranslationService(
                _apiKey,
                _model,
                sourceLanguageName,
                targetLanguageName);

            float delayMs = _requestsPerMinute > 0 ? (60_000f / _requestsPerMinute) : 0f;

            StringTable targetTable = _collection.GetTable(_targetLocale.Identifier) as StringTable;

            for (int i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];
                string translated = entry.SourceText;

                if (!_mock)
                {
                    try
                    {
                        translated = await service.TranslateAsync(
                            entry.SourceText,
                            entry.Feeling,
                            entry.CharacterContext,
                            stripTags: true);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error translating '{entry.KeyId}': {ex.Message}");
                        continue;
                    }

                    if (delayMs > 0)
                        await Task.Delay(TimeSpan.FromMilliseconds(delayMs));
                }
                else
                {
                    translated = "[MOCK] " + entry.SourceText;
                }

                var targetEntry = targetTable.GetEntry(entry.SharedId);
                if (targetEntry == null)
                    targetEntry = targetTable.AddEntry(entry.SharedId, translated);
                else
                    targetEntry.Value = translated;

                EditorUtility.SetDirty(targetTable);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Translation finished.");
        }
        finally
        {
            _isTranslating = false;
        }
    }

    private async void QuickTranslate(TranslatableEntry entry)
    {
        if (string.IsNullOrEmpty(_apiKey) && !_mock)
        {
            Debug.LogError("OpenAI API key is empty.");
            return;
        }

        var sourceLanguageName = GetLanguageName(_sourceLocale);
        var targetLanguageName = GetLanguageName(_targetLocale);
        var service = new OpenAITranslationService(_apiKey, _model, sourceLanguageName, targetLanguageName);

        StringTable targetTable = _collection.GetTable(_targetLocale.Identifier) as StringTable;

        string translated = entry.SourceText;
        if (!_mock)
        {
            try
            {
                translated = await service.TranslateAsync(
                    entry.SourceText,
                    entry.Feeling,
                    entry.CharacterContext,
                    stripTags: true);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error quick-translating '{entry.KeyId}': {ex.Message}");
                return;
            }
        }
        else
            translated = "[MOCK] " + entry.SourceText;

        var targetEntry = targetTable.GetEntry(entry.SharedId);
        if (targetEntry == null)
            targetEntry = targetTable.AddEntry(entry.SharedId, translated);
        else
            targetEntry.Value = translated;

        EditorUtility.SetDirty(targetTable);
        AssetDatabase.SaveAssets();

        Debug.Log($"Quick translated {entry.KeyId}");
    }

    [Serializable]
    private class TranslatableEntry
    {
        public string KeyId;
        public long SharedId;
        public string SourceText;
        public string Feeling;
        public string CharacterContext;
    }

    private static string GetLanguageName(Locale locale)
    {
        if (locale?.Identifier.CultureInfo != null)
        {
            CultureInfo ci = locale.Identifier.CultureInfo;
            return ci.EnglishName; // e.g. "French (France)"
        }

        if (!string.IsNullOrEmpty(locale?.LocaleName))
            return locale.LocaleName;

        return "Unknown language";
    }
}
#endif

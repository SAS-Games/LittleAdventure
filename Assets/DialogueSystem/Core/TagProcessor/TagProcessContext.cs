using System;
using System.Collections.Generic;
using System.Linq;

public sealed class DialogueLineContext
{
    private readonly Dictionary<string, SpeakerState> _speakers = new();
    private readonly List<string> _rawTags;
    private readonly Dictionary<string, List<string>> _tags = new(StringComparer.OrdinalIgnoreCase);

    public DialogueLineContext(string text, IEnumerable<string> rawTags)
    {
        Text = text ?? string.Empty;
        _rawTags = rawTags?.ToList() ?? new List<string>();
    }

    public string Text { get; }
    public IReadOnlyList<string> RawTags => _rawTags;
    public IReadOnlyDictionary<string, List<string>> Tags => _tags;
    public IReadOnlyDictionary<string, SpeakerState> Speakers => _speakers;
    public string CurrentSpeakerId { get; private set; } = string.Empty;
    public string Locale { get; private set; } = string.Empty;
    public string LayoutAnim { get; private set; } = string.Empty;
    public string AudioInfoId { get; private set; } = string.Empty;

    internal void SetSpeaker(string speakerId, SpeakerState state)
    {
        if (string.IsNullOrEmpty(speakerId))
            return;

        CurrentSpeakerId = speakerId;
        _speakers[speakerId] = state;
    }

    internal void SetCurrentSpeakerId(string speakerId) => CurrentSpeakerId = speakerId ?? string.Empty;
    internal void SetLocale(string locale) => Locale = locale ?? string.Empty;
    internal void SetLayoutAnim(string layoutAnim) => LayoutAnim = layoutAnim ?? string.Empty;
    internal void SetAudioInfo(string audioInfoId) => AudioInfoId = audioInfoId ?? string.Empty;

    internal void AddTag(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        key = key.Trim();
        if (!_tags.TryGetValue(key, out var values))
        {
            values = new List<string>();
            _tags[key] = values;
        }

        values.Add(value ?? string.Empty);
    }

    public bool HasTag(string key)
    {
        return !string.IsNullOrWhiteSpace(key) && _tags.ContainsKey(key.Trim());
    }

    public bool TryGetTagValue(string key, out string value)
    {
        value = string.Empty;
        if (string.IsNullOrWhiteSpace(key) || !_tags.TryGetValue(key.Trim(), out var values) || values.Count == 0)
            return false;

        value = values[values.Count - 1];
        return true;
    }

    public IReadOnlyList<string> GetTagValues(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || !_tags.TryGetValue(key.Trim(), out var values))
            return Array.Empty<string>();

        return values;
    }
}

public class TagProcessContext
{
    public TagProcessContext(IInkMetaParser metaParser)
    {
        MetaParser = metaParser;
        CurrentLine = new DialogueLineContext(string.Empty, null);
    }

    public IInkMetaParser MetaParser { get; }
    public DialogueLineContext CurrentLine { get; private set; }

    public string CurrentSpeakerId
    {
        get => CurrentLine?.CurrentSpeakerId ?? string.Empty;
        set => CurrentLine?.SetCurrentSpeakerId(value);
    }

    public DialogueLineContext BeginLine(string text, IEnumerable<string> rawTags)
    {
        CurrentLine = new DialogueLineContext(text, rawTags);
        return CurrentLine;
    }
}

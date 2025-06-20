using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public interface ITypewriterEffect
{
    void StartTyping(string text);
    void Skip(bool quickSkipNeeded = false);
    Action CompleteTextRevealed { get; set; }
    Action<char> CharacterRevealed { get; set; }
}

public interface ITypewriterAudioEffect
{
    void SetCurrentAudioInfo(string id);

    void SetDefaultAudioInfo();
}

[RequireComponent(typeof(TMP_Text))]
public class TypewriterEffect : MonoBehaviour, ITypewriterEffect, ITypewriterAudioEffect
{
    [Header("Typewriter Settings")]
    [SerializeField] private float m_CharactersPerSecond = 20;

    [SerializeField] private float m_InterpunctuationDelay = 0.5f;

    [Header("Skip options")]
    [SerializeField] private bool m_QuickSkip;

    [SerializeField][Min(1)] private int m_SkipSpeedup = 5;
    [SerializeField][Range(0.1f, 0.5f)] private float m_SendDoneDelay = 0.25f;

    [Header("Audio")]
    [SerializeField] private DialogueAudioInfoSO m_DefaultAudioInfo;

    [SerializeField] private InputAction _skipInputAction;

    [SerializeField] private DialogueAudioInfoSO[] m_AudioInfos;
    [SerializeField] private bool m_MakePredictable;
    [SerializeField] private bool m_AutoPlay;

    public bool CurrentlySkipping { get; private set; }
    public Action CompleteTextRevealed { get; set; }
    public Action<char> CharacterRevealed { get; set; }

    private TMP_Text _textBox;
    private int _currentVisibleCharacterIndex;
    private bool _readyForNewText = true;

    private Coroutine _typewriterCoroutine;
    private WaitForSeconds _simpleDelay;
    private WaitForSeconds _interpunctuationDelay;
    private WaitForSeconds _skipDelay;
    private WaitForSeconds _immediateSkipDelay;
    private WaitForSeconds _textboxFullEventDelay;

    private DialogueAudioInfoSO _currentAudioInfo;
    private Dictionary<string, DialogueAudioInfoSO> _audioInfoDictionary;
    private AudioSource _audioSource;

    private void Awake()
    {
        //_skipInputAction.performed += _ => { (this as ITypewriterEffect). Skip(); };
        _audioSource = this.gameObject.GetComponentInParent<AudioSource>();
        _currentAudioInfo = m_DefaultAudioInfo;
        _textBox = GetComponent<TMP_Text>();

        _simpleDelay = new WaitForSeconds(1 / m_CharactersPerSecond);
        _interpunctuationDelay = new WaitForSeconds(m_InterpunctuationDelay);

        _skipDelay = new WaitForSeconds(1 / (m_CharactersPerSecond * m_SkipSpeedup));
        _immediateSkipDelay = new WaitForSeconds(0.25f);
        _textboxFullEventDelay = new WaitForSeconds(m_SendDoneDelay);
        InitializeAudioInfoDictionary();
    }

    void Start()
    {
        if (m_AutoPlay)
            StartTyping(_textBox.text);
    }

    private void InitializeAudioInfoDictionary()
    {
        _audioInfoDictionary = new Dictionary<string, DialogueAudioInfoSO>();
        if (m_DefaultAudioInfo)
            _audioInfoDictionary.Add(m_DefaultAudioInfo.id, m_DefaultAudioInfo);
        foreach (DialogueAudioInfoSO audioInfo in m_AudioInfos)
        {
            _audioInfoDictionary.Add(audioInfo.id, audioInfo);
        }
    }

    //todo: change it to use new input system

    public void StartTyping(string text)
    {
        PrepareForNewText(text);
    }

    private void PrepareForNewText(string text)
    {
        CurrentlySkipping = false;
        _readyForNewText = false;

        if (_typewriterCoroutine != null)
            StopCoroutine(_typewriterCoroutine);

        _textBox.text = text;
        _textBox.maxVisibleCharacters = 0;
        _currentVisibleCharacterIndex = 0;

        _typewriterCoroutine = StartCoroutine(Typewriter());
    }

    private IEnumerator Typewriter()
    {
        yield return null;

        TMP_TextInfo textInfo = _textBox.textInfo;

        while (_currentVisibleCharacterIndex < textInfo.characterCount + 1)
        {
            var lastCharacterIndex = textInfo.characterCount - 1;

            if (_currentVisibleCharacterIndex >= lastCharacterIndex)
            {
                _textBox.maxVisibleCharacters++;
                yield return _textboxFullEventDelay;
                CompleteTextRevealed?.Invoke();
                _readyForNewText = true;
                yield break;
            }

            char character = textInfo.characterInfo[_currentVisibleCharacterIndex].character;

            _textBox.maxVisibleCharacters++;

            if (!CurrentlySkipping &&
                (character == '?' || character == '.' || character == ',' || character == ':' ||
                 character == ';' || character == '!' || character == '-'))
                yield return _interpunctuationDelay;
            else
                yield return CurrentlySkipping ? _skipDelay : _simpleDelay;


            CharacterRevealed?.Invoke(character);
            PlayDialogueSound(_currentVisibleCharacterIndex, character);
            _currentVisibleCharacterIndex++;
        }
    }

    void ITypewriterEffect.Skip(bool quickSkipNeeded)
    {
        if (CurrentlySkipping)
            return;

        CurrentlySkipping = true;

        if (!m_QuickSkip || !quickSkipNeeded)
        {
            StartCoroutine(SkipSpeedupReset());
            return;
        }

        StopCoroutine(_typewriterCoroutine);
        StartCoroutine(SkipWithDelay());
        //_textBox.maxVisibleCharacters = _textBox.textInfo.characterCount;
        //_readyForNewText = true;
        //CompleteTextRevealed?.Invoke();
    }

    private IEnumerator SkipWithDelay()
    {
        _textBox.maxVisibleCharacters = _textBox.textInfo.characterCount;
        yield return _immediateSkipDelay;
        _readyForNewText = true;
        CompleteTextRevealed?.Invoke();
    }

    private IEnumerator SkipSpeedupReset()
    {
        yield return new WaitUntil(() => _textBox.maxVisibleCharacters == _textBox.textInfo.characterCount - 1);
        CurrentlySkipping = false;
    }

    private void PlayDialogueSound(int currentDisplayedCharacterCount, char currentCharacter)
    {
        // set variables for the below based on our config
        if (_currentAudioInfo == null)
            return;

        AudioClip[] dialogueTypingSoundClips = _currentAudioInfo.dialogueTypingSoundClips;
        int frequencyLevel = _currentAudioInfo.frequencyLevel;
        float minPitch = _currentAudioInfo.minPitch;
        float maxPitch = _currentAudioInfo.maxPitch;
        bool stopAudioSource = _currentAudioInfo.stopAudioSource;

        // play the sound based on the config
        if (currentDisplayedCharacterCount % frequencyLevel == 0)
        {
            if (stopAudioSource)
                _audioSource.Stop();

            AudioClip soundClip = null;
            // create predictable audio from hashing
            if (m_MakePredictable)
            {
                int hashCode = currentCharacter.GetHashCode();
                // sound clip
                int predictableIndex = hashCode % dialogueTypingSoundClips.Length;
                soundClip = dialogueTypingSoundClips[predictableIndex];
                // pitch
                int minPitchInt = (int)(minPitch * 100);
                int maxPitchInt = (int)(maxPitch * 100);
                int pitchRangeInt = maxPitchInt - minPitchInt;
                // cannot divide by 0, so if there is no range then skip the selection
                if (pitchRangeInt != 0)
                {
                    int predictablePitchInt = (hashCode % pitchRangeInt) + minPitchInt;
                    float predictablePitch = predictablePitchInt / 100f;
                    _audioSource.pitch = predictablePitch;
                }
                else
                    _audioSource.pitch = minPitch;
            }
            // otherwise, randomize the audio
            else
            {
                // sound clip
                int randomIndex = Random.Range(0, dialogueTypingSoundClips.Length);
                soundClip = dialogueTypingSoundClips[randomIndex];
                // pitch
                _audioSource.pitch = Random.Range(minPitch, maxPitch);
            }

            // play sound
            _audioSource.PlayOneShot(soundClip);
        }
    }

    void ITypewriterAudioEffect.SetCurrentAudioInfo(string id)
    {
        DialogueAudioInfoSO audioInfo = null;
        _audioInfoDictionary?.TryGetValue(id, out audioInfo);
        if (audioInfo != null)
            this._currentAudioInfo = audioInfo;
        else
            Debug.LogWarning("Failed to find audio info for id: " + id);
    }

    void ITypewriterAudioEffect.SetDefaultAudioInfo()
    {
        if (m_DefaultAudioInfo == null)
            return;
        (this as ITypewriterAudioEffect)?.SetCurrentAudioInfo(m_DefaultAudioInfo.id);
    }
}
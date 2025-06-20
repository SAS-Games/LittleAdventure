using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Ink.Runtime;

public class DialogueWidget : MonoBehaviour, IDialogueWidget
{
    [Header("UI References")] [SerializeField]
    private Canvas _dialogueCanvas;

    [SerializeField] private TextMeshProUGUI _textDisplay;
    [SerializeField] private Transform _speakerContainer;
    [SerializeField] private Transform _choicesContainer;
    [SerializeField] private GameObject _choiceButtonPrefab;

    private readonly Dictionary<string, SpeakerView> _speakers = new();
    private readonly List<ChoiceWidget> _activeChoices = new();
    private DialogueConfig _config;


    public event Action<int> OnChoiceSelected;
    public event Action OnContinuePressed;
    public void ShowDialogue() => _dialogueCanvas.enabled = true;
    public void HideDialogue() => _dialogueCanvas.enabled = false;

    public IEnumerator DisplayLine(string text)
    {
        _textDisplay.text = text;
        yield break;
    }

    public void ShowChoices(List<Choice> choices)
    {
        ClearChoices();

        foreach (var choice in choices)
        {
            var buttonObj = Instantiate(_choiceButtonPrefab, _choicesContainer);
            var button = buttonObj.GetComponent<ChoiceWidget>();
            button.Init(choice.text, choices.IndexOf(choice));
            _activeChoices.Add(button);
        }
    }

    public void HideChoices() => ClearChoices();

    public void UpdateSpeaker(SpeakerState speaker)
    {
        if (!_speakers.TryGetValue(speaker.Id, out var view))
        {
            // view = Instantiate(_config.defaultSpeakerPrefab, _speakerContainer)
            //       .GetComponent<SpeakerView>();
            // _speakers.Add(speaker.Id, view);
        }
        //view.UpdateState(speaker);
    }

    public IEnumerator RunOperations(IEnumerable<IEnumerator> operations, Action onComplete)
    {
        foreach (var op in operations)
        {
            while (op.MoveNext())
            {
                yield return op.Current;
            }
        }
    }

    private void ClearChoices()
    {
        foreach (var choice in _activeChoices)
            Destroy(choice.gameObject);
        _activeChoices.Clear();
    }
}
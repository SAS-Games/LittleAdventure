using System;
using System.Collections;
using System.Collections.Generic;
using SAS.Utilities;
using UnityEngine;

public class DialoguePresenter
{
    private readonly DialogueModel _model;
    private readonly IDialogueWidget _widget;
    private readonly TagProcessor _tagProcessor;
    private Coroutine _autoAdvanceRoutine;
    private IChoiceView _choiceView;

    public DialoguePresenter(DialogueModel model, IDialogueWidget widget)
    {
        _model = model;
        _widget = widget;

        _tagProcessor = new TagProcessor()
            .Add(new SpeakerTagProcessor())
            .Add(new AudioTagProcessor());
        //.Add(new LayoutTagProcessor());

        _widget.OnContinuePressed += ContinueDialogue;
        if (_widget is IChoiceView choiceView)
            choiceView.OnChoiceSelected += SelectChoice;
    }

    public void BindChoiceView(IChoiceView choiceView)
    {
        _choiceView = choiceView;
        _choiceView.OnChoiceSelected += SelectChoice;
    }

    public void StartDialogue(TextAsset inkJSON)
    {
        _model.StartStory(inkJSON);
        _widget.ShowDialogue();
        ContinueDialogue();
    }

    public void ContinueDialogue()
    {
        HideChoices();
        var line = _model.ContinueStory();
        if (string.IsNullOrEmpty(line))
        {
            EndDialogue();
            return;
        }

        var operations = new List<IEnumerator>
        {
            ProcessTags(_model.CurrentStory.currentTags),
            _widget.DisplayLine(line),
        };

        StaticCoroutine.Start(RunOperations(operations, () =>
        {
            if (_model.CurrentStory.currentChoices.Count > 0)
                ShowChoices();
            else
                HandleAutoAdvance();
        }));
    }

    private void HandleAutoAdvance()
    {
        if (_model.AutoAdvance)
            StartAutoAdvance();
    }

    private IEnumerator RunOperations(IEnumerable<IEnumerator> operations, Action onComplete)
    {
        foreach (var op in operations)
        {
            while (op.MoveNext())
            {
                yield return op.Current;
            }
        }

        onComplete?.Invoke();
    }

    private IEnumerator ProcessTags(List<string> tags)
    {
        var context = new TagProcessContext
        {
            Model = _model,
            Widget = _widget,
            Config = _model.Config,
            MetaParser = _model.MetaParser
        };

        yield return _tagProcessor.Process(tags, context);
    }

    private void ShowChoices()
    {
        _choiceView?.ShowChoices(_model.CurrentStory.currentChoices);
    }

    private void HideChoices()
    {
        _choiceView?.HideChoices();
    }

    private void StartAutoAdvance()
    {
        _autoAdvanceRoutine = StaticCoroutine.Start(AutoAdvanceRoutine(_model.AutoAdvanceDelay));
    }

    private IEnumerator AutoAdvanceRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        ContinueDialogue();
    }

    private void StopAutoAdvance()
    {
        if (_autoAdvanceRoutine != null)
        {
            // _view.StopCoroutine(_autoAdvanceRoutine);
            _autoAdvanceRoutine = null;
        }
    }

    public void SelectChoice(int index)
    {
        _model.CurrentStory.ChooseChoiceIndex(index);
        ContinueDialogue();
    }

    private void EndDialogue()
    {
        _widget.HideDialogue();
    }

    public void UnbindChoiceView()
    {
        _choiceView.OnChoiceSelected -= SelectChoice;
        _choiceView = null;
    }
}
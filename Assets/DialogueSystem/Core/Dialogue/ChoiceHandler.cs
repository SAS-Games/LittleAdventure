using Ink.Runtime;
using SAS.Core.TagSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAS.DialogueSystem
{
    public struct ChoiceContext
    {
        public IReadOnlyList<Choice> Choices;
        public IReadOnlyList<ChoiceOptionContext> Options;
    }

    public struct ChoiceOptionContext
    {
        public Choice Choice;
        public int ChoiceIndex;
        public DialogueLineContext LineContext;
    }

    public class ChoiceHandler : MonoBehaviour
    {
        [FieldRequiresParent] private DialogueHandler _dialogueHandler;

        public event Action<ChoiceContext> OnChoicesPrepared;
        public event Action OnChoicesHidden;

        private void Awake()
        {
            this.Initialize();
        }

        private void OnEnable()
        {
            if (_dialogueHandler == null)
                return;

            _dialogueHandler.OnLineReady += HandleLineReady;
            _dialogueHandler.OnLineMessageShown += HandleLineMessageShown;
        }

        private void OnDisable()
        {
            if (_dialogueHandler == null)
                return;

            _dialogueHandler.OnLineReady -= HandleLineReady;
            _dialogueHandler.OnLineMessageShown -= HandleLineMessageShown;
        }

        private void HandleLineReady(DialogueLineContext _)
        {
            OnChoicesHidden?.Invoke();
        }

        private void HandleLineMessageShown(Story story, DialogueLineContext _)
        {
            if (story == null)
            {
                OnChoicesHidden?.Invoke();
                return;
            }

            var choices = story.currentChoices;
            var options = new List<ChoiceOptionContext>(choices.Count);

            for (int index = 0; index < choices.Count; index++)
            {
                var choice = choices[index];
                options.Add(new ChoiceOptionContext
                {
                    Choice = choice,
                    ChoiceIndex = index,
                    LineContext = BuildChoiceLineContext(choice)
                });
            }

            OnChoicesPrepared?.Invoke(new ChoiceContext
            {
                Choices = choices,
                Options = options
            });
        }

        private DialogueLineContext BuildChoiceLineContext(Choice choice)
        {
            return _dialogueHandler.CreateLineContext(choice.text, choice.tags);
        }

        public void MakeChoice(int choiceIndex) => _dialogueHandler.MakeChoice(choiceIndex);
    }
}

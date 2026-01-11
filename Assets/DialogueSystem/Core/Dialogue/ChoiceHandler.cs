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
    }

    public class ChoiceHandler : MonoBehaviour
    {
        [FieldRequiresParent] private DialogueHandler _dialogueHandler;

        private ITagProcessor[] _tagProcessors;

        public event Action<ChoiceContext> OnChoicesPrepared;
        public event Action OnChoicesHidden;

        private void Awake()
        {
            this.Initialize();
            _tagProcessors = GetComponentsInChildren<ITagProcessor>();
        }

        private void OnEnable()
        {
            _dialogueHandler.OnStoryContinue += HandleStoryContinue;
            _dialogueHandler.OnStoryMessageShown += HandleStoryMessageShown;
        }

        private void OnDisable()
        {
            _dialogueHandler.OnStoryContinue -= HandleStoryContinue;
            _dialogueHandler.OnStoryMessageShown -= HandleStoryMessageShown;
        }

        private void HandleStoryContinue(string _)
        {
            OnChoicesHidden?.Invoke();
        }

        private void HandleStoryMessageShown(Story story)
        {
            var choices = story.currentChoices;

            foreach (var choice in choices)
            {
                if (choice.tags == null)
                    continue;

                foreach (var tag in choice.tags)
                {
                    if (!Utils.GetTagKeyValue(tag, out var key, out var value))
                        continue;

                    foreach (var processor in _tagProcessors)
                    {
                        if (!processor.CanHandle(key))
                            continue;

                        processor.Reset();
                        processor.Process(value, _dialogueHandler.TagProcessContext);
                    }
                }
            }

            OnChoicesPrepared?.Invoke(new ChoiceContext
            {
                Choices = choices
            });
        }
    }
}

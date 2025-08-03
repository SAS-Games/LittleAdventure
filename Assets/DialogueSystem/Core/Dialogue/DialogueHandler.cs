using Ink.Runtime;
using SAS.Utilities.TagSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SAS.DialogueSystem
{
    public class DialogueHandler : MonoBehaviour, IDialogueHandler
    {
        [Header("Load Globals JSON")]
        [SerializeField] private TextAsset m_LoadGlobalsJSON;
        [SerializeField] private GameObject m_DialoguePanel;

        [SerializeField] private bool m_AutoContinueToNextLine;
        [SerializeField] private InputAction _nextInputAction;
        [FieldRequiresSelf] private IInkMetaParser _inkMetaParser;

        public Story CurrentStory { get; private set; }

        private DialogueGlobalVariables _dialogueGlobalVariables;
        private InkExternalFunctions _inkExternalFunctions;
        public bool DialogueIsPlaying { get; private set; }

        private ITagProcessor[] _tagProcessors;
        private TagProcessContext _tagProcessContext;

        public Action<Story> OnStoryMessageShown;
        public Action<String> OnStoryContinue;
        public Action OnEnterDialogueMode;
        public Action OnExitDialogueMode;
        public Action OnSkipRequested;
        private bool _canContinueToNextLine = false;

        private void Awake()
        {
            this.Initialize();
            _tagProcessors = GetComponentsInChildren<ITagProcessor>();
            _tagProcessContext = new TagProcessContext(_inkMetaParser);
            _nextInputAction.performed += _ => Skip();

            _dialogueGlobalVariables = new DialogueGlobalVariables(m_LoadGlobalsJSON);
            _inkExternalFunctions = new InkExternalFunctions();
        }

        private void OnEnable() => _nextInputAction.Enable();
        private void OnDisable() => _nextInputAction.Disable();


        private void Start()
        {
            DialogueIsPlaying = false;
            m_DialoguePanel.SetActive(false);
        }

        public void EnterDialogueMode(TextAsset inkJSON, Animator emoteAnimator)
        {
            _canContinueToNextLine = true;
            EventBus<DialogueStartEvent>.Raise(new DialogueStartEvent { dialogueHandler = this });
            OnEnterDialogueMode?.Invoke();

            CurrentStory = new Story(inkJSON.text);
            DialogueIsPlaying = true;
            m_DialoguePanel.SetActive(true);
            _dialogueGlobalVariables.StartListening(CurrentStory);
            if (emoteAnimator)
                _inkExternalFunctions.Bind(CurrentStory, emoteAnimator);
            ContinueStory();
        }

        private IEnumerator ExitDialogueMode()
        {
            yield return new WaitForSeconds(0.2f);

            _dialogueGlobalVariables.StopListening(CurrentStory);

            // _inkExternalFunctions.Unbind(_currentStory); ToDo

            DialogueIsPlaying = false;
            m_DialoguePanel.SetActive(false);

            // go back to default audio
            //(_typewriterEffect as ITypewriterAudioEffect)?.SetDefaultAudioInfo();
            OnExitDialogueMode?.Invoke();
            EventBus<DialogueEndEvent>.Raise(new DialogueEndEvent { dialogueHandler = this });
        }

        public void ContinueStory()
        {
            if (!m_AutoContinueToNextLine)
            {
                if (_canContinueToNextLine)
                    _canContinueToNextLine = false;
                else
                    return;
            }

            if (CurrentStory.canContinue)
            {
                string nextLine = CurrentStory.Continue();
                if (!nextLine.Equals("") || CurrentStory.canContinue)
                {
                    HandleTags(CurrentStory.currentTags);
                    OnStoryContinue?.Invoke(nextLine);
                }
                else
                    StartCoroutine(ExitDialogueMode());
            }
            else
                StartCoroutine(ExitDialogueMode());
        }

        private void HandleTags(List<string> currentTags)
        {
            foreach (string tag in currentTags)
            {
                if (Utils.GetTagKeyValue(tag, out string tagKey, out string tagValue))
                {
                    foreach (var tagProcessor in _tagProcessors)
                    {
                        tagProcessor.Reset();
                        if (tagProcessor.CanHandle(tagKey))
                            tagProcessor.Process(tagValue, _tagProcessContext);
                    }
                }
            }
        }

        public void MakeChoice(int choiceIndex)
        {
            CurrentStory.ChooseChoiceIndex(choiceIndex);
            ContinueStory();

        }

        private void Skip()
        {
            OnSkipRequested?.Invoke();
            _canContinueToNextLine = true;
        }

        private void OnApplicationQuit()
        {
            _dialogueGlobalVariables?.SaveVariables();
        }
    }
}
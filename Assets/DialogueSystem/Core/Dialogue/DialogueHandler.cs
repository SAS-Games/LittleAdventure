using Ink.Runtime;
using SAS.Core.TagSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = SAS.Debug;

namespace SAS.DialogueSystem
{
    public class DialogueHandler : MonoBehaviour, IDialogueHandler
    {
        [Header("Load Globals JSON")]
        [SerializeField] private TextAsset m_LoadGlobalsJSON;
        [SerializeField] private string m_GlobalsSaveKey = "INK_VARIABLES";
        [SerializeField] private GameObject m_DialoguePanel;
        [field:SerializeField] public bool AutoContinueToNextLine { get;private set; }
        [SerializeField] private InputAction _nextInputAction;
        [FieldRequiresSelf] private IInkMetaParser _inkMetaParser;

        public Story CurrentStory { get; private set; }

        private DialogueGlobalVariables _dialogueGlobalVariables;
        private InkExternalMethodRegistry _inkExternalMethodRegistry;
        public InkExternalMethodRegistry InkExternalMethodRegistry => _inkExternalMethodRegistry;
        public bool DialogueIsPlaying { get; private set; }

        private ITagProcessor[] _tagProcessors;
        private TagProcessContext _tagProcessContext;
        public TagProcessContext TagProcessContext => _tagProcessContext;
        public DialogueLineContext CurrentLineContext => _tagProcessContext?.CurrentLine;


        public event Action<Story> OnStoryMessageShown;
        public event Action<Story, DialogueLineContext> OnLineMessageShown;
        public event Action<string> OnStoryContinue;
        public event Action<DialogueLineContext> OnLineReady;
        public event Action OnEnterDialogueMode;
        public event Action OnExitDialogueMode;
        public event Action OnSkipRequested;
        private GameObject _initiator;
        private IContextBinder _contextBinder;
        private Coroutine _exitCoroutine;

        private void Awake()
        {
            this.Initialize();
            _tagProcessors = GetComponentsInChildren<ITagProcessor>();
            _tagProcessContext = new TagProcessContext(_inkMetaParser);
            if (_nextInputAction != null)
                _nextInputAction.performed += OnNextInputPerformed;

            if (m_LoadGlobalsJSON != null)
                _dialogueGlobalVariables = new DialogueGlobalVariables(m_LoadGlobalsJSON, m_GlobalsSaveKey);
            else
                Debug.LogWarning("Dialogue globals JSON is not assigned.", "DialogueHandler");

            _inkExternalMethodRegistry = new InkExternalMethodRegistry();
        }

        private void OnEnable() => _nextInputAction?.Enable();
        private void OnDisable() => _nextInputAction?.Disable();


        private void Start()
        {
            DialogueIsPlaying = false;
            if (m_DialoguePanel != null)
                m_DialoguePanel.SetActive(false);
        }

        public void EnterDialogueMode(TextAsset inkJSON, GameObject initiator)
        {
            if (DialogueIsPlaying)
                return;

            if (inkJSON == null)
            {
                Debug.LogError("Cannot enter dialogue mode because Ink JSON is not assigned.", "DialogueHandler");
                return;
            }

            try
            {
                CurrentStory = new Story(inkJSON.text);
            }
            catch (Exception ex)
            {
                CurrentStory = null;
                Debug.LogError($"Failed to create Ink story from '{inkJSON.name}': {ex.Message}", "DialogueHandler");
                return;
            }

            _initiator = initiator;
            DialogueIsPlaying = true;
            if (m_DialoguePanel != null)
                m_DialoguePanel.SetActive(true);
            _dialogueGlobalVariables?.StartListening(CurrentStory);

            EventBus<DialogueStartEvent>.Raise(new DialogueStartEvent
            {
                dialogueHandler = this,
                initiator = initiator
            });
            OnEnterDialogueMode?.Invoke();
            _inkExternalMethodRegistry.Bind(CurrentStory);
            ContinueStory();
        }

        private IEnumerator ExitDialogueMode()
        {
            yield return new WaitForSeconds(0.2f);

            _dialogueGlobalVariables?.StopListening(CurrentStory);
            _inkExternalMethodRegistry.Unbind(CurrentStory);

            DialogueIsPlaying = false;
            if (m_DialoguePanel != null)
                m_DialoguePanel.SetActive(false);

            // go back to default audio
            //(_typewriterEffect as ITypewriterAudioEffect)?.SetDefaultAudioInfo();
            OnExitDialogueMode?.Invoke();
            EventBus<DialogueEndEvent>.Raise(new DialogueEndEvent
            {
                dialogueHandler = this,
                initiator = _initiator
            });
            _initiator = null;
            CurrentStory = null;
            _exitCoroutine = null;
        }

        public void ContinueStory()
        {
            if (CurrentStory == null)
                return;

            if (CurrentStory.canContinue)
            {
                string nextLine;
                try
                {
                    nextLine = CurrentStory.Continue();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to continue Ink story: {ex.Message}", "DialogueHandler");
                    BeginExitDialogueMode();
                    return;
                }

                if (!string.IsNullOrEmpty(nextLine) || CurrentStory.canContinue || CurrentStory.currentChoices.Count > 0)
                {
                    var lineContext = BuildLineContext(nextLine, CurrentStory.currentTags);
                    OnLineReady?.Invoke(lineContext);
                    OnStoryContinue?.Invoke(nextLine);
                }
                else
                    BeginExitDialogueMode();
            }
            else if (CurrentStory.currentChoices.Count > 0)
                return;
            else
                BeginExitDialogueMode();
        }

        private DialogueLineContext BuildLineContext(string lineText, List<string> currentTags)
        {
            var lineContext = _tagProcessContext.BeginLine(lineText, currentTags);
            ProcessTags(_tagProcessContext, currentTags);
            return lineContext;
        }

        public DialogueLineContext CreateLineContext(string lineText, List<string> currentTags)
        {
            var context = new TagProcessContext(_inkMetaParser);
            var lineContext = context.BeginLine(lineText, currentTags);
            ProcessTags(context, currentTags, false);
            return lineContext;
        }

        public void NotifyLineMessageShown(DialogueLineContext lineContext)
        {
            OnStoryMessageShown?.Invoke(CurrentStory);
            OnLineMessageShown?.Invoke(CurrentStory, lineContext);
        }

        private void ProcessTags(TagProcessContext context, List<string> currentTags)
        {
            ProcessTags(context, currentTags, true);
        }

        private void ProcessTags(TagProcessContext context, List<string> currentTags, bool runProcessors)
        {
            if (runProcessors)
            {
                foreach (var tagProcessor in _tagProcessors)
                    tagProcessor.Reset();
            }

            if (currentTags == null)
                return;

            foreach (string tag in currentTags)
            {
                if (Utils.GetTagKeyValue(tag, out string tagKey, out string tagValue))
                {
                    ApplyTagMetadata(context, tagKey, tagValue);

                    if (!runProcessors)
                        continue;

                    foreach (var tagProcessor in _tagProcessors)
                    {
                        if (tagProcessor.CanHandle(tagKey))
                            tagProcessor.Process(tagValue, context);
                    }
                }
            }
        }

        private void ApplyTagMetadata(TagProcessContext context, string tagKey, string tagValue)
        {
            context.CurrentLine.AddTag(tagKey, tagValue);

            if (string.Equals(tagKey, "local", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(tagKey, "locale", StringComparison.OrdinalIgnoreCase))
            {
                context.CurrentLine.SetLocale(tagValue);
                return;
            }

            if (string.Equals(tagKey, "layout", StringComparison.OrdinalIgnoreCase))
            {
                context.CurrentLine.SetLayoutAnim(tagValue);
                return;
            }

            if (string.Equals(tagKey, "audio", StringComparison.OrdinalIgnoreCase))
            {
                context.CurrentLine.SetAudioInfo(tagValue);
                return;
            }

            if (!string.Equals(tagKey, "speaker", StringComparison.OrdinalIgnoreCase) || context.MetaParser == null)
                return;

            var parsed = context.MetaParser.Parse(tagValue);
            if (!parsed.TryGetValue("id", out var speakerId))
                return;

            context.CurrentLine.SetSpeaker(speakerId, new SpeakerState
            {
                Name = parsed.GetValueOrDefault("name"),
                Image = parsed.GetValueOrDefault("image"),
                Animation = parsed.GetValueOrDefault("anim")
            });
        }

        public void MakeChoice(int choiceIndex)
        {
            if (CurrentStory == null)
                return;

            if (choiceIndex < 0 || choiceIndex >= CurrentStory.currentChoices.Count)
            {
                Debug.LogWarning($"Choice index {choiceIndex} is out of range.", "DialogueHandler");
                return;
            }

            Debug.Log("Making choice " + choiceIndex, "DialogueHandler");
            try
            {
                CurrentStory.ChooseChoiceIndex(choiceIndex);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to choose Ink choice index {choiceIndex}: {ex.Message}", "DialogueHandler");
                BeginExitDialogueMode();
                return;
            }

            ContinueStory();
        }

        private void Skip()
        {
            OnSkipRequested?.Invoke();
        }

        private void OnNextInputPerformed(InputAction.CallbackContext _) => Skip();

        private void BeginExitDialogueMode()
        {
            if (_exitCoroutine != null)
                return;

            _exitCoroutine = StartCoroutine(ExitDialogueMode());
        }

        private void OnApplicationQuit()
        {
            _dialogueGlobalVariables?.SaveVariables();
        }

        public void OnCreated(IContextBinder contextBinder)
        {
            _contextBinder = contextBinder;
        }

        private void OnDestroy()
        {
            if (_nextInputAction != null)
                _nextInputAction.performed -= OnNextInputPerformed;
            _contextBinder?.Remove(this);
        }
    }
}

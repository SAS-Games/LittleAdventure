using SAS.DialogueSystem;
using SAS.Utilities.TagSystem;
using UnityEngine;
using Ink.Runtime;
using UnityEngine.Events;

public class DialogueEventListener : MonoBehaviour
{
    [Inject] protected IDialogueHandler _dialogueHandler;
    [SerializeField] private UnityEvent<DialogueHandler> m_OnDialogueStart;
    [SerializeField] private UnityEvent<DialogueHandler> m_OnDialogueEnd;
    [SerializeField] private UnityEvent<string> m_OnDialogueTextRevealed;

    private EventBinding<DialogueStartEvent> _dialogueStartEventBinding;
    private EventBinding<DialogueEndEvent> _dialogueEndEventBinding;
    protected Story CurrentStory => ((DialogueHandler)_dialogueHandler).CurrentStory;

    protected virtual void Awake()
    {
        _dialogueStartEventBinding = new EventBinding<DialogueStartEvent>(OnDialogueStartInternal);
        _dialogueEndEventBinding = new EventBinding<DialogueEndEvent>(OnDialogueEndInternal);
    }

    protected virtual void OnEnable()
    {
        EventBus<DialogueStartEvent>.Register(_dialogueStartEventBinding);
        EventBus<DialogueEndEvent>.Register(_dialogueEndEventBinding);
    }

    protected virtual void Start()
    {
        this.Initialize();
    }

    protected virtual void OnDisable()
    {
        EventBus<DialogueStartEvent>.Deregister(_dialogueStartEventBinding);
        EventBus<DialogueEndEvent>.Deregister(_dialogueEndEventBinding);
    }

    private void OnDialogueStartInternal(DialogueStartEvent evt)
    {
        var dialogueHandler = _dialogueHandler as DialogueHandler;
        m_OnDialogueStart?.Invoke(dialogueHandler);
        OnDialogueStart(evt);
        dialogueHandler.OnStoryMessageShown += OnTextRevealed;
    }

    private void OnTextRevealed(Story story)
    {
        m_OnDialogueTextRevealed.Invoke(((DialogueHandler)_dialogueHandler).TagProcessContext.CurrentSpeakerId);
    }

    private void OnDialogueEndInternal(DialogueEndEvent evt)
    {
        var dialogueHandler = _dialogueHandler as DialogueHandler;
        m_OnDialogueEnd?.Invoke(dialogueHandler);
        OnDialogueEnd(evt);
        dialogueHandler.OnStoryMessageShown -= OnTextRevealed;
    }

    /// <summary>
    /// Called when a dialogue starts.
    /// </summary>
    protected virtual void OnDialogueStart(DialogueStartEvent dialogueStartEvent)
    {
    }

    /// <summary>
    /// Called when a dialogue ends.
    /// </summary>
    protected virtual void OnDialogueEnd(DialogueEndEvent dialogueEndEvent)
    {
    }
}
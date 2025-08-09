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
        m_OnDialogueStart?.Invoke(_dialogueHandler as DialogueHandler);
        OnDialogueStart(evt);
    }

    private void OnDialogueEndInternal(DialogueEndEvent evt)
    {
        m_OnDialogueEnd?.Invoke(_dialogueHandler as DialogueHandler);
        OnDialogueEnd(evt);
    }

    /// <summary>
    /// Called when a dialogue starts.
    /// </summary>
    protected virtual void OnDialogueStart(DialogueStartEvent dialogueStartEvent){}

    /// <summary>
    /// Called when a dialogue ends.
    /// </summary>
    protected virtual void OnDialogueEnd(DialogueEndEvent dialogueEndEvent){}
}
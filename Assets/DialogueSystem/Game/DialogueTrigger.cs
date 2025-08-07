using SAS.Utilities.TagSystem;
using UnityEngine;
using UnityEngine.Events;


public class DialogueTrigger : MonoBehaviour
{
    [Inject] private IDialogueHandler _dialogueHandler;

    [Header("Ink JSON")]
    [SerializeField] private TextAsset inkJSON;
    [SerializeField] private bool m_AutoStart = false;
    [SerializeField] private bool m_TriggerOncePerSession = true;
    [SerializeField] private UnityEvent m_OnDialogueStart;
    [SerializeField] private UnityEvent m_OnDialogueEnd;

    private bool _triggered = false;
    private EventBinding<DialogueStartEvent> _dialogueStartEventBinding;
    private EventBinding<DialogueEndEvent> _dialogueEndEventBinding;

    private void Awake()
    {
        _dialogueStartEventBinding = new EventBinding<DialogueStartEvent>(OnDialogueStart);
        _dialogueEndEventBinding = new EventBinding<DialogueEndEvent>(OnDialogueEnd);
    }
    private void OnEnable()
    {
        EventBus<DialogueStartEvent>.Register(_dialogueStartEventBinding);
        EventBus<DialogueEndEvent>.Register(_dialogueEndEventBinding);
    }

    private void Start()
    {
        _dialogueHandler = GetComponentInChildren<IDialogueHandler>(true);
        this.Initialize();
        if (m_AutoStart)
            ShowDialogue();
    }

    private void OnDisable()
    {
        EventBus<DialogueStartEvent>.Deregister(_dialogueStartEventBinding);
        EventBus<DialogueEndEvent>.Deregister(_dialogueEndEventBinding);
    }

    public void ShowDialogue()
    {
        if (_dialogueHandler.DialogueIsPlaying)
            return;

        if (m_TriggerOncePerSession)
        {
            if (!_triggered)
            {
                _triggered = true;
                _dialogueHandler.EnterDialogueMode(inkJSON);
            }
        }
        else
            _dialogueHandler.EnterDialogueMode(inkJSON);
    }

    private void OnDialogueStart(DialogueStartEvent dialogueStartEvent)
    {
        m_OnDialogueStart?.Invoke();
    }

    private void OnDialogueEnd(DialogueEndEvent dialogueEndEvent)
    {
        m_OnDialogueEnd?.Invoke();
    }
}
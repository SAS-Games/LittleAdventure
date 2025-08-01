using SAS.Utilities.TagSystem;
using UnityEngine;


public class DialogueTrigger : MonoBehaviour
{
    [Inject] private IDialogueHandler _dialogueHandler;

    [Header("Ink JSON")]
    [SerializeField] private TextAsset inkJSON;
    [SerializeField] private bool m_AutoStart = false;
    [SerializeField] private bool m_TriggerOncePerSession = true;

    private bool _triggered = false;

    private void Start()
    {
        _dialogueHandler = GetComponentInChildren<IDialogueHandler>(true);
        this.Initialize();
        if (m_AutoStart)
            ShowDialogue();
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
                _dialogueHandler.EnterDialogueMode(inkJSON, null);
            }
        }
        else
            _dialogueHandler.EnterDialogueMode(inkJSON, null);
    }
}
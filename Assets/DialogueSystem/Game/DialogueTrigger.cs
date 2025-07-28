using SAS.Utilities.TagSystem;
using UnityEngine;


public class DialogueTrigger : MonoBehaviour
{
    [Inject] private IDialogueHandler _dialogueHandler;

    [Header("Ink JSON")]
    [SerializeField] private TextAsset inkJSON;
    [SerializeField] private bool m_TriggerOncePerSession = true;

    private bool _triggered = false;

    private void Awake()
    {
        this.Initialize();
    }

    public void ShowDialogue()
    {
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
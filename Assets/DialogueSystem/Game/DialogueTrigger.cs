using SAS.Core.TagSystem;
using UnityEngine;
using UnityEngine.Serialization;

public class DialogueTrigger : MonoBehaviour
{
    [Inject] private IDialogueHandler _dialogueHandler;
    [Header("Ink JSON")]
    [FormerlySerializedAs("inkJSON")]
    [SerializeField] private TextAsset m_InkJSON;
    [SerializeField] private bool m_AutoStart = false;
    [SerializeField] private bool m_TriggerOncePerSession = true;
    private bool _triggered = false;
    
    private void Start()
    {
        this.Initialize();
        if (m_AutoStart)
            ShowDialogue();
    }

    public void ShowDialogue()
    {
        if (_dialogueHandler == null)
        {
            Debug.LogWarning("DialogueTrigger cannot show dialogue because no dialogue handler is bound.", this);
            return;
        }

        if (m_InkJSON == null)
        {
            Debug.LogWarning("DialogueTrigger cannot show dialogue because Ink JSON is not assigned.", this);
            return;
        }

        if (_dialogueHandler.DialogueIsPlaying)
            return;

        if (m_TriggerOncePerSession)
        {
            if (!_triggered)
            {
                _triggered = true;
                _dialogueHandler.EnterDialogueMode(m_InkJSON, gameObject);
            }
        }
        else
            _dialogueHandler.EnterDialogueMode(m_InkJSON, gameObject);
    }
}

using Ink.Runtime;
using SAS.Utilities.TagSystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

#if UNITY_LOCALIZATION
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
#endif

public class DialogueHandler : MonoBehaviour, IDialogueHandler
{
    [Header("Load Globals JSON")] [SerializeField]
    private TextAsset m_LoadGlobalsJSON;

    [Header("Dialogue UI")] [SerializeField]
    private GameObject m_DialoguePanel;

    [SerializeField] private GameObject m_ContinueIcon;
    [SerializeField] private TMP_Text m_DialogueText;

    [SerializeField] private Animator m_LayoutAnimator;

    //[SerializeField] private LocalizeStringEvent m_LocalizedStringEvent;
    [SerializeField] private string m_LocalizedTableName = "DialogueTextTable";
    

    [SerializeField] private bool m_Auto;
    [SerializeField] private InputAction _nextInputAction;


    [Header("Choices UI")] [SerializeField]
    private ChoiceView[] m_Choices;

    [FieldRequiresChild] private ITypewriterEffect _typewriterEffect;
    [FieldRequiresSelf] private IInkMetaParser _inkMetaParser;

    private Story _currentStory;
    private bool canContinueToNextLine = false;

    private const string LAYOUT_ANIM_TAG = "layout";
    private const string LOCAL_TAG = "local";

    private DialogueVariables _dialogueVariables;
    private InkExternalFunctions _inkExternalFunctions;
    public bool DialogueIsPlaying { get; private set; }
    private bool _skip = false;
    private string _localkey = "";
    private ITagProcessor[] _tagProcessors;
    private TagProcessContext _tagProcessContext;

    private void Awake()
    {
        this.Initialize();
        _tagProcessors = GetComponentsInChildren<ITagProcessor>();
        _tagProcessContext = new TagProcessContext(_inkMetaParser);
        _nextInputAction.performed += _ => Skip();

        _typewriterEffect.CompleteTextRevealed += () =>
        {
            m_ContinueIcon.SetActive(true);
            DisplayChoices();
            canContinueToNextLine = true;
        };

        _dialogueVariables = new DialogueVariables(m_LoadGlobalsJSON);
        _inkExternalFunctions = new InkExternalFunctions();
    }

    public void Skip()
    {
        if (!_skip)
        {
            _skip = true;
            _typewriterEffect.Skip(true);
        }
    }

    private void OnEnable()
    {
        _nextInputAction.Enable();
    }

    private void OnDisable()
    {
        _nextInputAction.Disable();
    }

    private void Start()
    {
        DialogueIsPlaying = false;
        m_DialoguePanel.SetActive(false);

        int index = 0;
        foreach (var choice in m_Choices)
        {
            choice.BindSelectedEvent(MakeChoice, index);
            index++;
        }
    }

    private void Update()
    {
        if (!DialogueIsPlaying)
            return;

        if ((m_Auto || _skip) && canContinueToNextLine && _currentStory.currentChoices.Count == 0)
            ContinueStory();
    }

    public void EnterDialogueMode(TextAsset inkJSON, Animator emoteAnimator)
    {
        EventBus<DialogueStartEvent>.Raise(new DialogueStartEvent { dialogueHandler = this });
        

        _currentStory = new Story(inkJSON.text);
        DialogueIsPlaying = true;
        m_DialoguePanel.SetActive(true);
        //m_LocalizedStringEvent.OnUpdateString.AddListener(_typewriterEffect.StartTyping);
        _dialogueVariables.StartListening(_currentStory);
        if (emoteAnimator)
            _inkExternalFunctions.Bind(_currentStory, emoteAnimator);

        m_LayoutAnimator?.Play("None");

        ContinueStory();
    }

    private IEnumerator ExitDialogueMode()
    {
        yield return new WaitForSeconds(0.2f);

        _dialogueVariables.StopListening(_currentStory);
        //m_LocalizedStringEvent.OnUpdateString.RemoveAllListeners();

        // _inkExternalFunctions.Unbind(_currentStory); ToDo

        DialogueIsPlaying = false;
        m_DialoguePanel.SetActive(false);
        m_DialogueText.text = "";

        // go back to default audio
        (_typewriterEffect as ITypewriterAudioEffect)?.SetDefaultAudioInfo();
        EventBus<DialogueEndEvent>.Raise(new DialogueEndEvent { dialogueHandler = this });
    }

    private void ContinueStory()
    {
        _skip = false;
        canContinueToNextLine = false;
        if (_currentStory.canContinue)
        {
            string nextLine = _currentStory.Continue();
            if (!nextLine.Equals("") || _currentStory.canContinue)
            {
                HandleTags(_currentStory.currentTags);
                m_ContinueIcon.SetActive(false);
                HideChoices();
                //if (!string.IsNullOrEmpty(_localkey))
                //    m_LocalizedStringEvent.StringReference = new LocalizedString(m_LocalizedTableName, _localkey);
                //else 
                _typewriterEffect.StartTyping(nextLine);
            }
            else
                StartCoroutine(ExitDialogueMode());
        }
        else
            StartCoroutine(ExitDialogueMode());
    }

    private void HideChoices()
    {
        foreach (var choice in m_Choices)
        {
            choice.gameObject.SetActive(false);
        }
    }

    private void HandleTags(List<string> currentTags)
    {
        _localkey = string.Empty;
        foreach (string tag in currentTags)
        {
            // parse the tag
            string[] splitTag = tag.Split(new char[] { ':' }, 2);
            if (splitTag.Length != 2)
                Debug.LogError("Tag could not be appropriately parsed: " + tag);

            string tagKey = splitTag[0].Trim();
            string tagValue = splitTag[1].Trim();
            foreach (var tagProcessor in _tagProcessors)
                tagProcessor.Process(tagValue, _tagProcessContext);
        }
    }

    private void DisplayChoices()
    {
        List<Choice> currentChoices = _currentStory.currentChoices;

        // defensive check to make sure our UI can support the number of choices coming in
        if (currentChoices.Count > m_Choices.Length)
            Debug.LogError("More choices were given than the UI can support. Number of choices given: " +
                           currentChoices.Count);

        int index = 0;
        // enable and initialize the choices up to the amount of choices for this line of dialogue
        foreach (Choice choice in currentChoices)
        {
            m_Choices[index].gameObject.SetActive(true);
            var localKey = string.Empty;
            if (choice.tags != null)
            {
                foreach (string tag in choice.tags)
                {
                    // parse the tag
                    string[] splitTag = tag.Split(new char[] { ':' }, 2);
                    if (splitTag.Length != 2)
                        Debug.LogError("Tag could not be appropriately parsed: " + tag);

                    string tagKey = splitTag[0].Trim();
                    string tagValue = splitTag[1].Trim();

                    // handle the tag
                    switch (tagKey)
                    {
                        case LOCAL_TAG:
                            localKey = tagValue;
                            break;

                        default:
                            Debug.LogWarning("Tag came in but is not currently being handled: " + tag);
                            break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(localKey))
                m_Choices[index].SetLocalText(localKey);
            else
                m_Choices[index].SetText(choice.text);
            index++;
        }

        // go through the remaining choices the UI supports and make sure they're hidden
        for (int i = index; i < m_Choices.Length; i++)
            m_Choices[i].gameObject.SetActive(false);

        StartCoroutine(SelectFirstChoice(currentChoices));
    }

    /// <summary>
    ///   Event System requires we clear it first, then wait
    ///   for at least one frame before we set the current selected object.
    /// </summary>
    /// <returns></returns>
    private IEnumerator SelectFirstChoice(List<Choice> currentChoices)
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return null;
        if (currentChoices?.Count == 0)
            EventSystem.current.SetSelectedGameObject(m_ContinueIcon);
        else if (m_Choices.Length > 0)
            EventSystem.current.SetSelectedGameObject(m_Choices[0].gameObject);
    }

    private void MakeChoice(int choiceIndex)
    {
        if (canContinueToNextLine)
        {
            _currentStory.ChooseChoiceIndex(choiceIndex);
            ContinueStory();
        }
    }

    public Ink.Runtime.Object GetVariableState(string variableName)
    {
        Ink.Runtime.Object variableValue = null;
        _dialogueVariables.GlobalVariables.TryGetValue(variableName, out variableValue);
        if (variableValue == null)
        {
            Debug.LogWarning("Ink Variable was found to be null: " + variableName);
        }

        return variableValue;
    }

    public void OnApplicationQuit()
    {
        _dialogueVariables?.SaveVariables();
    }
}
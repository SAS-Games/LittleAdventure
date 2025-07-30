using Ink.Runtime;
using SAS.Utilities.TagSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SAS.DialogueSystem
{
    public class ChoicePresenter : MonoBehaviour
    {
        [FieldRequiresParent] private DialogueHandler _dialogueHandler;
        [SerializeField] private ChoiceView[] m_Choices;

        private const string LOCAL_TAG = "local";

        void Start()
        {
            this.Initialize();
            _dialogueHandler.OnStoryContinue += _ => HideChoices();
            _dialogueHandler.OnStoryMessageShown += DisplayChoices;

            for (int i = 0; i < m_Choices.Length; i++)
                m_Choices[i].BindSelectedEvent(_dialogueHandler.MakeChoice, i);
        }

        private IEnumerator SelectFirstChoice(List<Choice> currentChoices)
        {
            EventSystem.current.SetSelectedGameObject(null);
            yield return null;
            if (m_Choices.Length > 0)
                EventSystem.current.SetSelectedGameObject(m_Choices[0].gameObject);
        }

        private void HideChoices()
        {
            foreach (var choice in m_Choices)
                choice.gameObject.SetActive(false);
        }

        private void DisplayChoices(Story _currentStory)
        {
            List<Choice> currentChoices = _currentStory.currentChoices;

            // defensive check to make sure our UI can support the number of choices coming in
            if (currentChoices.Count > m_Choices.Length)
                Debug.LogError("More choices were given than the UI can support. Number of choices given: " + currentChoices.Count);

            int index = 0;
            foreach (Choice choice in currentChoices)
            {
                m_Choices[index].gameObject.SetActive(true);
                var localKey = string.Empty;
                if (choice.tags != null)
                {
                    foreach (string tag in choice.tags)
                    {
                        if (Utils.GetTagKeyValue(tag, out string tagKey, out string tagValue))
                        {
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
                }

                //if (!string.IsNullOrEmpty(localKey))
                //    m_Choices[index].SetLocalText(localKey);
                // else
                m_Choices[index].SetText(choice.text);
                index++;
            }

            // go through the remaining choices the UI supports and make sure they're hidden
            for (int i = index; i < m_Choices.Length; i++)
                m_Choices[i].gameObject.SetActive(false);

            StartCoroutine(SelectFirstChoice(currentChoices));
        }
    }
}

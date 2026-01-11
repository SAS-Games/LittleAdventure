using System.Collections;
using SAS.Core.TagSystem;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SAS.DialogueSystem
{
    public class ChoicePresenter : MonoBehaviour
    {
        [FieldRequiresParent] private ChoiceHandler _handler;
        [FieldRequiresChild] private ChoiceView[] _choices;

        private void Awake()
        {
            this.Initialize();
        }

        private void OnEnable()
        {
            _handler.OnChoicesPrepared += PresentChoices;
            _handler.OnChoicesHidden += HideChoices;
        }

        private void OnDisable()
        {
            _handler.OnChoicesPrepared -= PresentChoices;
            _handler.OnChoicesHidden -= HideChoices;
        }

        private void PresentChoices(ChoiceContext context)
        {
            var choices = context.Choices;

            if (choices.Count > _choices.Length)
                Debug.LogError($"UI supports {_choices.Length} choices but received {choices.Count}");

            int index = 0;
            foreach (var choice in choices)
            {
                if (index >= _choices.Length)
                    break;

                var view = _choices[index];
                SetText(view, choice.text);
                index++;
            }

            for (int i = index; i < _choices.Length; i++)
                _choices[i].gameObject.SetActive(false);

            StartCoroutine(SelectFirstChoice());
        }

        protected virtual void SetText(ChoiceView view, string text)
        {
            view.SetText(text);
            view.gameObject.SetActive(true);
        }

        private IEnumerator SelectFirstChoice()
        {
            EventSystem.current.SetSelectedGameObject(null);
            yield return null;

            if (_choices.Length > 0)
                EventSystem.current.SetSelectedGameObject(_choices[0].gameObject);
        }

        private void HideChoices()
        {
            foreach (var choice in _choices)
                choice.gameObject.SetActive(false);
        }
    }
}
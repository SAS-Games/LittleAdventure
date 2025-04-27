using SAS.Utilities.TagSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButton : MonoBehaviour, IPointerClickHandler
{
    [FieldRequiresSelf] private Button _button;
    [FieldRequiresParent] private UIScreenView _screenView;

    private void Awake()
    {
        this.Initialize();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _screenView.OnPointerClick(eventData);
    }
}
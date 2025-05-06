using SAS.Utilities.TagSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButton : Button
{
    [FieldRequiresParent] private UIScreenView _screenView;

    protected override void Awake()
    {
        base.Awake();
        this.Initialize();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        _screenView?.OnButtonClick(this, eventData);
    }

    public override void OnSubmit(BaseEventData eventData)
    {
        base.OnSubmit(eventData);
        _screenView?.OnButtonClick(this, eventData);
    }
}
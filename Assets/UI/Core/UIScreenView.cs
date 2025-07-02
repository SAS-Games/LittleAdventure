using UnityEngine.EventSystems;

public abstract class UIScreenView : UIBehaviour
{
    protected UIScreenView _parentUIScreenView;

    protected override void Start()
    {
        base.Start();
        _parentUIScreenView = transform.parent?.GetComponentInParent<UIScreenView>();
    }

    public virtual void OnButtonClick(UIButton button, BaseEventData eventData)
    {
        _parentUIScreenView.OnButtonClick(button, eventData);
    }
    
}
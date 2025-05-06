using UnityEngine.EventSystems;

public abstract class UIScreenView : UIBehaviour
{
    public abstract void OnButtonClick(UIButton button, BaseEventData eventData);
}
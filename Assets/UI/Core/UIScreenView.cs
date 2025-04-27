using UnityEngine;
using UnityEngine.EventSystems;

public abstract class UIScreenView : UIBehaviour, IPointerClickHandler
{
    protected virtual void OnButtonClick(GameObject button, PointerEventData eventData)
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnButtonClick(eventData.selectedObject, eventData);
    }
}
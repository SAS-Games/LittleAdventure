using System;
using System.Collections.Generic;
using Ink.Runtime;
using UnityEngine;

public class TestChoiceView : MonoBehaviour, IChoiceView
{
    public event Action<int> OnChoiceSelected;

    private void Start()
    {
        GetComponent<DialogueSystem>().Presenter.BindChoiceView(this);
    }

    private void OnDestroy()
    {
        GetComponent<DialogueSystem>().Presenter.UnbindChoiceView();

    }

    public void SelectChoice(int index)
    {
        OnChoiceSelected?.Invoke(index);
    }
    
    public void ShowChoices(List<Choice> choices)
    {
        Debug.Log("Showing choices");
        foreach (var choice in choices)
        {
            Debug.Log($"\n{choice}");
        }
    }

    public void HideChoices()
    {
        Debug.Log("Hiding choices");
    }
}

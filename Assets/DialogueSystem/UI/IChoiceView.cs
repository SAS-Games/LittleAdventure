using System;
using System.Collections.Generic;
using Ink.Runtime;

public interface IChoiceView
{
    event Action<int> OnChoiceSelected;

    void ShowChoices(List<Choice> choices);
    void HideChoices();
}
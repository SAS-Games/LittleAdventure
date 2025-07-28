using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;

public class DialogueVariables
{
    public Dictionary<string, Ink.Runtime.Object> GlobalVariables { get; private set; }

    private Story _globalVariablesStory;
    private const string SaveVariablesKey = "INK_VARIABLES";

    public DialogueVariables(TextAsset loadGlobalsJSON) 
    {
        // create the story
        _globalVariablesStory = new Story(loadGlobalsJSON.text);
        // if we have saved data, load it
         if (PlayerPrefs.HasKey(SaveVariablesKey))
         {
             string jsonState = PlayerPrefs.GetString(SaveVariablesKey);
            _globalVariablesStory.state.LoadJson(jsonState);
         }

        // initialize the dictionary
        GlobalVariables = new Dictionary<string, Ink.Runtime.Object>();
        foreach (string name in _globalVariablesStory.variablesState)
        {
            Ink.Runtime.Object value = _globalVariablesStory.variablesState.GetVariableWithName(name);
            GlobalVariables.Add(name, value);
            Debug.Log("Initialized global dialogue variable: " + name + " = " + value);
        }
    }

    public void SaveVariables() 
    {
        if (_globalVariablesStory != null) 
        {
            // Load the current state of all of our variables to the globals story
            VariablesToStory(_globalVariablesStory);
            // NOTE: eventually, you'd want to replace this with an actual save/load method
            // rather than using PlayerPrefs.
            PlayerPrefs.SetString(SaveVariablesKey, _globalVariablesStory.state.ToJson());
        }
    }

    public void StartListening(Story story) 
    {
        // it's important that VariablesToStory is before assigning the listener!
        VariablesToStory(story);
        story.variablesState.variableChangedEvent += VariableChanged;
    }

    public void StopListening(Story story) 
    {
        story.variablesState.variableChangedEvent -= VariableChanged;
    }

    private void VariableChanged(string name, Ink.Runtime.Object value) 
    {
        // only maintain variables that were initialized from the globals ink file
        if (GlobalVariables.ContainsKey(name)) 
        {
            GlobalVariables.Remove(name);
            GlobalVariables.Add(name, value);
        }
    }

    private void VariablesToStory(Story story) 
    {
        foreach(KeyValuePair<string, Ink.Runtime.Object> variable in GlobalVariables) 
        {
            story.variablesState.SetGlobal(variable.Key, variable.Value);
        }
    }

}

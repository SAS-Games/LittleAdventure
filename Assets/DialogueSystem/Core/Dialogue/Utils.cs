using UnityEngine;
using Debug = SAS.Debug;

namespace SAS.DialogueSystem
{
    public static class Utils
    {
        public static Ink.Runtime.Object GetVariableState(this DialogueGlobalVariables dialogueGlobalVariables, string variableName)
        {
            Ink.Runtime.Object variableValue = null;
            dialogueGlobalVariables.GlobalVariables.TryGetValue(variableName, out variableValue);
            if (variableValue == null)
            {
                Debug.LogWarning("Ink Variable was found to be null: " + variableName);
            }

            return variableValue;
        }

        public static bool GetTagKeyValue(string tag, out string tagKey, out string tagValue)
        {
            string[] splitTag = tag.Split(new char[] { ':' }, 2);
            if (splitTag.Length != 2)
            {
                Debug.LogError("Tag could not be appropriately parsed: " + tag);
                tagKey = string.Empty;
                tagValue = string.Empty;   
                return false;
            }

             tagKey = splitTag[0].Trim();
             tagValue = splitTag[1].Trim();
            return true;
        }
    }
}

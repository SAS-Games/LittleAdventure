using Ink.Runtime;

namespace SAS.DialogueSystem
{
    public static class Utils
    {
        public static Object GetVariableState(this DialogueGlobalVariables dialogueGlobalVariables, string variableName)
        {
            Object variableValue = null;
            dialogueGlobalVariables.GlobalVariables.TryGetValue(variableName, out variableValue);
            if (variableValue == null)
            {
                Debug.LogWarning("Ink Variable was found to be null: " + variableName);
            }

            return variableValue;
        }

        public static bool GetTagKeyValue(string tag, out string tagKey, out string tagValue)
        {
            tagKey = string.Empty;
            tagValue = string.Empty;

            if (string.IsNullOrWhiteSpace(tag))
                return false;

            tag = tag.Trim();
            if (tag.StartsWith("#"))
                tag = tag.Substring(1).Trim();

            if (string.IsNullOrWhiteSpace(tag))
                return false;

            string[] splitTag = tag.Split(new char[] { ':' }, 2);
            tagKey = splitTag[0].Trim();
            if (string.IsNullOrWhiteSpace(tagKey))
                return false;

            if (splitTag.Length == 1)
            {
                tagValue = string.Empty;
                return true;
            }

             tagValue = splitTag[1].Trim();
            return true;
        }
    }
}

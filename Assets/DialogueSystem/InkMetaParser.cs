using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInkMetaParser
{
    Dictionary<string, string> Parse(string input);
}

public class InkMetaParser : MonoBehaviour, IInkMetaParser
{
    Dictionary<string, string> IInkMetaParser.Parse(string input)
    {
        Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

        // Split the string by ","
        string[] parts = input.Split(new char[] { ',' }, StringSplitOptions.None);

        foreach (string part in parts)
        {
            // Split each part by "::"
            string[] keyValue = part.Split(new string[] { "::" }, StringSplitOptions.None);
            if (keyValue.Length == 2)
            {
                string key = keyValue[0].Trim();
                string value = keyValue[1].Trim();
                keyValuePairs[key] = value;
            }
        }

        return keyValuePairs;
    }
}
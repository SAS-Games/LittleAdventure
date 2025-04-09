#if UNITY_EDITOR
using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

[CustomEditor(typeof(JenkinsBuildConfig))]
public class JenkinsBuildConfigEditor : Editor
{
    private string[] streamOptions;
    private bool streamsLoaded = false;

    public override void OnInspectorGUI()
    {
        JenkinsBuildConfig config = (JenkinsBuildConfig)target;

        // Draw the default fields
        DrawDefaultInspector();

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Perforce Stream Selection", EditorStyles.boldLabel);

        if (GUILayout.Button("Refresh Streams"))
        {
            config.availableStreams = PerforceHelper.GetAllStreams();
            if (config.availableStreams.Count > 0)
                config.selectedStreamIndex = 0;
            EditorUtility.SetDirty(config); // Save to asset
        }

        if (config.availableStreams != null && config.availableStreams.Count > 0)
        {
            config.selectedStreamIndex = EditorGUILayout.Popup("Select Stream", config.selectedStreamIndex, config.availableStreams.ToArray());
            EditorUtility.SetDirty(config);
        }
        else
        {
            EditorGUILayout.HelpBox("No streams loaded. Click 'Refresh Streams'.", MessageType.Info);
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Trigger Jenkins Build"))
        {
            TriggerBuild(config);
        }
    }

    private void TriggerBuild(JenkinsBuildConfig config)
    {
        string selectedStream = null;

        if (config.selectedStreamIndex >= 0 && config.selectedStreamIndex < config.availableStreams.Count)
        {
            selectedStream = config.availableStreams[config.selectedStreamIndex];
        }

        if (string.IsNullOrEmpty(selectedStream))
        {
            selectedStream = PerforceHelper.GetCurrentStream(); // Fallback to current stream
        }

        WWWForm form = new WWWForm();
        form.AddField("PLATFORM", config.platform.ToString());
        form.AddField("STREAM", selectedStream);

        UnityWebRequest req = UnityWebRequest.Post(config.jenkinsUrl, form);
        string auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{config.username}:{config.apiToken}"));
        req.SetRequestHeader("Authorization", "Basic " + auth);

        var op = req.SendWebRequest();

        EditorApplication.update += () =>
        {
            if (op.isDone)
            {
                if (req.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Build triggered successfully.");
                }
                else
                {
                    Debug.LogError("Failed to trigger build: " + req.error);
                }

                EditorApplication.update -= null; // fix: change this line below
            }
        };
    }
}
#endif

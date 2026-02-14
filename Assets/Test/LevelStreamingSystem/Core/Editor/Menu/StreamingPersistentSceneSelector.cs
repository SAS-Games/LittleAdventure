using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class StreamingPersistentSceneSelector : EditorWindow
{
    private List<string> scenes;
    private Vector2 scroll;

    [MenuItem("Tools/Streaming/Persistent Scenes")]
    private static void Open()
    {
        var window = GetWindow<StreamingPersistentSceneSelector>(true, "Persistent Scenes");
        window.minSize = new Vector2(320, 250);
        window.Refresh();
    }

    private void Refresh()
    {
        scenes = StreamingPersistentSceneMenu.FindPersistentScenes();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(8);

        if (GUILayout.Button("Refresh"))
            Refresh();

        EditorGUILayout.Space(10);

        if (scenes == null || scenes.Count == 0)
        {
            EditorGUILayout.HelpBox("No Persistent Scenes Found.", MessageType.Info);
            return;
        }

        scroll = EditorGUILayout.BeginScrollView(scroll);

        foreach (var scenePath in scenes)
        {
            string sceneName = Path.GetFileNameWithoutExtension(scenePath);

            if (GUILayout.Button(sceneName, GUILayout.Height(32)))
            {
                StreamingPersistentSceneMenu.LoadPersistentScene(scenePath);
                Close();
            }
        }

        EditorGUILayout.EndScrollView();
    }
}
using Ink.UnityIntegration;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SAS.DialogueSystem.EditorTools
{
    internal static class DialogueInkCompileService
    {
        public static void CompileInk(string assetPath)
        {
            var inkAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(assetPath);
            if (inkAsset == null)
            {
                Debug.LogWarning($"Ink asset was written but could not be loaded: {assetPath}");
                return;
            }

            InkLibrary.CreateOrReadUpdatedInkFiles(new List<string> { assetPath });
            InkLibrary.RebuildInkFileConnections();

            var inkFile = InkLibrary.GetInkFileWithFile(inkAsset, true);
            if (inkFile == null)
            {
                Debug.LogWarning($"Ink file was not found in the Ink Library: {assetPath}");
                Selection.activeObject = inkAsset;
                return;
            }

            InkCompiler.CompileInk(new[] { inkFile }, true, () =>
            {
                inkFile.FindCompiledJSONAsset();
                Selection.activeObject = inkFile.jsonAsset != null ? inkFile.jsonAsset : inkAsset;
                EditorGUIUtility.PingObject(Selection.activeObject);
            });
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace SAS.DialogueSystem.EditorTools
{
    internal static class DialogueStoryAssetService
    {
        public static DialogueStoryDraft CreateDraftAsset()
        {
            var path = EditorUtility.SaveFilePanelInProject(
                "Create Dialogue Story Draft",
                "DialogueStoryDraft",
                "asset",
                "Choose where to save the dialogue draft.",
                DialogueStoryDraft.DefaultInkFolder);

            if (string.IsNullOrEmpty(path))
                return null;

            var draft = ScriptableObject.CreateInstance<DialogueStoryDraft>();
            draft.outputFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(DialogueStoryDraft.DefaultInkFolder);
            draft.outputFileName = Path.GetFileNameWithoutExtension(path).Replace("Draft", string.Empty);
            draft.startKnot = "start";
            draft.sections.Add(new DialogueStorySection
            {
                sectionType = DialogueStorySectionType.Knot,
                knotName = "start",
                entries = new List<DialogueStoryEntry>
                {
                    DialogueStoryValidator.CreateEntry(DialogueStoryEntryType.Line)
                }
            });

            AssetDatabase.CreateAsset(draft, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = draft;
            return draft;
        }

        public static DialogueStoryDraft ImportInkFile(string absolutePath)
        {
            if (!File.Exists(absolutePath))
            {
                EditorUtility.DisplayDialog("Ink Not Found", $"Could not find Ink file:\n{absolutePath}", "OK");
                return null;
            }

            var inkName = Path.GetFileNameWithoutExtension(absolutePath);
            var defaultFolder = GetAssetFolderPathForAbsolutePath(absolutePath) ?? DialogueStoryDraft.DefaultInkFolder;
            var draftPath = EditorUtility.SaveFilePanelInProject(
                "Create Dialogue Story Draft From Ink",
                $"{inkName}Draft",
                "asset",
                "Choose where to save the imported dialogue draft.",
                defaultFolder);

            if (string.IsNullOrEmpty(draftPath))
                return null;

            var draft = ScriptableObject.CreateInstance<DialogueStoryDraft>();
            draft.outputFileName = inkName;
            draft.outputFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(defaultFolder);
            draft.sections = new List<DialogueStorySection>();
            DialogueInkImporter.ImportIntoDraft(draft, File.ReadAllText(absolutePath, Encoding.UTF8));

            if (draft.sections.Count == 0)
                draft.sections.Add(new DialogueStorySection { knotName = "start" });

            if (string.IsNullOrWhiteSpace(draft.startKnot) && draft.sections.Count > 0)
                draft.startKnot = draft.sections[0].knotName;

            AssetDatabase.CreateAsset(draft, draftPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = draft;
            EditorGUIUtility.PingObject(draft);
            return draft;
        }

        public static void SaveInk(DialogueStoryDraft draft, bool forceCompile)
        {
            if (draft == null)
                return;

            DialogueStoryValidator.EnsureDraftShape(draft);
            var hierarchyErrors = DialogueStoryValidator.GetSectionHierarchyErrors(draft);
            if (hierarchyErrors.Count > 0)
            {
                EditorUtility.DisplayDialog("Invalid Story Structure", string.Join("\n", hierarchyErrors), "OK");
                return;
            }

            var folder = GetOutputFolderPath(draft);
            if (!AssetDatabase.IsValidFolder(folder))
            {
                EditorUtility.DisplayDialog("Invalid Output Folder", $"Output folder does not exist:\n{folder}", "OK");
                return;
            }

            var assetPath = $"{folder}/{InkSanitizer.SanitizeFileName(draft.outputFileName)}.ink";
            var absolutePath = Path.GetFullPath(assetPath);
            File.WriteAllText(absolutePath, DialogueInkBuilder.Build(draft), new UTF8Encoding(false));

            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (forceCompile || draft.compileOnSave)
                DialogueInkCompileService.CompileInk(assetPath);
            else
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<DefaultAsset>(assetPath);
        }

        public static void PingGeneratedInk(DialogueStoryDraft draft)
        {
            var asset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(GetInkAssetPath(draft));
            if (asset == null)
            {
                EditorUtility.DisplayDialog("Ink Not Found", "Save the draft once before pinging the generated Ink file.", "OK");
                return;
            }

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        public static string GetInkAssetPath(DialogueStoryDraft draft)
        {
            if (draft == null)
                return string.Empty;

            return $"{GetOutputFolderPath(draft)}/{InkSanitizer.SanitizeFileName(draft.outputFileName)}.ink";
        }

        public static string GetOutputFolderPath(DialogueStoryDraft draft)
        {
            if (draft.outputFolder != null)
            {
                var folderPath = AssetDatabase.GetAssetPath(draft.outputFolder);
                if (AssetDatabase.IsValidFolder(folderPath))
                    return folderPath;
            }

            return DialogueStoryDraft.DefaultInkFolder;
        }

        private static string GetAssetFolderPathForAbsolutePath(string absolutePath)
        {
            var assetPath = AbsoluteToAssetPath(absolutePath);
            if (string.IsNullOrWhiteSpace(assetPath))
                return null;

            if (AssetDatabase.IsValidFolder(assetPath))
                return assetPath;

            var folder = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            return !string.IsNullOrWhiteSpace(folder) && AssetDatabase.IsValidFolder(folder)
                ? folder
                : null;
        }

        private static string AbsoluteToAssetPath(string absolutePath)
        {
            if (string.IsNullOrWhiteSpace(absolutePath))
                return string.Empty;

            var fullPath = Path.GetFullPath(absolutePath).Replace('\\', '/');
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."))
                .Replace('\\', '/')
                .TrimEnd('/');

            if (!fullPath.StartsWith(projectRoot + "/", StringComparison.OrdinalIgnoreCase))
                return string.Empty;

            return fullPath.Substring(projectRoot.Length + 1);
        }
    }
}

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SAS.DialogueSystem.EditorTools
{
    public class DialogueStoryWriterWindow : EditorWindow
    {
        [SerializeField] private DialogueStoryDraft _draft;
        private Vector2 _sectionScroll;
        private Vector2 _mainScroll;
        private Vector2 _previewScroll;
        private Vector2 _playerScroll;
        private int _selectedSection;
        private bool _showPreview = true;
        private bool _showSettings = true;
        private readonly DialogueStoryPreviewPlayer _previewPlayer = new DialogueStoryPreviewPlayer();

        [MenuItem("Tools/Dialogue/Story Writer")]
        public static void Open()
        {
            Open(null);
        }

        [MenuItem("Assets/Open Dialogue Story Writer", true)]
        public static bool CanOpenSelectedDraft()
        {
            return Selection.activeObject is DialogueStoryDraft;
        }

        [MenuItem("Assets/Open Dialogue Story Writer")]
        public static void OpenSelectedDraft()
        {
            Open(Selection.activeObject as DialogueStoryDraft);
        }

        [MenuItem("Assets/Import Ink Into Dialogue Story Writer", true)]
        public static bool CanImportSelectedInk()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return !string.IsNullOrEmpty(path) && Path.GetExtension(path).Equals(".ink", StringComparison.OrdinalIgnoreCase);
        }

        [MenuItem("Assets/Import Ink Into Dialogue Story Writer")]
        public static void ImportSelectedInk()
        {
            var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            var absolutePath = Path.GetFullPath(assetPath);
            var window = GetWindow<DialogueStoryWriterWindow>();
            window.ConfigureWindow();
            window.ImportInkFile(absolutePath);
            window.Show();
        }

        private static void Open(DialogueStoryDraft draft)
        {
            var window = GetWindow<DialogueStoryWriterWindow>();
            window.ConfigureWindow();
            if (draft != null)
                window._draft = draft;
            window.Show();
        }

        private void OnEnable()
        {
            ConfigureWindow();
            if (_draft == null)
                _draft = Selection.activeObject as DialogueStoryDraft;
        }

        private void OnGUI()
        {
            DialogueStoryToolbarGUI.Draw(
                ref _draft,
                ref _selectedSection,
                CreateDraftAsset,
                ImportInkFromFilePanel,
                () => DialogueStoryAssetService.SaveInk(_draft, false),
                () => DialogueStoryAssetService.SaveInk(_draft, true),
                () => DialogueStoryAssetService.PingGeneratedInk(_draft));

            if (_draft == null)
            {
                DialogueStoryToolbarGUI.DrawEmptyState(CreateDraftAsset);
                return;
            }

            DialogueStoryValidator.EnsureDraftShape(_draft);

            EditorGUI.BeginChangeCheck();

            using (new EditorGUILayout.HorizontalScope())
            {
                DialogueStorySidebarGUI.Draw(_draft, ref _sectionScroll, ref _selectedSection);
                DialogueStorySectionGUI.DrawSectionInspector(
                    _draft,
                    ref _selectedSection,
                    ref _mainScroll,
                    ref _previewScroll,
                    ref _showSettings,
                    ref _showPreview);
            }

            DialogueStoryPreviewPlayerGUI.Draw(_draft, _previewPlayer, ref _playerScroll);

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(_draft);
        }

        private void ConfigureWindow()
        {
            titleContent = new GUIContent("Dialogue Story Writer");
            minSize = new Vector2(760f, 520f);
        }

        private void CreateDraftAsset()
        {
            var draft = DialogueStoryAssetService.CreateDraftAsset();
            if (draft == null)
                return;

            _draft = draft;
            _selectedSection = 0;
        }

        private void ImportInkFromFilePanel()
        {
            var startFolder = Path.GetFullPath(DialogueStoryDraft.DefaultInkFolder);
            var absolutePath = EditorUtility.OpenFilePanel("Import Ink", startFolder, "ink");
            if (!string.IsNullOrEmpty(absolutePath))
                ImportInkFile(absolutePath);
        }

        private void ImportInkFile(string absolutePath)
        {
            var draft = DialogueStoryAssetService.ImportInkFile(absolutePath);
            if (draft == null)
                return;

            _draft = draft;
            _selectedSection = 0;
        }
    }
}

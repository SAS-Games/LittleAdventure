using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SAS.WorldStreaming.Editor
{
    public sealed class WorldStreamingAuthoringWindow : EditorWindow
    {
        private const string MenuPath = "Tools/SAS/World Streaming Authoring";

        [SerializeField] private WorldStreamingProfile m_Profile;
        [SerializeField] private int m_SelectedRegionIndex = -1;
        [SerializeField] private bool m_EditLayoutInSceneView;

        private SerializedObject m_ProfileSerializedObject;
        private Vector2 m_Scroll;
        private Vector2 m_RegionScroll;
        private readonly UniformGridRegionProvider m_GridProvider = new();
        private readonly WorldStreamingSceneViewDrawer m_SceneViewDrawer = new();
        private List<WorldStreamingValidationIssue> m_ValidationIssues = new();

        [MenuItem(MenuPath)]
        private static void Open()
        {
            var window = GetWindow<WorldStreamingAuthoringWindow>();
            window.titleContent = new GUIContent("World Streaming");
            window.minSize = new Vector2(440f, 560f);
            window.Show();
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += DuringSceneGui;
            BindProfile();
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= DuringSceneGui;
        }

        private void OnGUI()
        {
            DrawProfileSelector();
            if (m_Profile == null)
            {
                EditorGUILayout.HelpBox(
                    "Create or select a World Streaming Profile to begin. Phase 1 only previews " +
                    "the partition; it does not move, copy, or delete source objects.",
                    MessageType.Info);
                return;
            }

            EnsureSerializedObject();
            m_ProfileSerializedObject.UpdateIfRequiredOrScript();

            m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
            DrawProfileSection();
            DrawWorldLayoutSection();
            DrawClassificationDefaultsSection();
            DrawRegionBrowser();
            DrawAnalysisAndGenerationStatus();
            DrawValidationIssues();
            EditorGUILayout.EndScrollView();

            if (m_ProfileSerializedObject.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(m_Profile);
                SceneView.RepaintAll();
            }
        }

        private void DrawProfileSelector()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Profile", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            WorldStreamingProfile selected = EditorGUILayout.ObjectField(
                "World Streaming Profile",
                m_Profile,
                typeof(WorldStreamingProfile),
                false) as WorldStreamingProfile;
            if (EditorGUI.EndChangeCheck())
            {
                m_Profile = selected;
                m_SelectedRegionIndex = -1;
                BindProfile();
            }

            if (GUILayout.Button("Create Profile"))
                CreateProfileAsset();
        }

        private void DrawProfileSection()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("1. Source World", EditorStyles.boldLabel);
            DrawProperty("m_SourceAuthoringScene");
            DrawProperty("m_OutputRootFolder");
            DrawProperty("m_GeneratedSceneNamePrefix");
            DrawProperty("m_CreatePersistentScene");
            if (FindProperty("m_CreatePersistentScene").boolValue)
                DrawProperty("m_PersistentSceneName");
            DrawProperty("m_AuthoringDatabase");

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Open Source Scene"))
                    OpenSourceScene();
                if (GUILayout.Button("Create Database"))
                    CreateDatabaseAsset();
                if (GUILayout.Button("Validate Profile"))
                    ValidateProfile();
            }
        }

        private void DrawWorldLayoutSection()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("2. World Layout", EditorStyles.boldLabel);

            SerializedProperty mode = FindProperty("m_RegionGenerationMode");
            EditorGUILayout.PropertyField(mode);
            var selectedMode = (StreamingRegionGenerationMode)mode.enumValueIndex;

            if (selectedMode == StreamingRegionGenerationMode.UniformGrid)
            {
                DrawProperty("m_WorldOrigin");
                DrawProperty("m_CellSize");
                DrawProperty("m_CellCount");
                DrawProperty("m_BoundsPadding",
                    "Load Bounds Padding",
                    "Padding is applied to load bounds, not ownership bounds.");
                DrawProperty("m_UseXZPlaneOnly");
                if (FindProperty("m_UseXZPlaneOnly").boolValue)
                {
                    DrawProperty("m_MinimumWorldY");
                    DrawProperty("m_MaximumWorldY");
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Generate Grid Preview"))
                        GenerateGridPreview();
                    if (GUILayout.Button("Clear Grid Preview"))
                        ClearGridPreview();
                }
            }
            else
            {
                if (GUILayout.Button("Add Manual Region"))
                    AddManualRegion();
            }

            DrawProperty("m_ShowSceneViewVisualization");
            m_EditLayoutInSceneView = EditorGUILayout.Toggle(
                new GUIContent(
                    "Edit Layout In Scene View",
                    selectedMode == StreamingRegionGenerationMode.UniformGrid
                        ? "Move the grid origin with a position handle."
                        : "Move and resize the selected manual region."),
                m_EditLayoutInSceneView);
        }

        private void DrawClassificationDefaultsSection()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("3. Classification Defaults", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "These settings are persisted now so Phase 2 classification remains compatible " +
                "with profiles created during Phase 1.",
                MessageType.Info);

            DrawProperty("m_PersistentRootObjects", includeChildren: true);
            DrawProperty("m_PersistentLayers");
            DrawProperty("m_IncludeChildrenOfPersistentLayerObjects");
            DrawProperty("m_PersistentUnityTags", includeChildren: true);
            DrawProperty("m_SharedScenes", includeChildren: true);
            DrawProperty("m_BoundsFallbackMode");
            if ((BoundsFallbackMode)FindProperty("m_BoundsFallbackMode").enumValueIndex ==
                BoundsFallbackMode.FixedSizeBounds)
            {
                DrawProperty("m_FixedFallbackBoundsSize");
            }

            DrawProperty("m_AssignmentStrategy");
            DrawProperty("m_CrossRegionPolicy");
            DrawProperty("m_HierarchyPolicy");
            DrawProperty("m_NeighborMode");
            DrawProperty("m_BlockGenerationOnUnsafeCrossSceneReferences");
        }

        private void DrawRegionBrowser()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(
                $"4. Region Browser ({m_Profile.Regions.Count})",
                EditorStyles.boldLabel);

            if (m_Profile.Regions.Count == 0)
            {
                EditorGUILayout.HelpBox("No region definitions have been created.", MessageType.Info);
                return;
            }

            m_SelectedRegionIndex = Mathf.Clamp(
                m_SelectedRegionIndex,
                -1,
                m_Profile.Regions.Count - 1);
            m_RegionScroll = EditorGUILayout.BeginScrollView(
                m_RegionScroll,
                GUILayout.MinHeight(80f),
                GUILayout.MaxHeight(220f));

            for (int i = 0; i < m_Profile.Regions.Count; i++)
            {
                StreamingRegionDefinition region = m_Profile.Regions[i];
                string label = region == null
                    ? $"{i}: <null>"
                    : $"{region.DisplayName}  |  {region.SceneName}.unity";
                bool selected = i == m_SelectedRegionIndex;
                if (GUILayout.Toggle(selected, label, "Button") && !selected)
                {
                    m_SelectedRegionIndex = i;
                    SceneView.RepaintAll();
                }
            }

            EditorGUILayout.EndScrollView();

            if (m_SelectedRegionIndex < 0 ||
                m_SelectedRegionIndex >= m_Profile.Regions.Count)
                return;

            StreamingRegionDefinition selectedRegion =
                m_Profile.Regions[m_SelectedRegionIndex];
            if (selectedRegion == null)
                return;

            EditorGUILayout.LabelField("Region ID", selectedRegion.RegionId);
            EditorGUILayout.LabelField("Bounds", selectedRegion.Bounds.ToString());
            EditorGUILayout.LabelField(
                "Assigned Objects",
                (m_Profile.AuthoringDatabase == null
                    ? 0
                    : m_Profile.AuthoringDatabase.GetAssignedObjectCount(selectedRegion.RegionId)).ToString());

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Frame Region"))
                    FrameSelectedRegion();

                using (new EditorGUI.DisabledScope(selectedRegion.GeneratedFromGrid))
                {
                    if (GUILayout.Button("Duplicate"))
                        DuplicateSelectedRegion();
                    if (GUILayout.Button("Delete"))
                        DeleteSelectedRegion();
                }
            }

            if (!selectedRegion.GeneratedFromGrid)
            {
                SerializedProperty regions = FindProperty("m_ManualRegions");
                if (m_SelectedRegionIndex < regions.arraySize)
                {
                    EditorGUILayout.PropertyField(
                        regions.GetArrayElementAtIndex(m_SelectedRegionIndex),
                        new GUIContent("Manual Region"),
                        true);
                }
            }
        }

        private static void DrawAnalysisAndGenerationStatus()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("5. Analysis", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "World scanning, bounds calculation, assignment preview, and cross-region " +
                "detection are Phase 2. No source objects are modified in Phase 1.",
                MessageType.Info);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("6. Generation", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Scene generation is intentionally disabled until analysis, validation, and a " +
                "dry-run preview are implemented. This prevents destructive map splitting.",
                MessageType.Warning);
        }

        private void DrawValidationIssues()
        {
            if (m_ValidationIssues == null || m_ValidationIssues.Count == 0)
                return;

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
            foreach (WorldStreamingValidationIssue issue in m_ValidationIssues)
            {
                MessageType type = issue.Severity switch
                {
                    ValidationSeverity.Error => MessageType.Error,
                    ValidationSeverity.Warning => MessageType.Warning,
                    _ => MessageType.Info
                };
                EditorGUILayout.HelpBox($"[{issue.Code}] {issue.Message}", type);
            }
        }

        private void DuringSceneGui(SceneView sceneView)
        {
            if (m_Profile == null || !m_Profile.ShowSceneViewVisualization)
                return;

            string sourcePath = m_Profile.SourceAuthoringScene == null
                ? string.Empty
                : AssetDatabase.GetAssetPath(m_Profile.SourceAuthoringScene);
            Scene activeScene = SceneManager.GetActiveScene();
            if (string.IsNullOrWhiteSpace(sourcePath) ||
                !string.Equals(activeScene.path, sourcePath, StringComparison.Ordinal))
                return;

            int selection = m_SceneViewDrawer.Draw(
                m_Profile,
                m_SelectedRegionIndex,
                m_EditLayoutInSceneView);
            if (selection != m_SelectedRegionIndex)
            {
                m_SelectedRegionIndex = selection;
                Repaint();
            }
        }

        private void GenerateGridPreview()
        {
            ApplyPendingProperties();
            try
            {
                Undo.RecordObject(m_Profile, "Generate World Streaming Grid");
                m_Profile.ReplaceRegions(m_GridProvider.Generate(m_Profile));
                EditorUtility.SetDirty(m_Profile);
                m_SelectedRegionIndex = m_Profile.Regions.Count > 0 ? 0 : -1;
                ValidateProfile();
                RefreshAfterDirectEdit();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, m_Profile);
                EditorUtility.DisplayDialog(
                    "Grid Preview Failed",
                    exception.Message,
                    "OK");
            }
        }

        private void ClearGridPreview()
        {
            ApplyPendingProperties();
            Undo.RecordObject(m_Profile, "Clear World Streaming Grid");
            m_Profile.ClearGeneratedRegions();
            EditorUtility.SetDirty(m_Profile);
            m_SelectedRegionIndex = -1;
            ValidateProfile();
            RefreshAfterDirectEdit();
        }

        private void AddManualRegion()
        {
            ApplyPendingProperties();
            Undo.RecordObject(m_Profile, "Add World Streaming Region");
            m_SelectedRegionIndex = m_Profile.AddManualRegion();
            EditorUtility.SetDirty(m_Profile);
            RefreshAfterDirectEdit();
        }

        private void DuplicateSelectedRegion()
        {
            ApplyPendingProperties();
            Undo.RecordObject(m_Profile, "Duplicate World Streaming Region");
            int duplicate = m_Profile.DuplicateManualRegion(m_SelectedRegionIndex);
            if (duplicate >= 0)
                m_SelectedRegionIndex = duplicate;
            EditorUtility.SetDirty(m_Profile);
            RefreshAfterDirectEdit();
        }

        private void DeleteSelectedRegion()
        {
            ApplyPendingProperties();
            Undo.RecordObject(m_Profile, "Delete World Streaming Region");
            if (m_Profile.RemoveManualRegion(m_SelectedRegionIndex))
                m_SelectedRegionIndex = Mathf.Min(
                    m_SelectedRegionIndex,
                    m_Profile.Regions.Count - 1);
            EditorUtility.SetDirty(m_Profile);
            RefreshAfterDirectEdit();
        }

        private void FrameSelectedRegion()
        {
            if (m_SelectedRegionIndex < 0 ||
                m_SelectedRegionIndex >= m_Profile.Regions.Count ||
                m_Profile.Regions[m_SelectedRegionIndex] == null)
                return;

            SceneView.lastActiveSceneView?.Frame(
                m_Profile.Regions[m_SelectedRegionIndex].Bounds,
                false);
        }

        private void OpenSourceScene()
        {
            ApplyPendingProperties();
            if (m_Profile.SourceAuthoringScene == null)
            {
                EditorUtility.DisplayDialog(
                    "Missing Source Scene",
                    "Assign the large source authoring scene first.",
                    "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            string path = AssetDatabase.GetAssetPath(m_Profile.SourceAuthoringScene);
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        }

        private void CreateProfileAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create World Streaming Profile",
                "WorldStreamingProfile",
                "asset",
                "Choose where to save the authoring profile.");
            if (string.IsNullOrWhiteSpace(path))
                return;

            var profile = CreateInstance<WorldStreamingProfile>();
            AssetDatabase.CreateAsset(profile, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = profile;
            m_Profile = profile;
            BindProfile();
        }

        private void CreateDatabaseAsset()
        {
            ApplyPendingProperties();
            string path = EditorUtility.SaveFilePanelInProject(
                "Create World Streaming Authoring Database",
                $"{m_Profile.name}_Database",
                "asset",
                "Choose where to save the regeneration database.");
            if (string.IsNullOrWhiteSpace(path))
                return;

            var database = CreateInstance<WorldStreamingAuthoringDatabase>();
            string sourcePath = m_Profile.SourceAuthoringScene == null
                ? string.Empty
                : AssetDatabase.GetAssetPath(m_Profile.SourceAuthoringScene);
            database.Initialize(m_Profile, sourcePath);
            AssetDatabase.CreateAsset(database, path);

            Undo.RecordObject(m_Profile, "Assign World Streaming Database");
            m_Profile.SetAuthoringDatabase(database);
            EditorUtility.SetDirty(m_Profile);
            AssetDatabase.SaveAssets();
            Selection.activeObject = database;
            RefreshAfterDirectEdit();
        }

        private void ValidateProfile()
        {
            ApplyPendingProperties();
            m_ValidationIssues = WorldStreamingProfileValidator.Validate(m_Profile);
            Repaint();
        }

        private void DrawProperty(
            string propertyName,
            bool includeChildren = false)
        {
            EditorGUILayout.PropertyField(FindProperty(propertyName), includeChildren);
        }

        private void DrawProperty(
            string propertyName,
            string label,
            string tooltip)
        {
            EditorGUILayout.PropertyField(
                FindProperty(propertyName),
                new GUIContent(label, tooltip));
        }

        private SerializedProperty FindProperty(string propertyName)
        {
            SerializedProperty property = m_ProfileSerializedObject.FindProperty(propertyName);
            if (property == null)
                throw new InvalidOperationException(
                    $"WorldStreamingProfile property '{propertyName}' was not found.");
            return property;
        }

        private void ApplyPendingProperties()
        {
            if (m_ProfileSerializedObject != null)
                m_ProfileSerializedObject.ApplyModifiedProperties();
        }

        private void RefreshAfterDirectEdit()
        {
            m_ProfileSerializedObject?.Update();
            SceneView.RepaintAll();
            Repaint();
        }

        private void BindProfile()
        {
            m_ProfileSerializedObject = m_Profile == null
                ? null
                : new SerializedObject(m_Profile);
            m_ValidationIssues = m_Profile == null
                ? new List<WorldStreamingValidationIssue>()
                : WorldStreamingProfileValidator.Validate(m_Profile);
            SceneView.RepaintAll();
            Repaint();
        }

        private void EnsureSerializedObject()
        {
            if (m_ProfileSerializedObject == null ||
                m_ProfileSerializedObject.targetObject != m_Profile)
                BindProfile();
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LevelStreaming.Editor
{
    public sealed class LevelStreamingEditorWindow : EditorWindow
    {
        private enum Page
        {
            Setup,
            Regions,
            Connections,
            Runtime
        }

        private enum RegionFilter
        {
            All,
            Scene,
            Prefab,
            AddressableScene
        }

        private static readonly string[] PageNames = { "Setup", "Regions", "Connections", "Runtime" };

        [SerializeField] private RegionManager m_Manager;
        [SerializeField] private Page m_Page = Page.Regions;
        [SerializeField] private int m_SelectedRegion = -1;
        [SerializeField] private int m_SelectedPortal = -1;
        [SerializeField] private string m_Search = string.Empty;
        [SerializeField] private RegionFilter m_Filter;
        [SerializeField] private bool m_EditRegionBounds = true;
        [SerializeField] private bool m_EditPortalBounds = true;
        [SerializeField] private bool m_ShowAllLabels = true;
        [SerializeField] private bool m_ShowProviderInspector = true;
        [SerializeField] private MonoBehaviour m_InspectedProvider;

        private SerializedObject _serializedManager;
        private Vector2 _leftScroll;
        private Vector2 _rightScroll;
        private Vector2 _runtimeScroll;
        private Vector2 _validationScroll;
        private readonly BoxBoundsHandle _regionHandle = new();
        private readonly BoxBoundsHandle _portalHandle = new();
        private List<StreamingEditorIssue> _issues = new();
        private UnityEditor.Editor _providerEditor;
        private double _nextRuntimeRepaint;

        [MenuItem("Tools/Streaming/Level Streaming Editor", priority = -100)]
        public static void Open()
        {
            RegionManager manager = FindManagerFromSelection() ?? FindSingleLoadedManager();
            Open(manager);
        }

        public static void Open(RegionManager manager)
        {
            var window = GetWindow<LevelStreamingEditorWindow>();
            window.titleContent = new GUIContent("Level Streaming", EditorGUIUtility.IconContent("SceneSet Icon").image);
            window.minSize = new Vector2(780f, 480f);
            window.SetManager(manager);
            window.Show();
            window.Focus();
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += DuringSceneGui;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            if (m_Manager == null)
                m_Manager = FindManagerFromSelection() ?? FindSingleLoadedManager();
            BindManager();
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= DuringSceneGui;
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            DestroyProviderEditor();
        }

        private void Update()
        {
            if (m_Page != Page.Runtime || !Application.isPlaying || EditorApplication.timeSinceStartup < _nextRuntimeRepaint)
                return;

            _nextRuntimeRepaint = EditorApplication.timeSinceStartup + 0.2d;
            Repaint();
            SceneView.RepaintAll();
        }

        private void OnGUI()
        {
            DrawToolbar();
            if (m_Manager == null)
            {
                DrawNoManagerState();
                return;
            }

            EnsureSerializedManager();
            _serializedManager.UpdateIfRequiredOrScript();

            switch (m_Page)
            {
                case Page.Setup:
                    DrawSetupPage();
                    break;
                case Page.Regions:
                    DrawRegionsPage();
                    break;
                case Page.Connections:
                    DrawConnectionsPage();
                    break;
                case Page.Runtime:
                    DrawRuntimePage();
                    break;
            }

            if (_serializedManager.ApplyModifiedProperties())
            {
                RebuildPortalCaches();
                RegionEditorCommands.MarkDirty(m_Manager);
                RefreshValidation();
                SceneView.RepaintAll();
            }

            DrawStatusBar();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUI.BeginChangeCheck();
                RegionManager manager = EditorGUILayout.ObjectField(
                    m_Manager,
                    typeof(RegionManager),
                    true,
                    GUILayout.MinWidth(180f)) as RegionManager;
                if (EditorGUI.EndChangeCheck())
                    SetManager(manager);

                if (GUILayout.Button("Find", EditorStyles.toolbarButton, GUILayout.Width(42f)))
                    ShowManagerMenu();
                if (m_Manager != null && GUILayout.Button("Select", EditorStyles.toolbarButton, GUILayout.Width(48f)))
                    Selection.activeObject = m_Manager;

                GUILayout.Space(8f);
                Page selected = (Page)GUILayout.Toolbar((int)m_Page, PageNames, EditorStyles.toolbarButton,
                    GUI.ToolbarButtonSize.Fixed);
                if (selected != m_Page)
                {
                    m_Page = selected;
                    SceneView.RepaintAll();
                }

                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Validate", EditorStyles.toolbarButton, GUILayout.Width(60f)))
                    RefreshValidation();
            }
        }

        private void DrawNoManagerState()
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox(
                "Open a persistent scene containing a RegionManager, then select it here.",
                MessageType.Info);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Find Loaded RegionManager", GUILayout.Width(220f), GUILayout.Height(30f)))
                    ShowManagerMenu();
                GUILayout.FlexibleSpace();
            }
            GUILayout.FlexibleSpace();
        }

        private void DrawSetupPage()
        {
            _rightScroll = EditorGUILayout.BeginScrollView(_rightScroll);

            DrawSectionTitle("Persistent Scene");
            EditorGUILayout.ObjectField("Region Manager", m_Manager, typeof(RegionManager), true);
            SerializedProperty strategy = _serializedManager.FindProperty("<RegionSelectionStrategy>k__BackingField");
            if (strategy != null)
                EditorGUILayout.PropertyField(strategy, new GUIContent("Selection Strategy"));

            DrawSectionTitle("Complete World Preview");
            EditorGUILayout.HelpBox(
                "Open every scene-backed region additively to inspect the complete world. " +
                "The persistent scene remains active, and unloading prompts you to save modified scenes.",
                MessageType.Info);
            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Load All Scenes", GUILayout.Height(28f)))
                        StreamingPersistentSceneMenu.LoadAllStreamingScenesForEditing(m_Manager);
                    if (GUILayout.Button("Frame Complete World", GUILayout.Height(28f)))
                        StreamingPersistentSceneMenu.FrameCompleteWorld(m_Manager);
                    if (GUILayout.Button("Unload Streaming Scenes", GUILayout.Height(28f)))
                        StreamingPersistentSceneMenu.UnloadAllStreamingScenesForEditing(m_Manager);
                }
            }

            RegionStreamingController controller = m_Manager.GetComponent<RegionStreamingController>();
            DrawSectionTitle("Streaming Controller");
            if (controller == null)
            {
                EditorGUILayout.HelpBox("RegionStreamingController is missing.", MessageType.Error);
                if (!Application.isPlaying && GUILayout.Button("Add RegionStreamingController"))
                {
                    Undo.AddComponent<RegionStreamingController>(m_Manager.gameObject);
                    RefreshValidation();
                }
            }
            else
            {
                var serializedController = new SerializedObject(controller);
                serializedController.Update();
                DrawPropertyIfPresent(serializedController, "m_StreamingLoader", "Streaming Loader");
                DrawPropertyIfPresent(serializedController, "m_UpdateInterval", "Update Interval");
                DrawPropertyIfPresent(serializedController, "m_LogStateChanges", "Log State Changes");
                DrawPropertyIfPresent(serializedController, "m_DrawStreamingBounds", "Draw Streaming Bounds");
                if (serializedController.ApplyModifiedProperties())
                {
                    EditorUtility.SetDirty(controller);
                    EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
                    RefreshValidation();
                }
            }

            DrawSectionTitle("Bounds Provider");
            List<MonoBehaviour> providers = StreamingEditorValidation.FindProviders(m_Manager.gameObject.scene);
            if (providers.Count == 0)
            {
                EditorGUILayout.HelpBox("No IStreamingBoundsProvider exists in this scene.", MessageType.Error);
                using (new EditorGUI.DisabledScope(Application.isPlaying))
                {
                    if (GUILayout.Button("Add Adaptive Provider To Scene Camera"))
                        AddAdaptiveProvider();
                }
            }
            else
            {
                foreach (MonoBehaviour provider in providers)
                {
                    using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        bool selected = m_InspectedProvider == provider;
                        if (GUILayout.Toggle(selected, provider.GetType().Name, "Button") && !selected)
                        {
                            m_InspectedProvider = provider;
                            DestroyProviderEditor();
                        }
                        EditorGUILayout.ObjectField(provider, typeof(MonoBehaviour), true, GUILayout.Width(180f));
                    }
                }

                if (m_InspectedProvider == null || !providers.Contains(m_InspectedProvider))
                    m_InspectedProvider = providers[0];

                m_ShowProviderInspector = EditorGUILayout.Foldout(
                    m_ShowProviderInspector,
                    "Provider Configuration",
                    true);
                if (m_ShowProviderInspector && m_InspectedProvider != null)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    UnityEditor.Editor.CreateCachedEditor(m_InspectedProvider, null, ref _providerEditor);
                    _providerEditor?.OnInspectorGUI();
                    EditorGUILayout.EndVertical();
                }
            }

            DrawSectionTitle("Validation");
            DrawValidationIssues(220f);
            EditorGUILayout.EndScrollView();
        }

        private void DrawRegionsPage()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(Mathf.Clamp(position.width * 0.34f, 250f, 360f))))
                    DrawRegionList();

                DrawVerticalSeparator();

                using (new EditorGUILayout.VerticalScope())
                    DrawSelectedRegionDetails();
            }
        }

        private void DrawRegionList()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                m_Search = GUILayout.TextField(m_Search ?? string.Empty,
                    GUI.skin.FindStyle("ToolbarSearchTextField"), GUILayout.MinWidth(80f));
                m_Filter = (RegionFilter)EditorGUILayout.EnumPopup(m_Filter, EditorStyles.toolbarPopup,
                    GUILayout.Width(105f));
            }

            _leftScroll = EditorGUILayout.BeginScrollView(_leftScroll);
            for (int i = 0; i < m_Manager.Regions.Count; i++)
            {
                RegionManager.Region region = m_Manager.Regions[i];
                if (!MatchesFilter(region))
                    continue;

                MessageType status = GetRegionStatus(region);
                GUIContent icon = status switch
                {
                    MessageType.Error => EditorGUIUtility.IconContent("console.erroricon.sml"),
                    MessageType.Warning => EditorGUIUtility.IconContent("console.warnicon.sml"),
                    _ => EditorGUIUtility.IconContent("TestPassed")
                };

                Rect row = EditorGUILayout.GetControlRect(false, 24f);
                bool selected = i == m_SelectedRegion;
                if (selected)
                    EditorGUI.DrawRect(row, new Color(0.18f, 0.42f, 0.7f, 0.35f));

                Rect iconRect = new(row.x + 4f, row.y + 4f, 16f, 16f);
                GUI.Label(iconRect, icon);
                Rect buttonRect = new(row.x + 24f, row.y, row.width - 24f, row.height);
                string name = region == null
                    ? $"{i}: <null>"
                    : $"{region.RegionName}   [{GetTypeLabel(region.Type)}]";
                if (GUI.Button(buttonRect, name, EditorStyles.label))
                    SelectRegion(i, true);
            }
            EditorGUILayout.EndScrollView();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add", GUILayout.Height(24f)))
                    ShowAddRegionMenu();

                using (new EditorGUI.DisabledScope(!HasSelectedRegion()))
                {
                    if (GUILayout.Button("Duplicate", GUILayout.Height(24f)))
                    {
                        ApplyPendingChanges();
                        SelectRegion(RegionEditorCommands.DuplicateRegion(m_Manager, m_SelectedRegion), true);
                        RebindAfterCommand();
                    }
                    if (GUILayout.Button("Delete", GUILayout.Height(24f)))
                    {
                        ApplyPendingChanges();
                        if (RegionEditorCommands.DeleteRegion(m_Manager, m_SelectedRegion))
                            SelectRegion(Mathf.Min(m_SelectedRegion, m_Manager.Regions.Count - 1), true);
                        RebindAfterCommand();
                    }
                }
            }
        }

        private void DrawSelectedRegionDetails()
        {
            if (!HasSelectedRegion())
            {
                EditorGUILayout.HelpBox("Select a region to edit its source, bounds, and policy.", MessageType.Info);
                return;
            }

            SerializedProperty region = GetSelectedRegionProperty();
            if (region == null)
                return;

            _rightScroll = EditorGUILayout.BeginScrollView(_rightScroll);
            using (new EditorGUILayout.HorizontalScope())
            {
                RegionManager.Region selectedRegion = m_Manager.Regions[m_SelectedRegion];
                string selectedName = string.IsNullOrWhiteSpace(selectedRegion?.RegionName)
                    ? $"Region {m_SelectedRegion + 1}"
                    : selectedRegion.RegionName;
                EditorGUILayout.LabelField($"Selected Region: {selectedName}", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                int previousRegion = FindAdjacentVisibleRegion(-1);
                int nextRegion = FindAdjacentVisibleRegion(1);
                using (new EditorGUI.DisabledScope(previousRegion < 0))
                {
                    if (GUILayout.Button(new GUIContent("Previous", "Select the previous visible region."), GUILayout.Width(70f)))
                        SelectAdjacentRegion(previousRegion);
                }
                using (new EditorGUI.DisabledScope(nextRegion < 0))
                {
                    if (GUILayout.Button(new GUIContent("Next", "Select the next visible region."), GUILayout.Width(52f)))
                        SelectAdjacentRegion(nextRegion);
                }
                if (GUILayout.Button("Frame", GUILayout.Width(55f)))
                    FrameRegion(m_SelectedRegion);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("List Order", GUILayout.Width(62f));
                using (new EditorGUI.DisabledScope(m_SelectedRegion <= 0))
                {
                    if (GUILayout.Button(new GUIContent("Move Up", "Move this region earlier in the serialized list.")))
                        MoveSelectedRegion(-1);
                }
                using (new EditorGUI.DisabledScope(m_SelectedRegion >= m_Manager.Regions.Count - 1))
                {
                    if (GUILayout.Button(new GUIContent("Move Down", "Move this region later in the serialized list.")))
                        MoveSelectedRegion(1);
                }
            }

            EditorGUILayout.PropertyField(region.FindPropertyRelative("regionName"), new GUIContent("Name"));
            SerializedProperty type = region.FindPropertyRelative("type");
            EditorGUILayout.PropertyField(type);
            RegionManager.RegionType regionType = (RegionManager.RegionType)type.enumValueIndex;
            SerializedProperty source = regionType switch
            {
                RegionManager.RegionType.Scene => region.FindPropertyRelative("sceneRef"),
                RegionManager.RegionType.Prefab => region.FindPropertyRelative("prefabRef"),
                RegionManager.RegionType.AddressableScene => region.FindPropertyRelative("addressableSceneRef"),
                _ => null
            };
            if (source != null)
                EditorGUILayout.PropertyField(source, new GUIContent("Content"), true);

            DrawSourceButtons(m_Manager.Regions[m_SelectedRegion]);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("World Bounds", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(region.FindPropertyRelative("cachedBounds"), GUIContent.none, true);
            using (new EditorGUILayout.HorizontalScope())
            {
                m_EditRegionBounds = GUILayout.Toggle(m_EditRegionBounds, "Edit In Scene View", "Button");
                if (GUILayout.Button("Fit From Asset"))
                    FitSelectedBounds();
                if (GUILayout.Button("Apply To Asset"))
                    ApplySelectedBounds();
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Streaming Policy", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(region.FindPropertyRelative("unloadStrategy"));
            int portalCount = region.FindPropertyRelative("portals")?.arraySize ?? 0;
            EditorGUILayout.LabelField("Portals", portalCount.ToString());
            if (portalCount > 0 && GUILayout.Button("Edit Connections"))
                m_Page = Page.Connections;

            MessageType status = GetRegionStatus(m_Manager.Regions[m_SelectedRegion]);
            if (status != MessageType.Info)
            {
                string message = status == MessageType.Error
                    ? "This region has an invalid name, source reference, or bounds."
                    : "This region has no unload strategy and will remain loaded once requested.";
                EditorGUILayout.HelpBox(message, status);
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawConnectionsPage()
        {
            if (m_Manager.Regions.Count == 0)
            {
                EditorGUILayout.HelpBox("Create a region before authoring connections.", MessageType.Info);
                return;
            }

            DrawRegionSelectionPopup();
            if (!HasSelectedRegion())
                return;

            SerializedProperty region = GetSelectedRegionProperty();
            SerializedProperty portals = region.FindPropertyRelative("portals");

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(280f)))
                {
                    EditorGUILayout.LabelField("Portals", EditorStyles.boldLabel);
                    _leftScroll = EditorGUILayout.BeginScrollView(_leftScroll);
                    for (int i = 0; i < portals.arraySize; i++)
                    {
                        SerializedProperty portal = portals.GetArrayElementAtIndex(i);
                        string targetName = RegionEditorCommands.FindPortalTargetProperty(portal)?.stringValue;
                        bool selected = i == m_SelectedPortal;
                        if (GUILayout.Toggle(selected,
                                $"Portal {i + 1}  →  {(string.IsNullOrWhiteSpace(targetName) ? "<none>" : targetName)}",
                                "Button") && !selected)
                        {
                            m_SelectedPortal = i;
                            SceneView.RepaintAll();
                        }
                    }
                    EditorGUILayout.EndScrollView();

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Add Portal"))
                        {
                            ApplyPendingChanges();
                            string defaultTarget = FindDefaultPortalTargetName(m_SelectedRegion);
                            m_SelectedPortal = RegionEditorCommands.AddPortal(
                                m_Manager, m_SelectedRegion, defaultTarget);
                            RebindAfterCommand();
                        }
                        using (new EditorGUI.DisabledScope(m_SelectedPortal < 0 || m_SelectedPortal >= portals.arraySize))
                        {
                            if (GUILayout.Button("Delete"))
                            {
                                ApplyPendingChanges();
                                if (RegionEditorCommands.DeletePortal(m_Manager, m_SelectedRegion, m_SelectedPortal))
                                    m_SelectedPortal = Mathf.Min(m_SelectedPortal, portals.arraySize - 2);
                                RebindAfterCommand();
                            }
                        }
                    }
                }

                DrawVerticalSeparator();

                using (new EditorGUILayout.VerticalScope())
                    DrawSelectedPortalDetails(portals);
            }
        }

        private void DrawSelectedPortalDetails(SerializedProperty portals)
        {
            if (m_SelectedPortal < 0 || m_SelectedPortal >= portals.arraySize)
            {
                EditorGUILayout.HelpBox("Select or create a portal.", MessageType.Info);
                return;
            }

            SerializedProperty portal = portals.GetArrayElementAtIndex(m_SelectedPortal);
            SerializedProperty target = RegionEditorCommands.FindPortalTargetProperty(portal);
            SerializedProperty bounds = RegionEditorCommands.FindPortalBoundsProperty(portal);
            EditorGUILayout.LabelField($"Portal {m_SelectedPortal + 1}", EditorStyles.boldLabel);

            string[] options = BuildRegionNameOptions();
            int selectedTarget = 0;
            for (int i = 1; i < options.Length; i++)
            {
                if (string.Equals(options[i], target?.stringValue, StringComparison.Ordinal))
                {
                    selectedTarget = i;
                    break;
                }
            }

            int newTarget = EditorGUILayout.Popup("Target Region", selectedTarget, options);
            if (target != null && newTarget != selectedTarget)
                target.stringValue = newTarget == 0 ? string.Empty : options[newTarget];

            if (bounds != null)
                EditorGUILayout.PropertyField(bounds, new GUIContent("Local Bounds"), true);

            using (new EditorGUILayout.HorizontalScope())
            {
                m_EditPortalBounds = GUILayout.Toggle(m_EditPortalBounds, "Edit In Scene View", "Button");
                if (GUILayout.Button("Frame Portal"))
                    FrameSelectedPortal();
                using (new EditorGUI.DisabledScope(target == null || string.IsNullOrWhiteSpace(target.stringValue)))
                {
                    if (GUILayout.Button("Create Reciprocal"))
                    {
                        ApplyPendingChanges();
                        int result = RegionEditorCommands.CreateReciprocalPortal(
                            m_Manager, m_SelectedRegion, m_SelectedPortal);
                        if (result < 0)
                            ShowNotification(new GUIContent("Reciprocal portal could not be created."));
                        else
                            ShowNotification(new GUIContent("Reciprocal portal is ready."));
                        RebindAfterCommand();
                    }
                }
            }

            if (target != null && string.Equals(
                    target.stringValue,
                    m_Manager.Regions[m_SelectedRegion]?.RegionName,
                    StringComparison.Ordinal))
            {
                EditorGUILayout.HelpBox("A portal should normally target a different region.", MessageType.Warning);
            }
        }

        private void DrawRuntimePage()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Enter Play Mode to inspect desired, loading, loaded, active, and failed regions.",
                    MessageType.Info);
                return;
            }

            RegionStreamingController controller = m_Manager.GetComponent<RegionStreamingController>();
            if (controller == null)
            {
                EditorGUILayout.HelpBox("RegionStreamingController is missing.", MessageType.Error);
                return;
            }

            IReadOnlyList<RegionStreamingController.RegionDebugSnapshot> snapshot = controller.GetDebugSnapshot();
            int desired = 0;
            int loaded = 0;
            int active = 0;
            int failed = 0;
            foreach (var region in snapshot)
            {
                if (region.DesireReason != RegionStreamingController.RegionDesireReason.None) desired++;
                if (region.State == RegionManager.RegionStreamingState.Loaded) loaded++;
                if (region.IsActive) active++;
                if (region.State == RegionManager.RegionStreamingState.Failed) failed++;
            }

            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                DrawMetric("Regions", snapshot.Count);
                DrawMetric("Desired", desired);
                DrawMetric("Loaded", loaded);
                DrawMetric("Active", active);
                DrawMetric("Failed", failed);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(controller.enabled ? "Pause Evaluation" : "Resume Evaluation", GUILayout.Width(120f)))
                    controller.enabled = !controller.enabled;
            }

            DrawRuntimeProviderState();

            _runtimeScroll = EditorGUILayout.BeginScrollView(_runtimeScroll);
            foreach (var region in snapshot)
            {
                Color old = GUI.backgroundColor;
                GUI.backgroundColor = GetRuntimeColor(region);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUI.backgroundColor = old;
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(region.Name, EditorStyles.boldLabel, GUILayout.MinWidth(130f));
                    EditorGUILayout.LabelField(region.Type.ToString(), GUILayout.Width(105f));
                    EditorGUILayout.LabelField(region.State.ToString(), GUILayout.Width(90f));
                    EditorGUILayout.LabelField(region.DesireReason.ToString(), GUILayout.Width(90f));
                    EditorGUILayout.ToggleLeft("Active", region.IsActive, GUILayout.Width(65f));
                    if (GUILayout.Button("Frame", GUILayout.Width(50f)))
                        FrameRegion(RegionEditorCommands.FindRegionIndex(m_Manager, region.Name));
                }

                string registry = string.IsNullOrWhiteSpace(region.RegistryKey)
                    ? "No registry ownership"
                    : $"{region.RegistryKey}  |  refs={region.RegistryReferenceCount}";
                EditorGUILayout.LabelField(registry, EditorStyles.miniLabel);
                if (!string.IsNullOrWhiteSpace(region.LastError))
                    EditorGUILayout.HelpBox(
                        $"{region.LastError} (consecutive failures: {region.ConsecutiveFailures})",
                        MessageType.Error);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawRuntimeProviderState()
        {
            foreach (MonoBehaviour provider in StreamingEditorValidation.FindProviders(m_Manager.gameObject.scene))
            {
                if (provider is not IStreamingBoundsSnapshotProvider snapshotProvider ||
                    !snapshotProvider.TryGetSnapshot(out StreamingBoundsSnapshot bounds))
                    continue;

                EditorGUILayout.LabelField(
                    $"Provider: {provider.GetType().Name}  |  Zoom: {bounds.NormalizedZoom:F2}  |  " +
                    $"Velocity: {bounds.Velocity.magnitude:F1} u/s  |  Revision: {bounds.Revision}",
                    EditorStyles.miniLabel);
                break;
            }
        }

        private void DrawValidationIssues(float maxHeight)
        {
            if (_issues == null || _issues.Count == 0)
            {
                EditorGUILayout.HelpBox("Streaming configuration is valid.", MessageType.Info);
                return;
            }

            _validationScroll = EditorGUILayout.BeginScrollView(_validationScroll, GUILayout.MaxHeight(maxHeight));
            foreach (StreamingEditorIssue issue in _issues)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.HelpBox(issue.Message, issue.Severity);
                    if (issue.Context != null && GUILayout.Button("Select", GUILayout.Width(52f), GUILayout.Height(38f)))
                        Selection.activeObject = issue.Context;
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawStatusBar()
        {
            int errors = 0;
            int warnings = 0;
            foreach (StreamingEditorIssue issue in _issues)
            {
                if (issue.Severity == MessageType.Error) errors++;
                else if (issue.Severity == MessageType.Warning) warnings++;
            }

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUILayout.LabelField(
                    $"{m_Manager.Regions.Count} regions  |  {errors} errors  |  {warnings} warnings",
                    EditorStyles.miniLabel);
                GUILayout.FlexibleSpace();
                m_ShowAllLabels = GUILayout.Toggle(m_ShowAllLabels, "Scene Labels", EditorStyles.toolbarButton);
            }
        }

        private void DuringSceneGui(SceneView sceneView)
        {
            if (m_Manager == null || m_Manager.Regions == null ||
                m_Page is not (Page.Regions or Page.Connections))
                return;

            for (int i = 0; i < m_Manager.Regions.Count; i++)
            {
                RegionManager.Region region = m_Manager.Regions[i];
                if (region == null)
                    continue;

                bool selected = i == m_SelectedRegion;
                Color color = selected ? Color.yellow : GetRegionColor(region.Type);
                Handles.color = color;
                Handles.DrawWireCube(region.CachedBounds.center, region.CachedBounds.size);

                Vector3 labelPosition = region.CachedBounds.center + Vector3.up * region.CachedBounds.extents.y;
                float handleSize = HandleUtility.GetHandleSize(labelPosition) * 0.08f;
                if (!Application.isPlaying && Handles.Button(
                        labelPosition,
                        Quaternion.identity,
                        handleSize,
                        handleSize,
                        Handles.RectangleHandleCap))
                {
                    SelectRegion(i, false);
                    GUIUtility.ExitGUI();
                }

                if (m_ShowAllLabels || selected)
                    Handles.Label(labelPosition + Vector3.up * handleSize, region.RegionName, EditorStyles.miniBoldLabel);
            }

            if (!HasSelectedRegion() || Application.isPlaying)
                return;

            if (m_Page == Page.Regions && m_EditRegionBounds)
                DrawSelectedRegionHandle();
            if (m_Page == Page.Connections && m_EditPortalBounds)
                DrawSelectedPortalHandle();
        }

        private void DrawSelectedRegionHandle()
        {
            RegionManager.Region region = m_Manager.Regions[m_SelectedRegion];
            _regionHandle.center = region.CachedBounds.center;
            _regionHandle.size = region.CachedBounds.size;
            Handles.color = Color.yellow;
            EditorGUI.BeginChangeCheck();
            _regionHandle.DrawHandle();
            if (!EditorGUI.EndChangeCheck())
                return;

            Undo.RecordObject(m_Manager, "Edit Streaming Region Bounds");
            region.CachedBounds = new Bounds(_regionHandle.center, AbsSize(_regionHandle.size));
            region.RebuildPortalWorldBounds();
            RegionEditorCommands.MarkDirty(m_Manager);
            _serializedManager?.Update();
            RefreshValidation();
            Repaint();
        }

        private void DrawSelectedPortalHandle()
        {
            RegionManager.Region region = m_Manager.Regions[m_SelectedRegion];
            if (region?.Portals == null || m_SelectedPortal < 0 || m_SelectedPortal >= region.Portals.Count)
                return;

            RegionManager.Portal portal = region.Portals[m_SelectedPortal];
            if (portal == null)
                return;

            _portalHandle.center = region.CachedBounds.center + portal.LocalBounds.center;
            _portalHandle.size = portal.LocalBounds.size;
            Handles.color = Color.magenta;
            EditorGUI.BeginChangeCheck();
            _portalHandle.DrawHandle();
            if (!EditorGUI.EndChangeCheck())
                return;

            Undo.RecordObject(m_Manager, "Edit Streaming Portal Bounds");
            portal.LocalBounds = new Bounds(
                _portalHandle.center - region.CachedBounds.center,
                AbsSize(_portalHandle.size));
            region.RebuildPortalWorldBounds();
            RegionEditorCommands.MarkDirty(m_Manager);
            _serializedManager?.Update();
            RefreshValidation();
            Repaint();
        }

        private void DrawSourceButtons(RegionManager.Region region)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(GetSourceAsset(region) == null))
                {
                    if (GUILayout.Button("Ping Asset"))
                        EditorGUIUtility.PingObject(GetSourceAsset(region));
                    if (GUILayout.Button("Open Asset"))
                        AssetDatabase.OpenAsset(GetSourceAsset(region));
                }
            }
        }

        private void FitSelectedBounds()
        {
            ApplyPendingChanges();
            Undo.RecordObject(m_Manager, "Fit Streaming Region Bounds");
            m_Manager.RefreshBounds(m_Manager.Regions[m_SelectedRegion]);
            RebindAfterCommand();
        }

        private void ApplySelectedBounds()
        {
            ApplyPendingChanges();
            if (!EditorUtility.DisplayDialog(
                    "Apply Streaming Bounds",
                    "Write the cached world bounds into the selected source scene or prefab marker?",
                    "Apply",
                    "Cancel"))
                return;
            m_Manager.ApplyBounds(m_Manager.Regions[m_SelectedRegion]);
            RebindAfterCommand();
        }

        private void AddAdaptiveProvider()
        {
            Camera camera = FindSceneCamera(m_Manager.gameObject.scene);
            if (camera == null)
            {
                EditorUtility.DisplayDialog("No Camera", "Add a Camera to the persistent scene first.", "OK");
                return;
            }

            AdaptiveStreamingBoundsProvider provider = Undo.AddComponent<AdaptiveStreamingBoundsProvider>(camera.gameObject);
            var serializedProvider = new SerializedObject(provider);
            serializedProvider.FindProperty("m_Controller").objectReferenceValue =
                m_Manager.GetComponent<RegionStreamingController>();
            serializedProvider.FindProperty("m_Camera").objectReferenceValue = camera;
            serializedProvider.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(provider);
            EditorSceneManager.MarkSceneDirty(camera.gameObject.scene);
            m_InspectedProvider = provider;
            RefreshValidation();
        }

        private void ShowAddRegionMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Built-in Scene"), false,
                () => AddRegion(RegionManager.RegionType.Scene));
            menu.AddItem(new GUIContent("Addressable Prefab"), false,
                () => AddRegion(RegionManager.RegionType.Prefab));
            menu.AddItem(new GUIContent("Addressable Scene"), false,
                () => AddRegion(RegionManager.RegionType.AddressableScene));
            menu.ShowAsContext();
        }

        private void AddRegion(RegionManager.RegionType type)
        {
            ApplyPendingChanges();
            Vector3 center = SceneView.lastActiveSceneView != null
                ? SceneView.lastActiveSceneView.pivot
                : Vector3.zero;
            SelectRegion(RegionEditorCommands.AddRegion(m_Manager, type, center), true);
            RebindAfterCommand();
        }

        private void MoveSelectedRegion(int delta)
        {
            ApplyPendingChanges();
            int destination = RegionEditorCommands.MoveRegion(m_Manager, m_SelectedRegion, delta);
            if (destination == m_SelectedRegion)
                return;

            m_SelectedRegion = destination;
            RebindAfterCommand();
            GUIUtility.ExitGUI();
        }

        private void SelectAdjacentRegion(int index)
        {
            if (index < 0 || index >= m_Manager.Regions.Count)
                return;

            ApplyPendingChanges();
            _rightScroll = Vector2.zero;
            SelectRegion(index, true);
            GUIUtility.ExitGUI();
        }

        private int FindAdjacentVisibleRegion(int direction)
        {
            if (m_Manager == null || direction == 0)
                return -1;

            int step = Math.Sign(direction);
            for (int index = m_SelectedRegion + step;
                 index >= 0 && index < m_Manager.Regions.Count;
                 index += step)
            {
                if (MatchesFilter(m_Manager.Regions[index]))
                    return index;
            }

            return -1;
        }

        private void DrawRegionSelectionPopup()
        {
            string[] labels = new string[m_Manager.Regions.Count];
            for (int i = 0; i < labels.Length; i++)
                labels[i] = m_Manager.Regions[i]?.RegionName ?? $"{i}: <null>";
            int selected = Mathf.Clamp(m_SelectedRegion, 0, labels.Length - 1);
            int result = EditorGUILayout.Popup("Source Region", selected, labels);
            if (result != m_SelectedRegion)
                SelectRegion(result, true);
        }

        private void SelectRegion(int index, bool frame)
        {
            m_SelectedRegion = index >= 0 && index < (m_Manager?.Regions.Count ?? 0) ? index : -1;
            m_SelectedPortal = -1;
            if (frame && m_SelectedRegion >= 0)
                FrameRegion(m_SelectedRegion);
            SceneView.RepaintAll();
            Repaint();
        }

        private void FrameRegion(int index)
        {
            if (m_Manager == null || index < 0 || index >= m_Manager.Regions.Count ||
                m_Manager.Regions[index] == null)
                return;
            SceneView.lastActiveSceneView?.Frame(m_Manager.Regions[index].CachedBounds, false);
        }

        private void FrameSelectedPortal()
        {
            if (!HasSelectedRegion())
                return;
            RegionManager.Region region = m_Manager.Regions[m_SelectedRegion];
            if (region?.Portals == null || m_SelectedPortal < 0 || m_SelectedPortal >= region.Portals.Count)
                return;
            Bounds bounds = region.Portals[m_SelectedPortal].LocalBounds;
            bounds.center += region.CachedBounds.center;
            SceneView.lastActiveSceneView?.Frame(bounds, false);
        }

        private void SetManager(RegionManager manager)
        {
            if (m_Manager == manager)
                return;
            m_Manager = manager;
            m_SelectedRegion = manager != null && manager.Regions.Count > 0 ? 0 : -1;
            m_SelectedPortal = -1;
            m_InspectedProvider = null;
            DestroyProviderEditor();
            BindManager();
        }

        private void BindManager()
        {
            _serializedManager = m_Manager == null ? null : new SerializedObject(m_Manager);
            ClampSelection();
            RefreshValidation();
            SceneView.RepaintAll();
            Repaint();
        }

        private void EnsureSerializedManager()
        {
            if (_serializedManager == null || _serializedManager.targetObject != m_Manager)
                BindManager();
        }

        private void ApplyPendingChanges()
        {
            if (_serializedManager != null && _serializedManager.ApplyModifiedProperties())
                RegionEditorCommands.MarkDirty(m_Manager);
        }

        private void RebindAfterCommand()
        {
            _serializedManager = new SerializedObject(m_Manager);
            ClampSelection();
            RebuildPortalCaches();
            RefreshValidation();
            SceneView.RepaintAll();
            Repaint();
        }

        private void RefreshValidation()
        {
            _issues = StreamingEditorValidation.Validate(m_Manager);
        }

        private void RebuildPortalCaches()
        {
            if (m_Manager == null)
                return;
            foreach (RegionManager.Region region in m_Manager.Regions)
                region?.RebuildPortalWorldBounds();
        }

        private void ClampSelection()
        {
            if (m_Manager == null || m_Manager.Regions.Count == 0)
            {
                m_SelectedRegion = -1;
                m_SelectedPortal = -1;
                return;
            }

            m_SelectedRegion = Mathf.Clamp(m_SelectedRegion, 0, m_Manager.Regions.Count - 1);
            int count = m_Manager.Regions[m_SelectedRegion]?.Portals?.Count ?? 0;
            m_SelectedPortal = count == 0 ? -1 : Mathf.Clamp(m_SelectedPortal, -1, count - 1);
        }

        private bool HasSelectedRegion()
        {
            return m_Manager != null && m_SelectedRegion >= 0 && m_SelectedRegion < m_Manager.Regions.Count &&
                   m_Manager.Regions[m_SelectedRegion] != null;
        }

        private SerializedProperty GetSelectedRegionProperty()
        {
            if (!HasSelectedRegion())
                return null;
            return _serializedManager.FindProperty("regions").GetArrayElementAtIndex(m_SelectedRegion);
        }

        private bool MatchesFilter(RegionManager.Region region)
        {
            if (region == null)
                return m_Filter == RegionFilter.All && string.IsNullOrWhiteSpace(m_Search);
            if (m_Filter != RegionFilter.All && (int)region.Type != (int)m_Filter - 1)
                return false;
            return string.IsNullOrWhiteSpace(m_Search) ||
                   (!string.IsNullOrWhiteSpace(region.RegionName) &&
                    region.RegionName.IndexOf(m_Search, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static MessageType GetRegionStatus(RegionManager.Region region)
        {
            if (region == null || string.IsNullOrWhiteSpace(region.RegionName) ||
                !IsUsableBounds(region.CachedBounds) || GetSourceAsset(region) == null)
                return MessageType.Error;
            return region.UnloadStrategy == null ? MessageType.Warning : MessageType.Info;
        }

        private static bool IsUsableBounds(Bounds bounds)
        {
            Vector3 center = bounds.center;
            Vector3 size = bounds.size;
            return float.IsFinite(center.x) && float.IsFinite(center.y) && float.IsFinite(center.z) &&
                   float.IsFinite(size.x) && float.IsFinite(size.y) && float.IsFinite(size.z) &&
                   size.x > 0f && size.y > 0f && size.z > 0f;
        }

        private static UnityEngine.Object GetSourceAsset(RegionManager.Region region)
        {
            if (region == null)
                return null;
            return region.Type switch
            {
                RegionManager.RegionType.Scene => region.SceneRef?.SceneAsset,
                RegionManager.RegionType.Prefab => AssetDatabase.LoadAssetAtPath<GameObject>(
                    AssetDatabase.GUIDToAssetPath(region.PrefabRef?.AssetGUID)),
                RegionManager.RegionType.AddressableScene => AssetDatabase.LoadAssetAtPath<SceneAsset>(
                    AssetDatabase.GUIDToAssetPath(region.AddressableSceneRef?.AssetGUID)),
                _ => null
            };
        }

        private string[] BuildRegionNameOptions()
        {
            var result = new string[m_Manager.Regions.Count + 1];
            result[0] = "<None>";
            for (int i = 0; i < m_Manager.Regions.Count; i++)
                result[i + 1] = m_Manager.Regions[i]?.RegionName ?? $"<null {i}>";
            return result;
        }

        private string FindDefaultPortalTargetName(int sourceIndex)
        {
            if (m_Manager.Regions.Count < 2)
                return string.Empty;
            int target = sourceIndex == 0 ? 1 : 0;
            return m_Manager.Regions[target]?.RegionName ?? string.Empty;
        }

        private void ShowManagerMenu()
        {
            var menu = new GenericMenu();
            RegionManager[] managers = FindObjectsByType<RegionManager>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            if (managers.Length == 0)
                menu.AddDisabledItem(new GUIContent("No loaded RegionManagers"));
            else
            {
                foreach (RegionManager manager in managers)
                {
                    RegionManager candidate = manager;
                    string path = $"{candidate.gameObject.scene.name}/{GetHierarchyPath(candidate.transform)}";
                    menu.AddItem(new GUIContent(path), candidate == m_Manager, () => SetManager(candidate));
                }
            }
            menu.ShowAsContext();
        }

        private void OnHierarchyChanged()
        {
            if (m_Manager == null)
                m_Manager = FindManagerFromSelection() ?? FindSingleLoadedManager();
            BindManager();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            BindManager();
        }

        private void DestroyProviderEditor()
        {
            if (_providerEditor != null)
                DestroyImmediate(_providerEditor);
            _providerEditor = null;
        }

        private static RegionManager FindManagerFromSelection()
        {
            return Selection.activeGameObject != null
                ? Selection.activeGameObject.GetComponentInParent<RegionManager>()
                : Selection.activeObject as RegionManager;
        }

        private static RegionManager FindSingleLoadedManager()
        {
            RegionManager[] managers = FindObjectsByType<RegionManager>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            return managers.Length == 1 ? managers[0] : null;
        }

        private static Camera FindSceneCamera(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Camera camera = root.GetComponentInChildren<Camera>(true);
                if (camera != null)
                    return camera;
            }
            return null;
        }

        private static string GetHierarchyPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = $"{transform.name}/{path}";
            }
            return path;
        }

        private static string GetTypeLabel(RegionManager.RegionType type)
        {
            return type switch
            {
                RegionManager.RegionType.Scene => "Scene",
                RegionManager.RegionType.Prefab => "Prefab",
                RegionManager.RegionType.AddressableScene => "Addr Scene",
                _ => type.ToString()
            };
        }

        private static Color GetRegionColor(RegionManager.RegionType type)
        {
            return type switch
            {
                RegionManager.RegionType.Scene => new Color(0.1f, 0.8f, 1f, 0.8f),
                RegionManager.RegionType.Prefab => new Color(0.35f, 1f, 0.35f, 0.8f),
                RegionManager.RegionType.AddressableScene => new Color(0.75f, 0.4f, 1f, 0.8f),
                _ => Color.white
            };
        }

        private static Color GetRuntimeColor(RegionStreamingController.RegionDebugSnapshot region)
        {
            if (region.State == RegionManager.RegionStreamingState.Failed)
                return new Color(1f, 0.35f, 0.35f);
            if (region.IsActive)
                return new Color(0.3f, 0.65f, 1f);
            if (region.State == RegionManager.RegionStreamingState.Loaded)
                return new Color(0.4f, 0.85f, 0.45f);
            if (region.State is RegionManager.RegionStreamingState.Loading or
                RegionManager.RegionStreamingState.Unloading)
                return new Color(1f, 0.8f, 0.3f);
            return Color.white;
        }

        private static Vector3 AbsSize(Vector3 value)
        {
            return new Vector3(
                Mathf.Max(0.01f, Mathf.Abs(value.x)),
                Mathf.Max(0.01f, Mathf.Abs(value.y)),
                Mathf.Max(0.01f, Mathf.Abs(value.z)));
        }

        private static void DrawSectionTitle(string title)
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        }

        private static void DrawVerticalSeparator()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1f, GUILayout.Width(1f), GUILayout.ExpandHeight(true));
            EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.25f));
        }

        private static void DrawPropertyIfPresent(SerializedObject serializedObject, string name, string label)
        {
            SerializedProperty property = serializedObject.FindProperty(name);
            if (property != null)
                EditorGUILayout.PropertyField(property, new GUIContent(label));
        }

        private static void DrawMetric(string label, int value)
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(66f)))
            {
                EditorGUILayout.LabelField(value.ToString(), EditorStyles.boldLabel);
                EditorGUILayout.LabelField(label, EditorStyles.miniLabel);
            }
        }
    }
}

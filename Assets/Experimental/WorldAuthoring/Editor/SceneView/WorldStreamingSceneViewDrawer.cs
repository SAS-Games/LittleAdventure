using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Rendering;

namespace SAS.WorldStreaming.Editor
{
    internal sealed class WorldStreamingSceneViewDrawer
    {
        private readonly BoxBoundsHandle m_BoundsHandle = new();
        private readonly UniformGridRegionProvider m_GridProvider = new();

        public int Draw(
            WorldStreamingProfile profile,
            int selectedRegionIndex,
            bool editLayout)
        {
            if (profile == null || !profile.ShowSceneViewVisualization)
                return selectedRegionIndex;

            CompareFunction previousZTest = Handles.zTest;
            Color previousColor = Handles.color;
            try
            {
                Handles.zTest = CompareFunction.LessEqual;

                for (int i = 0; i < profile.Regions.Count; i++)
                {
                    StreamingRegionDefinition region = profile.Regions[i];
                    if (region == null || !region.Enabled)
                        continue;

                    bool selected = i == selectedRegionIndex;
                    Color color = selected ? Color.yellow : region.GizmoColor;
                    DrawRegion(profile, region, color, selected);

                    float buttonSize = HandleUtility.GetHandleSize(region.Bounds.center) * 0.06f;
                    Handles.color = new Color(color.r, color.g, color.b, 0.9f);
                    if (Handles.Button(
                            region.Bounds.center,
                            Quaternion.identity,
                            buttonSize,
                            buttonSize,
                            Handles.DotHandleCap))
                    {
                        selectedRegionIndex = i;
                        SceneView.RepaintAll();
                    }
                }

                if (!editLayout)
                    return selectedRegionIndex;

                if (profile.RegionGenerationMode == StreamingRegionGenerationMode.UniformGrid)
                    EditGridOrigin(profile);
                else
                    EditSelectedManualRegion(profile, selectedRegionIndex);

                return selectedRegionIndex;
            }
            finally
            {
                Handles.zTest = previousZTest;
                Handles.color = previousColor;
            }
        }

        private static void DrawRegion(
            WorldStreamingProfile profile,
            StreamingRegionDefinition region,
            Color color,
            bool selected)
        {
            Handles.color = new Color(color.r, color.g, color.b, selected ? 1f : 0.75f);
            Handles.DrawWireCube(region.Bounds.center, region.Bounds.size);

            Bounds loadBounds = region.GetLoadBounds();
            Handles.color = new Color(color.r, color.g, color.b, 0.35f);
            Handles.DrawWireCube(loadBounds.center, loadBounds.size);

            Bounds unloadBounds = region.GetUnloadBounds();
            Handles.color = new Color(color.r, color.g, color.b, 0.18f);
            Handles.DrawWireCube(unloadBounds.center, unloadBounds.size);

            int objectCount = profile.AuthoringDatabase == null
                ? 0
                : profile.AuthoringDatabase.GetAssignedObjectCount(region.RegionId);
            string coordinate = region.GeneratedFromGrid
                ? $"\nGrid: {region.GridCoordinate}"
                : string.Empty;
            string label =
                $"{region.DisplayName}\n{region.SceneName}.unity{coordinate}\nObjects: {objectCount}";

            Vector3 labelPosition = region.Bounds.center +
                                    Vector3.up * region.Bounds.extents.y;
            Handles.Label(labelPosition, label, EditorStyles.miniBoldLabel);
        }

        private void EditGridOrigin(WorldStreamingProfile profile)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 origin = Handles.PositionHandle(profile.WorldOrigin, Quaternion.identity);
            if (!EditorGUI.EndChangeCheck())
                return;

            Undo.RecordObject(profile, "Move World Streaming Grid");
            profile.SetWorldOrigin(origin);
            profile.ReplaceRegions(m_GridProvider.Generate(profile));
            EditorUtility.SetDirty(profile);
        }

        private void EditSelectedManualRegion(
            WorldStreamingProfile profile,
            int selectedRegionIndex)
        {
            if (selectedRegionIndex < 0 || selectedRegionIndex >= profile.Regions.Count)
                return;

            StreamingRegionDefinition region = profile.Regions[selectedRegionIndex];
            if (region == null || region.GeneratedFromGrid)
                return;

            m_BoundsHandle.center = region.Bounds.center;
            m_BoundsHandle.size = region.Bounds.size;

            Handles.color = Color.yellow;
            EditorGUI.BeginChangeCheck();
            m_BoundsHandle.DrawHandle();
            if (!EditorGUI.EndChangeCheck())
                return;

            Undo.RecordObject(profile, "Edit World Streaming Region");
            region.SetBounds(new Bounds(m_BoundsHandle.center, m_BoundsHandle.size));
            EditorUtility.SetDirty(profile);
        }
    }
}

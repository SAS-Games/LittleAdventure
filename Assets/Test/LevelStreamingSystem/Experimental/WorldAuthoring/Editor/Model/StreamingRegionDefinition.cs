using System;
using UnityEngine;

namespace SAS.WorldStreaming.Editor
{
    /// <summary>
    /// Stable authoring definition for one streaming region.
    /// </summary>
    [Serializable]
    public sealed class StreamingRegionDefinition
    {
        [SerializeField, HideInInspector] private string m_RegionId = string.Empty;
        [SerializeField] private string m_DisplayName = "Region";
        [SerializeField] private Bounds m_Bounds = new(Vector3.zero, Vector3.one * 100f);
        [SerializeField] private bool m_Enabled = true;
        [SerializeField] private int m_Priority;
        [SerializeField] private string m_SceneName = "Region";
        [SerializeField] private Color m_GizmoColor = new(0.1f, 0.8f, 1f, 1f);
        [SerializeField] private Vector3 m_LoadPadding;
        [SerializeField] private Vector3 m_UnloadPadding;
        [SerializeField, HideInInspector] private Vector3Int m_GridCoordinate;
        [SerializeField, HideInInspector] private bool m_GeneratedFromGrid;

        public string RegionId => m_RegionId;
        public string DisplayName => m_DisplayName;
        public Bounds Bounds => m_Bounds;
        public bool Enabled => m_Enabled;
        public int Priority => m_Priority;
        public string SceneName => m_SceneName;
        public Color GizmoColor => m_GizmoColor;
        public Vector3 LoadPadding => m_LoadPadding;
        public Vector3 UnloadPadding => m_UnloadPadding;
        public Vector3Int GridCoordinate => m_GridCoordinate;
        public bool GeneratedFromGrid => m_GeneratedFromGrid;

        public Bounds GetLoadBounds()
        {
            Bounds result = m_Bounds;
            result.Expand(SanitizePadding(m_LoadPadding) * 2f);
            return result;
        }

        public Bounds GetUnloadBounds()
        {
            Bounds result = m_Bounds;
            result.Expand(SanitizePadding(m_UnloadPadding) * 2f);
            return result;
        }

        public void SetBounds(Bounds bounds)
        {
            m_Bounds = bounds;
        }

        public bool EnsureStableId()
        {
            if (!string.IsNullOrWhiteSpace(m_RegionId))
                return false;

            m_RegionId = Guid.NewGuid().ToString("N");
            return true;
        }

        public StreamingRegionDefinition DuplicateAsManual()
        {
            Bounds duplicateBounds = m_Bounds;
            duplicateBounds.center += Vector3.right * Mathf.Max(1f, duplicateBounds.extents.x * 0.25f);

            return new StreamingRegionDefinition
            {
                m_RegionId = Guid.NewGuid().ToString("N"),
                m_DisplayName = $"{m_DisplayName} Copy",
                m_Bounds = duplicateBounds,
                m_Enabled = m_Enabled,
                m_Priority = m_Priority,
                m_SceneName = $"{m_SceneName}_Copy",
                m_GizmoColor = m_GizmoColor,
                m_LoadPadding = m_LoadPadding,
                m_UnloadPadding = m_UnloadPadding,
                m_GridCoordinate = default,
                m_GeneratedFromGrid = false
            };
        }

        public static StreamingRegionDefinition CreateGridRegion(
            string stableId,
            string displayName,
            string sceneName,
            Bounds bounds,
            Vector3 padding,
            Vector3Int coordinate,
            Color color)
        {
            return new StreamingRegionDefinition
            {
                m_RegionId = string.IsNullOrWhiteSpace(stableId)
                    ? Guid.NewGuid().ToString("N")
                    : stableId,
                m_DisplayName = displayName,
                m_SceneName = sceneName,
                m_Bounds = bounds,
                m_Enabled = true,
                m_Priority = 0,
                m_GizmoColor = color,
                m_LoadPadding = SanitizePadding(padding),
                m_UnloadPadding = SanitizePadding(padding),
                m_GridCoordinate = coordinate,
                m_GeneratedFromGrid = true
            };
        }

        public static StreamingRegionDefinition CreateManual(int index, Bounds bounds)
        {
            Color color = Color.HSVToRGB(Mathf.Repeat(index * 0.173f, 1f), 0.65f, 1f);
            return new StreamingRegionDefinition
            {
                m_RegionId = Guid.NewGuid().ToString("N"),
                m_DisplayName = $"Region {index + 1}",
                m_SceneName = $"Region_{index + 1}",
                m_Bounds = bounds,
                m_GizmoColor = color
            };
        }

        internal void Sanitize()
        {
            EnsureStableId();
            Vector3 size = m_Bounds.size;
            size.x = SanitizeSize(size.x);
            size.y = SanitizeSize(size.y);
            size.z = SanitizeSize(size.z);
            m_Bounds.size = size;
            m_LoadPadding = SanitizePadding(m_LoadPadding);
            m_UnloadPadding = SanitizePadding(m_UnloadPadding);
        }

        private static Vector3 SanitizePadding(Vector3 padding)
        {
            return new Vector3(
                Mathf.Max(0f, padding.x),
                Mathf.Max(0f, padding.y),
                Mathf.Max(0f, padding.z));
        }

        private static float SanitizeSize(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value)
                ? 1f
                : Mathf.Max(0.01f, Mathf.Abs(value));
        }
    }
}

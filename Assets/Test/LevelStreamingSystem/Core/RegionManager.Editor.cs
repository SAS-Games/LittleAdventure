#if UNITY_EDITOR
using LevelStreaming.Editor;
using UnityEditor;
using UnityEngine;

namespace LevelStreaming
{
    public partial class RegionManager
    {
        [Header("Editor Debug")]
        [SerializeField] private bool m_DrawRegionGizmos = true;
        [SerializeField] private bool m_DrawPortalGizmos = true;

        public partial class Region
        {
            public void OnValidate()
            {
                if (Type == RegionType.Scene && SceneRef?.SceneAsset != null)
                {
                    // Reassign through the property so the serialized path follows moves.
                    SceneRef.SceneAsset = SceneRef.SceneAsset;
                }

                // Region names are stable portal/lookup identifiers. Only supply a name
                // for a newly created blank entry; never overwrite an authored one.
                if (string.IsNullOrWhiteSpace(regionName))
                    regionName = RegionAuthoringUtility.GetDefaultRegionName(this);

                Vector3 size = CachedBounds.size;
                size.x = Mathf.Max(0.01f, Mathf.Abs(size.x));
                size.y = Mathf.Max(0.01f, Mathf.Abs(size.y));
                size.z = Mathf.Max(0.01f, Mathf.Abs(size.z));
                CachedBounds = new Bounds(CachedBounds.center, size);
                RebuildPortalWorldBounds();
            }
        }

        public void ApplyBounds(Region region)
        {
            RegionBoundsAuthoringService.ApplyToSource(region);
        }

        public void RefreshBounds(Region region)
        {
            if (region == null)
                return;

            region.OnValidate();
            if (RegionBoundsAuthoringService.RefreshFromSource(region))
                EditorUtility.SetDirty(this);
        }

        public void RefreshBounds()
        {
            bool changed = false;
            foreach (var region in Regions)
            {
                if (region == null)
                    continue;

                region.OnValidate();
                changed |= RegionBoundsAuthoringService.RefreshFromSource(region);
            }

            if (changed)
                EditorUtility.SetDirty(this);
        }

        [ContextMenu("Refresh Bounds From Assets")]
        private void RefreshBoundsContextMenu() => RefreshBounds();

        private void OnValidate()
        {
            if (regions == null)
                return;

            foreach (var region in regions)
                region?.OnValidate();
        }

        private void OnDrawGizmos()
        {
            if (!m_DrawRegionGizmos || regions == null)
                return;

            foreach (var region in regions)
            {
                if (region == null)
                    continue;

                bool isLoaded = IsRegionLoaded(region);
                Color wireColor = isLoaded ? Color.green : Color.cyan;
                Color fillColor = new(wireColor.r, wireColor.g, wireColor.b, 0.1f);
                Bounds bounds = region.BroadphaseBounds;

                if (region.Volume is PolygonPrismStreamingVolume polygon)
                    DrawPolygonGizmo(polygon, wireColor);
                else
                {
                    Gizmos.color = fillColor;
                    Gizmos.DrawCube(bounds.center, bounds.size);
                    Gizmos.color = wireColor;
                    Gizmos.DrawWireCube(bounds.center, bounds.size);
                }

                if (!RegionSceneLabelState.Controls(this))
                {
                    Handles.Label(bounds.center + Vector3.up * bounds.extents.y,
                        string.IsNullOrWhiteSpace(region.RegionName) ? region.Type.ToString() : region.RegionName);
                }

                if (!m_DrawPortalGizmos || region.Portals == null)
                    continue;

                foreach (var portal in region.Portals)
                {
                    if (portal == null)
                        continue;

                    if (portal.WorldVolume is PolygonPrismStreamingVolume portalPolygon)
                    {
                        DrawPolygonGizmo(portalPolygon, Color.yellow);
                    }
                    else
                    {
                        Bounds portalBounds = portal.WorldVolume?.BroadphaseBounds ?? portal.LocalBounds;
                        if (portal.WorldVolume == null)
                            portalBounds.center += region.Origin;
                        Gizmos.color = new Color(1f, 1f, 0f, 0.1f);
                        Gizmos.DrawCube(portalBounds.center, portalBounds.size);
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawWireCube(portalBounds.center, portalBounds.size);
                    }
                }
            }
        }

        private static void DrawPolygonGizmo(PolygonPrismStreamingVolume polygon, Color color)
        {
            if (polygon?.Vertices == null || polygon.Vertices.Count < 2)
                return;

            Gizmos.color = color;
            for (int i = 0; i < polygon.Vertices.Count; i++)
            {
                Vector2 current = polygon.Vertices[i];
                Vector2 next = polygon.Vertices[(i + 1) % polygon.Vertices.Count];
                Vector3 minimumCurrent = ToWorld(polygon, current, polygon.MinExtrusion);
                Vector3 minimumNext = ToWorld(polygon, next, polygon.MinExtrusion);
                Vector3 maximumCurrent = ToWorld(polygon, current, polygon.MaxExtrusion);
                Vector3 maximumNext = ToWorld(polygon, next, polygon.MaxExtrusion);
                Gizmos.DrawLine(minimumCurrent, minimumNext);
                Gizmos.DrawLine(maximumCurrent, maximumNext);
                Gizmos.DrawLine(minimumCurrent, maximumCurrent);
            }
        }

        private static Vector3 ToWorld(PolygonPrismStreamingVolume polygon,
            Vector2 vertex, float extrusion)
        {
            return polygon.ProjectionPlane == PolygonPrismPlane.XY
                ? new Vector3(vertex.x, vertex.y, extrusion)
                : new Vector3(vertex.x, extrusion, vertex.y);
        }
    }

    /// <summary>
    /// Coordinates the runtime assembly's editor gizmos with the dedicated
    /// Level Streaming editor without introducing an assembly dependency cycle.
    /// </summary>
    internal static class RegionSceneLabelState
    {
        internal static RegionManager ControlledManager { get; set; }

        internal static bool Controls(RegionManager manager) =>
            manager != null && ControlledManager == manager;
    }
}
#endif

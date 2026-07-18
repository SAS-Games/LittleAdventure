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
                Bounds bounds = region.CachedBounds;

                Gizmos.color = fillColor;
                Gizmos.DrawCube(bounds.center, bounds.size);
                Gizmos.color = wireColor;
                Gizmos.DrawWireCube(bounds.center, bounds.size);

                Handles.Label(bounds.center + Vector3.up * bounds.extents.y,
                    string.IsNullOrWhiteSpace(region.RegionName) ? region.Type.ToString() : region.RegionName);

                if (!m_DrawPortalGizmos || region.Portals == null)
                    continue;

                foreach (var portal in region.Portals)
                {
                    if (portal == null)
                        continue;

                    Vector3 worldCenter = bounds.center + portal.LocalBounds.center;
                    Gizmos.color = new Color(1f, 1f, 0f, 0.1f);
                    Gizmos.DrawCube(worldCenter, portal.LocalBounds.size);
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireCube(worldCenter, portal.LocalBounds.size);
                }
            }
        }
    }
}
#endif

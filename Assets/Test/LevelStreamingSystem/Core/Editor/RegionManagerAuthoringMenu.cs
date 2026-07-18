using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LevelStreaming.Editor
{
    internal static class RegionManagerAuthoringMenu
    {
        [MenuItem("Tools/Streaming/Apply Bounds For All Regions")]
        private static void ApplyBoundsForAllRegions()
        {
            RegionManager manager = RegionAuthoringUtility.FindTargetManager();
            if (manager == null)
                return;

            var sources = new HashSet<string>();
            var sharedSources = new HashSet<string>();
            foreach (var region in manager.Regions)
            {
                string key = RegionBoundsAuthoringService.GetSourceKey(region);
                if (key != null && !sources.Add(key))
                    sharedSources.Add(key);
            }

            if (sharedSources.Count > 0)
            {
                EditorUtility.DisplayDialog(
                    "Cannot Apply All Bounds",
                    "Multiple regions share a source asset. Applying all would make the last region overwrite " +
                    "the others. Apply those regions individually instead.",
                    "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog(
                    "Apply Streaming Bounds",
                    "This writes every cached streaming bound into its source scene or prefab.",
                    "Apply",
                    "Cancel"))
                return;

            foreach (var region in manager.Regions)
                manager.ApplyBounds(region);
        }

        [MenuItem("Tools/Streaming/Refresh Bounds From Assets")]
        private static void RefreshBoundsFromAssets()
        {
            RegionManager manager = RegionAuthoringUtility.FindTargetManager();
            if (manager == null)
                return;

            Undo.RecordObject(manager, "Refresh Streaming Bounds");
            manager.RefreshBounds();
            EditorUtility.SetDirty(manager);
        }
    }
}

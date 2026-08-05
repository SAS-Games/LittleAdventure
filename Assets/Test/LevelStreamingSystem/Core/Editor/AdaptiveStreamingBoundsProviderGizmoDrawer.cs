using LevelStreaming;
using UnityEditor;
using UnityEngine;

internal static class AdaptiveStreamingBoundsProviderGizmoDrawer
{
    [DrawGizmo(GizmoType.Selected)]
    private static void DrawStreamingBounds(AdaptiveStreamingBoundsProvider provider, GizmoType _)
    {
        if (!provider.DrawGizmos ||
            !provider.TryGetDebugBounds(out StreamingBoundsSnapshot sample,
                out Bounds cameraFootprint, out bool hasCameraFootprint))
            return;

        DrawBounds(sample.Unload, new Color(1f, 0.25f, 0.2f, 0.8f));
        DrawBounds(sample.Load, new Color(1f, 0.85f, 0.1f, 0.9f));
        DrawBounds(sample.Activate, new Color(0.1f, 0.55f, 1f, 0.9f));

        Gizmos.color = Color.green;
        Gizmos.DrawLine(sample.ObserverPosition, sample.ObserverPosition + sample.Velocity);

        if (provider.DrawCameraFootprint && hasCameraFootprint)
            DrawBounds(cameraFootprint, new Color(0.8f, 0.3f, 1f, 0.9f));
    }

    private static void DrawBounds(Bounds bounds, Color color)
    {
        Gizmos.color = new Color(color.r, color.g, color.b, 0.08f);
        Gizmos.DrawCube(bounds.center, bounds.size);
        Gizmos.color = color;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}

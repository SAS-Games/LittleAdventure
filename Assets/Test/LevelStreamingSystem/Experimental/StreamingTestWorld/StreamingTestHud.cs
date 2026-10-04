using UnityEngine;

namespace LevelStreaming.TestWorld
{
    /// <summary>
    /// Minimal play-mode telemetry for validating movement- and zoom-driven streaming.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class StreamingTestHud : MonoBehaviour
    {
        [SerializeField] private RegionStreamingController m_Controller;
        [SerializeField] private AdaptiveStreamingBoundsProvider m_BoundsProvider;
        [SerializeField] private Transform m_Target;
        [SerializeField] private Camera m_Camera;
        [SerializeField] private bool m_Visible = true;

        private GUIStyle _boxStyle;
        private GUIStyle _labelStyle;

        private void Awake()
        {
            if (m_Target == null)
                m_Target = transform;
            if (m_Camera == null)
                m_Camera = Camera.main;
        }

        private void Update()
        {
            if (KeyboardTogglePressed())
                m_Visible = !m_Visible;
        }

        private void OnGUI()
        {
            if (!m_Visible)
                return;

            EnsureStyles();

            const float width = 440f;
            Rect area = new Rect(16f, 16f, width, 220f);
            GUI.Box(area, GUIContent.none, _boxStyle);

            GUILayout.BeginArea(new Rect(area.x + 14f, area.y + 10f, area.width - 28f, area.height - 20f));
            GUILayout.Label("STREAMING XZ + ZOOM VALIDATION", _labelStyle);
            GUILayout.Label("WASD: move on XZ   Shift: boost   Alt + LMB: orbit   Wheel: zoom   F1: HUD", _labelStyle);

            Vector3 targetPosition = m_Target != null ? m_Target.position : Vector3.zero;
            float cameraDistance = m_Target != null && m_Camera != null
                ? Vector3.Distance(m_Target.position, m_Camera.transform.position)
                : 0f;
            GUILayout.Label($"Player XZ: ({targetPosition.x:0.0}, {targetPosition.z:0.0})   Camera distance: {cameraDistance:0.0}", _labelStyle);

            if (m_Controller != null)
            {
                GUILayout.Label(
                    $"Regions — Desired: {m_Controller.DesiredRegions.Count}   Loaded: {m_Controller.LoadedRegions.Count}   Active: {m_Controller.ActiveRegions.Count}",
                    _labelStyle);
            }

            if (m_BoundsProvider != null && m_BoundsProvider.TryGetVolumes(out StreamingVolumeSnapshot snapshot))
            {
                GUILayout.Label($"Normalized zoom: {snapshot.NormalizedZoom:0.00}", _labelStyle);
                GUILayout.Label(
                    $"Bounds XZ — Activate: {snapshot.Activate.BroadphaseBounds.size.x:0}×{snapshot.Activate.BroadphaseBounds.size.z:0}   " +
                    $"Load: {snapshot.Load.BroadphaseBounds.size.x:0}×{snapshot.Load.BroadphaseBounds.size.z:0}   " +
                    $"Unload: {snapshot.Unload.BroadphaseBounds.size.x:0}×{snapshot.Unload.BroadphaseBounds.size.z:0}",
                    _labelStyle);

                m_BoundsProvider.TryGetDebugVolumes(
                    out _, out Bounds footprint, out bool hasFootprint);
                GUILayout.Label(
                    hasFootprint
                        ? $"Camera footprint: OK ({footprint.size.x:0}×{footprint.size.z:0})"
                        : "Camera footprint: FAILED (using zoom-scaled player bounds)",
                    _labelStyle);
            }

            GUILayout.EndArea();
        }

        private void EnsureStyles()
        {
            if (_boxStyle != null)
                return;

            _boxStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = Texture2D.grayTexture }
            };
            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                normal = { textColor = Color.white },
                wordWrap = false
            };
        }

        private static bool KeyboardTogglePressed()
        {
            return UnityEngine.InputSystem.Keyboard.current?.f1Key.wasPressedThisFrame == true;
        }
    }
}

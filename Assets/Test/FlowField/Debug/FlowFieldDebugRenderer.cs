using UnityEngine;
using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum FlowDebugMode
{
    None,
    Grid,
    Cost,
    Integration,
    Flow,
    Obstacles,
    Complete
}

public class FlowFieldDebugRenderer : MonoBehaviour
{
    public FlowField flowField;
    public FlowDebugMode debugMode = FlowDebugMode.Flow;

    [Header("Debug Height")] 
    public float debugHeightOffset = 0.2f; // Lowered slightly so it stays closer to the baked surface profile

    [Header("Visual Settings")] 
    public float arrowLength = 0.4f;
    public float cellPadding = 0.05f;

    [Header("Labels (Heavy Performance Cost)")]
    public bool drawCostLabels = false;
    public bool drawIntegrationLabels = false;
    public bool drawFlowLabels = false;
    public bool drawCellCoordinates = false;

#if UNITY_EDITOR
    private GUIStyle m_LabelStyle;
    private GUIStyle LabelStyle
    {
        get
        {
            if (m_LabelStyle == null)
            {
                m_LabelStyle = new GUIStyle(EditorStyles.miniLabel);
                m_LabelStyle.alignment = TextAnchor.MiddleCenter;
                m_LabelStyle.fontSize = 10;
                m_LabelStyle.normal.textColor = Color.white;
            }
            return m_LabelStyle;
        }
    }
#endif

    private void Start()
    {
            flowField = FlowFieldManager.Instance.ActiveFlowField;
    }

    private void OnDrawGizmos()
    {
        // Keep flowfield updated in editor
        if (flowField == null)
        {
            flowField = FlowFieldManager.Instance.ActiveFlowField;
        }

        if (flowField == null ) return;

        var grid = flowField.Grid;
        if (!grid.Cost.IsCreated) return;

#if UNITY_EDITOR
        // PERFORMANCE BOOST: Open GUI canvas exactly ONCE per frame instead of per-cell
        bool anyLabelsEnabled = drawCostLabels || drawIntegrationLabels || drawFlowLabels || drawCellCoordinates;
        if (anyLabelsEnabled) Handles.BeginGUI();
#endif

        for (int y = 0; y < grid.Height; y++)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                int index = x + y * grid.Width;
                float height = grid.TerrainHeight.IsCreated ? grid.TerrainHeight[index] : 0f;

                float3 center = new float3(
                    grid.Origin.x + (x + 0.5f) * grid.CellSize,
                    height + debugHeightOffset,
                    grid.Origin.y + (y + 0.5f) * grid.CellSize
                );

                // Run primary visualization blocks
                switch (debugMode)
                {
                    case FlowDebugMode.Grid:        DrawGridCell(grid, center); break;
                    case FlowDebugMode.Cost:        DrawCost(grid, index, center); break;
                    case FlowDebugMode.Integration: DrawIntegration(grid, index, center); break;
                    case FlowDebugMode.Flow:        DrawFlow(grid, index, center); break;
                    case FlowDebugMode.Obstacles:   DrawObstacle(grid, index, center); break;
                    case FlowDebugMode.Complete:    DrawComplete(grid, x, y, index, center); break;
                }

#if UNITY_EDITOR
                // Draw combined optimized dynamic labels
                if (anyLabelsEnabled)
                {
                    DrawLabelsOptimized(grid, x, y, index, center);
                }
#endif
            }
        }

#if UNITY_EDITOR
        if (anyLabelsEnabled) Handles.EndGUI();
#endif
    }

    private void DrawGridCell(FlowFieldGrid grid, float3 center)
    {
        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        Gizmos.DrawWireCube(center, new Vector3(grid.CellSize - cellPadding, 0.02f, grid.CellSize - cellPadding));
    }

    private void DrawObstacle(FlowFieldGrid grid, int index, float3 center)
    {
        if (grid.Cost[index] != byte.MaxValue) return;
        Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
        Gizmos.DrawCube(center, new Vector3(grid.CellSize * 0.9f, 0.1f, grid.CellSize * 0.9f));
    }

    private void DrawCost(FlowFieldGrid grid, int index, float3 center)
    {
        byte cost = grid.Cost[index];
        Gizmos.color = GetCostColor(cost);
        Gizmos.DrawCube(center, new Vector3(grid.CellSize - cellPadding, 0.02f, grid.CellSize - cellPadding));
    }

    private void DrawIntegration(FlowFieldGrid grid, int index, float3 center)
    {
        ushort value = grid.Integration[index];
        if (value == ushort.MaxValue)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawCube(center, new Vector3(grid.CellSize - cellPadding, 0.02f, grid.CellSize - cellPadding));
            return;
        }

        // Color ramp from target (White) to maximum path distance (Dark Blue/Black)
        float t = math.saturate(value / 300f);
        Gizmos.color = Color.Lerp(Color.white, new Color(0.1f, 0.1f, 0.3f, 1f), t);
        Gizmos.DrawCube(center, new Vector3(grid.CellSize - cellPadding, 0.02f, grid.CellSize - cellPadding));
    }

    private void DrawFlow(FlowFieldGrid grid, int index, float3 centerF)
    {
        if (grid.Cost[index] == byte.MaxValue)
        {
            DrawObstacle(grid, index, centerF);
            return;
        }

        float2 dir = grid.Flow[index];
        if (math.lengthsq(dir) < 0.0001f) return;

        Vector3 center = centerF;
        Vector3 to = center + new Vector3(dir.x, 0f, dir.y) * arrowLength;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(center, to);

        // FIXED: Reconstruct and finish the truncated arrow head rendering lines
        Vector3 back = (center - to).normalized;
        Vector3 left = Quaternion.Euler(0f, -30f, 0f) * back * (arrowLength * 0.3f);
        Vector3 right = Quaternion.Euler(0f, 30f, 0f) * back * (arrowLength * 0.3f);

        Gizmos.DrawLine(to, to + left);
        Gizmos.DrawLine(to, to + right);
    }

    private void DrawComplete(FlowFieldGrid grid, int x, int y, int index, float3 center)
    {
        DrawGridCell(grid, center);
        if (grid.Cost[index] == byte.MaxValue) DrawObstacle(grid, index, center);
        else DrawFlow(grid, index, center);
    }

#if UNITY_EDITOR
    private void DrawLabelsOptimized(FlowFieldGrid grid, int x, int y, int index, float3 worldPos)
    {
        // Don't waste CPU compute rendering labels if outside scene camera frame view
        Vector3 screenPos = HandleUtility.WorldToGUIPoint(worldPos);
        if (screenPos.x < 0 || screenPos.x > Screen.width || screenPos.y < 0 || screenPos.y > Screen.height) return;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        if (drawCellCoordinates) sb.AppendLine($"[{x},{y}]");
        if (drawCostLabels)      sb.AppendLine($"C:{(grid.Cost[index] == byte.MaxValue ? "X" : grid.Cost[index].ToString())}");
        if (drawIntegrationLabels) sb.AppendLine($"I:{(grid.Integration[index] == ushort.MaxValue ? "X" : grid.Integration[index].ToString())}");
        if (drawFlowLabels)      sb.AppendLine($"F:({grid.Flow[index].x:F1},{grid.Flow[index].y:F1})");

        string labelText = sb.ToString().TrimEnd();
        if (string.IsNullOrEmpty(labelText)) return;

        // Draw unified label container box
        Rect rect = new Rect(screenPos.x - 35, screenPos.y - 12, 70, 24);
        EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.6f));
        GUI.Label(rect, labelText, LabelStyle);
    }
#endif

    private Color GetCostColor(byte cost)
    {
        if (cost == byte.MaxValue) return new Color(1f, 0f, 0f, 0.5f); // Blocked Wall
        if (cost <= 1) return new Color(0f, 1f, 0f, 0.3f);             // Fast Road
        if (cost <= 3) return new Color(1f, 1f, 1f, 0.1f);             // Default Ground
        if (cost <= 10) return new Color(0.2f, 0.5f, 0.2f, 0.4f);       // Forest
        return new Color(0f, 0f, 1f, 0.4f);                            // Water / High Cost
    }
}

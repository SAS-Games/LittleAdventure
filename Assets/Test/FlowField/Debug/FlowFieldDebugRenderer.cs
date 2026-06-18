using System;
using UnityEngine;
using Unity.Mathematics;

public enum FlowDebugMode
{
    None,
    Grid,
    Cost,
    Integration,
    Flow
}

public class FlowFieldDebugRenderer : MonoBehaviour
{
    public FlowField flowField;

    public FlowDebugMode debugMode = FlowDebugMode.Flow;

    [Header("Visual Settings")]
    public float arrowLength = 0.4f;
    public float cellPadding = 0.05f;

    private void Start()
    {
        flowField = GetComponent<FlowFieldTestBootstrap>().flowField;
    }

    void OnDrawGizmos()
    {
        flowField = GetComponent<FlowFieldTestBootstrap>().flowField;

        if (flowField == null)
            return;

        var grid = flowField.Grid;

        for (int y = 0; y < grid.Height; y++)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                int index = x + y * grid.Width;

                float3 center = new float3(
                    grid.Origin.x + (x + 0.5f) * grid.CellSize,
                    0f,
                    grid.Origin.y + (y + 0.5f) * grid.CellSize
                );

                switch (debugMode)
                {
                    case FlowDebugMode.Grid:
                        DrawGridCell(grid, center);
                        break;

                    case FlowDebugMode.Cost:
                        DrawCost(grid, index, center);
                        break;

                    case FlowDebugMode.Integration:
                        DrawIntegration(grid, index, center);
                        break;

                    case FlowDebugMode.Flow:
                        DrawFlow(grid, index, center);
                        break;
                }
            }
        }
    }

    void DrawGridCell(FlowFieldGrid grid, float3 center)
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube(
            center,
            new Vector3(
                grid.CellSize - cellPadding,
                0.01f,
                grid.CellSize - cellPadding
            )
        );
    }

    void DrawCost(FlowFieldGrid grid, int index, float3 center)
    {
        byte cost = grid.Cost[index];

        if (cost == 255)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(
                center,
                new Vector3(
                    grid.CellSize - cellPadding,
                    0.01f,
                    grid.CellSize - cellPadding
                )
            );
        }
    }

    void DrawIntegration(FlowFieldGrid grid, int index, float3 center)
    {
        ushort value = grid.Integration[index];

        if (value == ushort.MaxValue)
            return;

        float t = math.saturate(value / 500f);

        Gizmos.color = Color.Lerp(Color.white, Color.black, t);

        Gizmos.DrawCube(
            center,
            new Vector3(
                grid.CellSize - cellPadding,
                0.01f,
                grid.CellSize - cellPadding
            )
        );
    }

    void DrawFlow(FlowFieldGrid grid, int index, float3 centerF)
    {
        float2 dir = grid.Flow[index];

        if (math.lengthsq(dir) < 0.0001f)
            return;

        Vector3 center = new Vector3(centerF.x, centerF.y, centerF.z);

        Gizmos.color = Color.cyan;

        Vector3 to = center + new Vector3(dir.x, 0f, dir.y) * arrowLength;

        Gizmos.DrawLine(center, to);
        Gizmos.DrawSphere(to, 0.05f);
    }
}

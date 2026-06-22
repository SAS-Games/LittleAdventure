using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public sealed class FlowField
{
    public FlowFieldGrid Grid { get; }
    private readonly IntegrationFieldBuilder integrationBuilder;

    public FlowField(FlowFieldGrid grid)
    {
        Grid = grid;
        integrationBuilder = new IntegrationFieldBuilder(grid);
    }

    public void Dispose()
    {
        integrationBuilder.Dispose();
        Grid.Dispose();
    }

    /// <summary>
    /// Build flow field toward a single target.
    /// </summary>
    public void Build(int2 target)
    {
        integrationBuilder.Build(target);
        BuildFlowDirections();
        DumpDebugInfo(target);
    }

    /// <summary>
    /// Build flow field toward multiple targets.
    /// </summary>
    public void Build(NativeArray<int2> targets)
    {
        integrationBuilder.Build(targets);
        BuildFlowDirections();
    }

    private void BuildFlowDirections()
    {
        var job = new BuildFlowFieldJob
        {
            width = Grid.Width,
            height = Grid.Height,
            integration = Grid.Integration,
            cost = Grid.Cost,
            flow = Grid.Flow
        };

        job.Schedule(Grid.CellCount, 64).Complete();
    }
    
     public void DumpDebugInfo(int2 targetCell)
     {
         DumpGridSummary();
         DumpCompleteGrid();
         DumpCostGrid();
         DumpTargetArea(targetCell, 1);
         DumpUnreachableCells();
         DumpSuspiciousCells();
         ValidateFlow();
     }
    
    private void DumpGridSummary()
    {
        Debug.Log(
            $"FlowField Summary\n" +
            $"Size: {Grid.Width} x {Grid.Height}\n" +
            $"Cells: {Grid.CellCount}\n");
    }

    private void DumpCompleteGrid()
    {
        var grid = Grid;

        var sb = new System.Text.StringBuilder();

        sb.AppendLine("===== COMPLETE GRID =====");
        sb.AppendLine("Format:");
        sb.AppendLine("[Cost|Integration|Flow]");
        sb.AppendLine();

        for (int y = grid.Height - 1; y >= 0; y--)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                int index = x + y * grid.Width;

                byte cost = grid.Cost[index];
                ushort integration = grid.Integration[index];
                float2 flow = grid.Flow[index];

                string costStr = cost == byte.MaxValue ? "XXX" : cost.ToString();
                string integrationStr = integration == ushort.MaxValue ? "XXX" : integration.ToString();

                string flowStr;

                if (math.lengthsq(flow) < 0.001f)
                {
                    flowStr = "•";
                }
                else
                {
                    int dx = (int)math.round(flow.x);
                    int dy = (int)math.round(flow.y);

                    flowStr =
                        dx == 0 && dy == 1 ? "↑" :
                        dx == 1 && dy == 1 ? "↗" :
                        dx == 1 && dy == 0 ? "→" :
                        dx == 1 && dy == -1 ? "↘" :
                        dx == 0 && dy == -1 ? "↓" :
                        dx == -1 && dy == -1 ? "↙" :
                        dx == -1 && dy == 0 ? "←" :
                        dx == -1 && dy == 1 ? "↖" :
                        "?";
                }

                sb.Append($"[{costStr,3}|{integrationStr,4}|{flowStr}] ");
            }

            sb.AppendLine();
        }

        Debug.Log(sb.ToString());
    }
    
    private void DumpUnreachableCells()
    {
        int count = 0;

        for (int i = 0; i < Grid.CellCount; i++)
        {
            if (Grid.Cost[i] != byte.MaxValue &&
                Grid.Integration[i] == ushort.MaxValue)
            {
                count++;
            }
        }

        Debug.Log($"Unreachable Walkable Cells: {count}");
    }
    
    private void DumpSuspiciousCells()
    {
        for (int y = 0; y < Grid.Height; y++)
        {
            for (int x = 0; x < Grid.Width; x++)
            {
                int index = x + y * Grid.Width;

                if (Grid.Cost[index] == byte.MaxValue)
                    continue;

                if (Grid.Integration[index] == ushort.MaxValue)
                    continue;

                if (Grid.Integration[index] == 0)
                    continue;

                if (math.lengthsq(Grid.Flow[index]) > 0.0001f)
                    continue;

                Debug.LogWarning(
                    $"Zero Flow Cell " +
                    $"({x},{y}) " +
                    $"Cost={Grid.Cost[index]} " +
                    $"Integration={Grid.Integration[index]}");
            }
        }
    }
    
    private void DumpTargetArea(int2 targetCell, int radius = 3)
    {
        int tx = targetCell.x;
        int ty = targetCell.y;

        var sb = new System.Text.StringBuilder();

        sb.AppendLine("===== TARGET AREA =====");

        for (int y = ty + radius; y >= ty - radius; y--)
        {
            for (int x = tx - radius; x <= tx + radius; x++)
            {
                if (x < 0 || x >= Grid.Width ||
                    y < 0 || y >= Grid.Height)
                {
                    sb.Append(" XXX ");
                    continue;
                }

                int index = x + y * Grid.Width;

                sb.Append($"{Grid.Integration[index],4}");
            }

            sb.AppendLine();
        }

        Debug.Log(sb.ToString());
    }
    
    private void ValidateFlow()
    {
        int errors = 0;

        for (int y = 0; y < Grid.Height; y++)
        {
            for (int x = 0; x < Grid.Width; x++)
            {
                int index = x + y * Grid.Width;

                if (Grid.Cost[index] == byte.MaxValue)
                    continue;

                ushort current = Grid.Integration[index];

                if (current == ushort.MaxValue || current == 0)
                    continue;

                float2 dir = Grid.Flow[index];
                if (math.lengthsq(dir) < 0.0001f)
                {
                    errors++;
                    continue;
                }
            }
        }

        Debug.Log($"Flow Validation Errors: {errors}");
    }
    
    public void DumpCostGrid()
    {
        var grid = Grid;

        var sb = new System.Text.StringBuilder();

        sb.AppendLine("===== COST GRID =====");

        for (int y = grid.Height - 1; y >= 0; y--)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                int i = x + y * grid.Width;
                sb.Append(grid.Cost[i] == byte.MaxValue ? "█ " : ". ");
            }

            sb.AppendLine();
        }

        Debug.Log(sb.ToString());
    }
}
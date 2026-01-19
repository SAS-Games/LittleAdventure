using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

public sealed class FlowField
{
    public FlowFieldGrid Grid { get; }

    IntegrationFieldBuilder integrationBuilder;

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
    /// Rebuild full flow field for given target
    /// </summary>
    public void Build(int2 target)
    {
        integrationBuilder.Build(target);

        BuildFlowDirections();
    }

    /// <summary>
    /// Multi-target support
    /// </summary>
    public void Build(NativeArray<int2> targets)
    {
        integrationBuilder.Build(targets);

        BuildFlowDirections();
    }

    void BuildFlowDirections()
    {
        var job = new FlowDirectionJob
        {
            width = Grid.Width,
            height = Grid.Height,
            integration = Grid.Integration,
            flow = Grid.Flow
        };

        job.Schedule(Grid.CellCount, 64).Complete();
    }
}
using Unity.Mathematics;

public static class FlowFieldBuilder
{
    public static FlowField Build(FlowFieldAsset asset, int2 targetCell)
    {
        FlowFieldGrid grid = FlowFieldGridFactory.Create(asset);
        FlowField flowField = new FlowField(grid);
        flowField.Build(targetCell);
        return flowField;
    }
}
public sealed class StaticInstancedBatch : GPUInstancedBatchBase
{
    protected override bool RequiresTransformAccess => false;
    protected override bool RequiresPerFrameUpload => false;

    protected override void UpdateInstanceData() { }
}

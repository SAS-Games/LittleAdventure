public sealed class TweenedBatch : GPUInstancedBatchBase
{
    protected override bool RequiresTransformAccess => false;
    protected override bool RequiresPerFrameUpload => true;

    public static TweenedBatch Instance { get; private set; }
    protected override void Awake()
    {
        Instance = this;
        base.Awake();
    }
    protected override void UpdateInstanceData()
    {
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Instance = null;
    }
}

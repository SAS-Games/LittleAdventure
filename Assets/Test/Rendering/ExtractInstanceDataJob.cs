using Unity.Burst;
using Unity.Collections;
using UnityEngine.Jobs;

[BurstCompile]
public struct ExtractInstanceDataJob : IJobParallelForTransform
{
    public NativeArray<InstanceRenderData> instanceData;
    
    public void Execute(int index, TransformAccess transform)
    {
        InstanceRenderData data = instanceData[index];
        data.objectToWorld = transform.localToWorldMatrix;
        instanceData[index] = data;
    }
}
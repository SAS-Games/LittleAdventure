using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Jobs;

[BurstCompile]
public struct TransformMotionApplyJob : IJobParallelForTransform
{
    [ReadOnly] public NativeArray<float3> pos;
    [ReadOnly] public NativeArray<quaternion> rot;

    public void Execute(int index, TransformAccess transform)
    {
        transform.position = pos[index];
        transform.rotation = rot[index];
    }
}
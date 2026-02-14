using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct TransformMotionUpdateJob : IJobParallelFor
{
    public float deltaTime;
    public NativeArray<TransformMotionState> states;
    public NativeArray<float3> outPos;
    public NativeArray<quaternion> outRot;
    public NativeQueue<int>.ParallelWriter completed;

    public void Execute(int index)
    {
        TransformMotionState s = states[index];

        float3 pos = s.currentPos;
        quaternion rot = s.currentRot;

        switch (s.phase)
        {
            case MotionPhase.StartDelay:
                {
                    pos = s.startPos;
                    rot = s.startRot;

                    if (s.startDelay <= 0f)
                    {
                        s.timer = 0f;
                        s.phase = MotionPhase.Forward;
                        goto case MotionPhase.Forward;
                    }

                    s.timer += deltaTime;

                    if (s.timer >= s.startDelay)
                    {
                        s.timer = 0f;
                        s.phase = MotionPhase.Forward;
                    }

                    break;
                }

            case MotionPhase.Forward:
                {
                    if (s.forwardTime <= 0f)
                    {
                        pos = s.endPos;
                        rot = s.endRot;
                        s.timer = 0f;
                        s.phase = MotionPhase.ReturnDelay;
                        goto case MotionPhase.ReturnDelay;
                    }

                    s.timer += deltaTime;

                    float t = math.saturate(s.timer / s.forwardTime);
                    float e = EaseUtility.Evaluate(s.EaseType, t);

                    pos = math.lerp(s.startPos, s.endPos, e);
                    rot = math.slerp(s.startRot, s.endRot, e);

                    if (t >= 1f)
                    {
                        s.timer = 0f;
                        s.phase = MotionPhase.ReturnDelay;

                        goto case MotionPhase.ReturnDelay;
                    }

                    break;
                }

            case MotionPhase.ReturnDelay:
                {
                    pos = s.endPos;
                    rot = s.endRot;

                    if (s.returnDelay <= 0f)
                    {
                        s.timer = 0f;
                        s.phase = MotionPhase.Return;
                        goto case MotionPhase.Return;
                    }

                    s.timer += deltaTime;

                    if (s.timer >= s.returnDelay)
                    {
                        s.timer = 0f;
                        s.phase = MotionPhase.Return;
                    }

                    break;
                }

            case MotionPhase.Return:
                {
                    if (s.returnTime <= 0f)
                    {
                        pos = s.startPos;
                        rot = s.startRot;
                        s.phase = MotionPhase.Completed;
                        completed.Enqueue(index);
                        break;
                    }

                    s.timer += deltaTime;

                    float t = math.saturate(s.timer / s.returnTime);
                    float e = EaseUtility.Evaluate(s.EaseType, t);

                    pos = math.lerp(s.endPos, s.startPos, e);
                    rot = math.slerp(s.endRot, s.startRot, e);

                    if (t >= 1f)
                    {
                        pos = s.startPos;
                        rot = s.startRot;
                        s.phase = MotionPhase.Completed;
                        completed.Enqueue(index);
                    }

                    break;
                }

            case MotionPhase.Completed:
                {
                    outPos[index] = s.currentPos;
                    outRot[index] = s.currentRot;
                    states[index] = s;
                    return;
                }
        }

        s.currentPos = pos;
        s.currentRot = rot;

        outPos[index] = pos;
        outRot[index] = rot;

        states[index] = s;
    }
}

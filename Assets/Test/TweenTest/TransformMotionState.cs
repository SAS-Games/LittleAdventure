using Unity.Mathematics;

public struct TransformMotionState
{
    public float3 startPos;
    public float3 endPos;

    public quaternion startRot;
    public quaternion endRot;

    public float forwardTime;
    public float returnTime;

    public float startDelay;
    public float returnDelay;

    public float timer;

    public MotionPhase phase;
    public EaseType EaseType;

    public float3 currentPos;
    public quaternion currentRot;
}


public enum MotionPhase
{
    StartDelay,
    Forward,
    ReturnDelay,
    Return,
    Completed
}


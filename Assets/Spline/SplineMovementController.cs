using System;
using UnityEngine;
using UnityEngine.Splines;

public class SplineMovementController
{
    public enum LoopMode
    {
        None, Loop, PingPong
    }

    public SplinePath Path { get; private set; }
    public float DistanceTravelled { get; private set; }
    public float NormalizedProgress => Mathf.Clamp01(DistanceTravelled / PathLength);
    public float PathLength { get; private set; }

    private float _baseSpeed;

    private AnimationCurve _speedCurve;
    private LoopMode _loopMode;
    private bool _forward = true;
    private bool _isPaused = false;

    public float Speed
    {
        get => _baseSpeed;
        set => _baseSpeed = Mathf.Max(0f, value); // prevent negative speeds
    }
    public bool IsPaused => _isPaused;
    public bool IsCompleted { get; private set; }
    public Action OnTraversalComplete;

    public void SetPath(SplinePath path, float pathLength, float baseSpeed, AnimationCurve speedCurve, LoopMode loopMode)
    {
        Path = path;
        PathLength = pathLength;
        _baseSpeed = baseSpeed;
        _speedCurve = speedCurve;
        _loopMode = loopMode;
        IsCompleted = false;
        DistanceTravelled = _forward ? 0f : pathLength;
    }

    public void UpdateMovement(float deltaTime)
    {
        if (_isPaused)
            return;

        float adjustedSpeed = Speed * _speedCurve.Evaluate(NormalizedProgress);
        float delta = adjustedSpeed * deltaTime;

        DistanceTravelled += _forward ? delta : -delta;

        if (DistanceTravelled >= PathLength || DistanceTravelled <= 0f)
        {
            switch (_loopMode)
            {
                case LoopMode.None:
                    DistanceTravelled = Mathf.Clamp(DistanceTravelled, 0f, PathLength);
                    IsCompleted = true;
                    OnTraversalComplete?.Invoke();
                    break;

                case LoopMode.Loop:
                    DistanceTravelled = _forward ? 0f : PathLength;
                    OnTraversalComplete?.Invoke();
                    break;

                case LoopMode.PingPong:
                    _forward = !_forward;
                    DistanceTravelled = Mathf.Clamp(DistanceTravelled, 0f, PathLength);
                    OnTraversalComplete?.Invoke();
                    break;
            }
        }
    }

    public Vector3 EvaluatePosition() => Path.EvaluatePosition(NormalizedProgress);
    public Vector3 EvaluateDirection() => Path.EvaluateTangent(NormalizedProgress);
    public void Pause() => _isPaused = true;
    public void Resume() => _isPaused = false;
}

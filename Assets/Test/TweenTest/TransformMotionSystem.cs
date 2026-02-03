using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

public sealed class TransformMotionSystem : MonoBehaviour
{
    public System.Action<Transform> OnTweenCompleted;
    public static TransformMotionSystem Instance;

    private readonly List<Transform> _transforms = new();
    private readonly Dictionary<int, int> _indexMap = new();

    private TransformAccessArray _transformAccess;

    private NativeArray<TransformMotionState> _states;
    private NativeArray<float3> _outPos;
    private NativeArray<quaternion> _outRot;
    private NativeQueue<int> _completedIndices;
    private readonly List<int> _completionBuffer = new();


    private void Awake()
    {
        Instance = this;
        _transformAccess = new TransformAccessArray(64);
        _completedIndices = new NativeQueue<int>(Allocator.Persistent);
    }

    private void FixedUpdate()
    {
        if (_transforms.Count == 0)
            return;

        var updateJob = new TransformMotionUpdateJob
        {
            deltaTime = Time.fixedDeltaTime,
            states = _states,
            outPos = _outPos,
            outRot = _outRot,
            completed = _completedIndices.AsParallelWriter()
        };

        var updateJobHandle = updateJob.Schedule(_transforms.Count, 64);

        var applyJob = new TransformMotionApplyJob
        {
            pos = _outPos,
            rot = _outRot
        };

        JobHandle applyJobHandle = applyJob.Schedule(_transformAccess, updateJobHandle);
        applyJobHandle.Complete();

        ProcessCompletions();
    }

    private void ProcessCompletions()
    {
        if (_completedIndices.IsEmpty())
            return;

        _completionBuffer.Clear();

        while (_completedIndices.TryDequeue(out int index))
            _completionBuffer.Add(index);

        _completionBuffer.Sort((a, b) => b.CompareTo(a));

        foreach (int i in _completionBuffer)
        {
            if (i >= 0 && i < _transforms.Count)
            {
                OnTweenCompleted?.Invoke(_transforms[i]);
                RemoveAtSwapBack(i);
            }
        }

        _completedIndices.Clear();
    }


    private void OnDestroy()
    {
        Instance = null;
        if (_states.IsCreated) _states.Dispose();
        if (_outPos.IsCreated) _outPos.Dispose();
        if (_outRot.IsCreated) _outRot.Dispose();
        if (_transformAccess.isCreated) _transformAccess.Dispose();
        if (_completedIndices.IsCreated) _completedIndices.Dispose();
    }

    public void Register(Transform cube, Vector3 endPos, Quaternion endRot, float deformTime, float delay,
        float restoreTime, float restoreDelay, EaseType easeType)
    {
        Register(cube, cube.position, endPos, cube.rotation, endRot, deformTime, delay, restoreTime, restoreDelay,
            easeType);
    }

    public void Register(Transform t, Vector3 startPos, Vector3 endPos, Quaternion startRot, Quaternion endRot,
        float deformTime, float delay, float restoreTime, float restoreDelay, EaseType easeType)
    {
        int id = t.GetInstanceID();

        if (_indexMap.TryGetValue(id, out int index))
            return;

        var s = new TransformMotionState
        {
            startPos = startPos,
            endPos = endPos,
            startRot = startRot,
            endRot = endRot,

            forwardTime = deformTime,
            returnTime = restoreTime,

            startDelay = delay,
            returnDelay = restoreDelay,

            timer = 0f,
            phase = MotionPhase.StartDelay,
            EaseType = easeType,

            currentPos = startPos,
            currentRot = startRot
        };

        _indexMap[id] = _transforms.Count;
        _transforms.Add(t);
        _transformAccess.Add(t);

        ResizeArraysIfNeeded();

        int i = _transforms.Count - 1;
        _states[i] = s;
    }

    public void Unregister(Transform t)
    {
        if (Instance == null)
            return;

        if (!_indexMap.TryGetValue(t.GetInstanceID(), out int index))
            return;

        RemoveAtSwapBack(index);
    }

    public bool IsActive(Transform cube)
    {
        return _indexMap.ContainsKey(cube.GetInstanceID());
    }

    private void RemoveAtSwapBack(int index)
    {
        int last = _transforms.Count - 1;

        Transform removed = _transforms[index];

        if (last != index)
        {
            Transform lastTransform = _transforms[last];
            _transforms[index] = lastTransform;

            _states[index] = _states[last];
            _indexMap[lastTransform.GetInstanceID()] = index;
        }

        _states[last] = default;
        _indexMap.Remove(removed.GetInstanceID());
        _transformAccess.RemoveAtSwapBack(index);
        _transforms.RemoveAt(last);
    }

    private void ResizeArraysIfNeeded()
    {
        int count = _transforms.Count;

        if (_states.IsCreated && _states.Length >= count)
            return;

        int newSize = math.max(count, _states.IsCreated ? _states.Length * 2 : 64);

        NativeArray<TransformMotionState> newStates =
            new NativeArray<TransformMotionState>(newSize, Allocator.Persistent);
        NativeArray<float3> newOutPos = new NativeArray<float3>(newSize, Allocator.Persistent);
        NativeArray<quaternion> newOutRot = new NativeArray<quaternion>(newSize, Allocator.Persistent);

        if (_states.IsCreated)
        {
            NativeArray<TransformMotionState>.Copy(_states, newStates, _states.Length);
            NativeArray<float3>.Copy(_outPos, newOutPos, _outPos.Length);
            NativeArray<quaternion>.Copy(_outRot, newOutRot, _outRot.Length);

            _states.Dispose();
            _outPos.Dispose();
            _outRot.Dispose();
        }

        _states = newStates;
        _outPos = newOutPos;
        _outRot = newOutRot;
    }
}
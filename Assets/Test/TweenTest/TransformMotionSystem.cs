using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

public sealed class TransformMotionSystem : MonoBehaviour
{
    public static TransformMotionSystem Instance;

    private readonly List<Transform> _transforms = new();
    private readonly Dictionary<Transform, int> _indexMap = new();

    private TransformAccessArray _transformAccess;

    private NativeArray<TransformMotionState> _states;
    private NativeArray<float3> _outPos;
    private NativeArray<quaternion> _outRot;
    private NativeQueue<int> _completedIndices = new();
    private int _nextHandleId = 1;

    private void Awake()
    {
        Instance = this;
        _transformAccess = new TransformAccessArray(64);
        _completedIndices = new NativeQueue<int>(Allocator.Persistent);
    }

    public void RegisterCube(Transform cube, Vector3 endPos, Quaternion endRot, float deformTime, float delay,
        float restoreTime, float restoreDelay, EaseType easeType)
    {
        RegisterCube(cube, cube.position, endPos, cube.rotation, endRot, deformTime, delay, restoreTime, restoreDelay,
            easeType);
    }

    public void RegisterCube(Transform cube, Vector3 startPos, Vector3 endPos, Quaternion startRot, Quaternion endRot,
        float deformTime, float delay, float restoreTime, float restoreDelay, EaseType easeType)
    {
        if (_indexMap.ContainsKey(cube))
            return;

        int handleId = _nextHandleId++;

        var s = new TransformMotionState
        {
            handleId = handleId,
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

        _indexMap[cube] = _transforms.Count;
        _transforms.Add(cube);
        _transformAccess.Add(cube);

        ResizeArraysIfNeeded();

        int i = _transforms.Count - 1;
        _states[i] = s;
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


    public bool IsActive(Transform cube)
    {
        return _indexMap.ContainsKey(cube);
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


        // for (int i = _transforms.Count - 1; i >= 0; i--)
        // {
        //     if (_states[i].phase == MotionPhase.Completed)
        //     {
        //         RemoveAtSwapBack(i);
        //     }
        // }

        while (_completedIndices.TryDequeue(out int index))
        {
            if (index < _transforms.Count)
                RemoveAtSwapBack(index);
        }
    }

    private void RemoveAtSwapBack(int index)
    {
        int last = _transforms.Count - 1;

        Transform removed = _transforms[index];
        Transform lastTransform = _transforms[last];

        int removedHandle = _states[index].handleId;
        int lastHandle = _states[last].handleId;

        _transforms[index] = lastTransform;
        _states[index] = _states[last];

        _indexMap[lastTransform] = index;
        _indexMap.Remove(removed);

        _transformAccess.RemoveAtSwapBack(index);
        _transforms.RemoveAt(last);
    }


    public void Unregister(Transform t)
    {
        if (Instance == null)
            return;
        
        if (!_indexMap.TryGetValue(t, out int index))
            return;

        RemoveAtSwapBack(index);
    }

    private void OnDestroy()
    {
        Instance = null;
        if (_states.IsCreated) _states.Dispose();
        if (_outPos.IsCreated) _outPos.Dispose();
        if (_outRot.IsCreated) _outRot.Dispose();
        if (_transformAccess.isCreated) _transformAccess.Dispose();
    }
}
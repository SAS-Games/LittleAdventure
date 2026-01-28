using System;
using SAS.TweenManagement;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class TweenTest : MonoBehaviour
{
    [Header("Grid")] public GameObject cubePrefab;
    public int gridX = 10;
    public int gridZ = 10;
    public float spacing = 1.2f;

    [Header("Tween")] public TweenConfig config;
    public TweenConfig configBack;
    public int randomPickCount = 10;

    [Header("Input")] public InputAction inputAction;

    private readonly List<Transform> _cubes = new();
    private readonly HashSet<Transform> _activeTweens = new();


    void OnEnable()
    {
        if (inputAction != null)
        {
            inputAction.performed += OnActionPerformed;
            inputAction.Enable();
        }
    }

    void OnDisable()
    {
        if (inputAction != null)
        {
            inputAction.performed += OnActionPerformed;
            inputAction.Disable();
        }
    }

    private void FixedUpdate()
    {
         //PlayRandomTweens(randomPickCount);
        PlayDeformationTest(randomPickCount);
    }


    private void OnActionPerformed(InputAction.CallbackContext ctx)
    {
        // PlayRandomTweens(randomPickCount);
        PlayDeformationTest(randomPickCount);
    }


    void Start()
    {
        CreateGrid();
        TransformMotionSystem.Instance.OnTweenCompleted = OnTweenCompleted;
    }


    void PlayRandomTweens(int count)
    {
        if (_cubes.Count == 0)
            return;

        int attempts = 0;
        int played = 0;

        while (played < count && attempts < _cubes.Count * 2)
        {
            attempts++;

            Transform cube = _cubes[Random.Range(0, _cubes.Count)];

            if (_activeTweens.Contains(cube))
                continue;

            PlayTween(cube);
            played++;
        }
    }


    async void PlayTween(Transform cube)
    {
        DynamicInstancedBatch.Instance.SetColor(cube, GetRandomColor());
        _activeTweens.Add(cube);

        Vector3 startPos = cube.position;
        Quaternion startRot = cube.rotation;

        Vector3 targetPos = startPos + Vector3.up;
        Quaternion targetRot = startRot * Quaternion.Euler(0f, 180f, 0f);

        // forward tweens
        var move = Tween.SetPositionAndRotation(cube, targetPos, targetRot, config);

        // var move = Tween.Move(cube, targetPos, config);
        // var rot = Tween.Rotation(cube, targetRot, config);

        await move;
        var moveBack = Tween.SetPositionAndRotation(cube, startPos, startRot, configBack);

        // var moveBack = Tween.Move(cube, startPos, configBack);
        // var rotBack = Tween.Rotation(cube, startRot, configBack);

        await moveBack;
        DynamicInstancedBatch.Instance.SetColor(cube, Color.white);
        _activeTweens.Remove(cube);
    }

    void PlayDeformationTest(int count)
    {
        if (_cubes.Count == 0)
            return;

        int attempts = 0;
        int played = 0;

        while (played < count && attempts < _cubes.Count * 2)
        {
            attempts++;

            Transform cube = _cubes[Random.Range(0, _cubes.Count)];

            // avoid double registration
            if (cube == null)
                continue;
            
            if (TransformMotionSystem.Instance.IsActive(cube))
                continue;
           
            Vector3 startPos = cube.position;
            Quaternion startRot = cube.rotation;

            Vector3 targetPos = startPos + Vector3.up;
            Quaternion targetRot = startRot * Quaternion.Euler(0f, 180f, 0f);

            TransformMotionSystem.Instance.RegisterCube(cube, targetPos, targetRot, config.DurationOrSpeed, config.Delay, configBack.DurationOrSpeed, configBack.Delay, EaseType.EaseOutQuad);
            DynamicInstancedBatch.Instance.SetColor(cube, GetRandomColor());

            played++;
        }
    }


    void CreateGrid()
    {
        _cubes.Clear();

        for (int x = 0; x < gridX; x++)
        {
            for (int z = 0; z < gridZ; z++)
            {
                Vector3 pos = new Vector3(
                    x * spacing,
                    0f,
                    z * spacing
                );

                var cube = Instantiate(
                    cubePrefab,
                    pos,
                    Quaternion.identity,
                    transform
                );

                _cubes.Add(cube.transform);
            }
        }
    }
    
    public static Color GetRandomColor()
    {
        return new Color(Random.value, Random.value, Random.value, 1f);
    }

    private void OnTweenCompleted(Transform tween)
    {
        DynamicInstancedBatch.Instance.SetColor(tween, Color.white);
    }
}
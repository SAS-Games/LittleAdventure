using SAS.TweenManagement;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TweenTest : MonoBehaviour
{
    [Header("Grid")]
    public GameObject cubePrefab;
    public int gridX = 10;
    public int gridZ = 10;
    public float spacing = 1.2f;

    [Header("Tween")]
    public TweenConfig config;
    public TweenConfig configBack;
    public int randomPickCount = 10;

    [Header("Input")]
    public InputAction inputAction;

    private readonly List<Transform> _cubes = new();
    private readonly HashSet<Transform> _activeTweens = new();

    // ------------------------------------------------------

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

    // ------------------------------------------------------

    private void OnActionPerformed(InputAction.CallbackContext ctx)
    {
        PlayRandomTweens(randomPickCount);
    }

    // ------------------------------------------------------

    void Start()
    {
        CreateGrid();
    }

    // ------------------------------------------------------

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
        _activeTweens.Add(cube);

        Vector3 startPos = cube.position;
        Quaternion startRot = cube.rotation;

        Vector3 targetPos = startPos + Vector3.up;
        Quaternion targetRot = startRot * Quaternion.Euler(0f, 180f, 0f);

        // forward tweens
        ITween move = Tween.Move(cube, targetPos, config);
        ITween rot = Tween.Rotation(cube, targetRot, config);

        await move;

        ITween moveBack = Tween.Move(cube, startPos, configBack);
        ITween rotBack = Tween.Rotation(cube, startRot, configBack);

        await moveBack;
        _activeTweens.Remove(cube);
    }

    //async void OnForwardComplete(ITween tween)
    //{
    //    Transform cube = (Transform)tween.UserData;

    //    Vector3 startPos = cube.position - Vector3.up;
    //    Quaternion startRot = cube.rotation * Quaternion.Euler(0f, -180f, 0f);

    //    // backward tweens
    //    ITween moveBack = Tween.Move(cube, startPos, configBack);
    //    ITween rotBack = Tween.Rotation(cube, startRot, configBack);

    //    moveBack.UserData = cube;
    //    await moveBack;
    //    OnBackwardComplete(moveBack);
    //}

    //void OnBackwardComplete(ITween tween)
    //{
    //    Transform cube = (Transform)tween.UserData;
    //    _activeTweens.Remove(cube);
    //}




    // ------------------------------------------------------

    //void PlayTween(Transform cube)
    //{
    //    _activeTweens.Add(cube);

    //    Vector3 startPos = cube.position;
    //    Vector3 targetPos = startPos + Vector3.up;

    //    Quaternion startRot = cube.rotation;
    //    Quaternion targetRot = startRot * Quaternion.Euler(0f, 180f, 0f);


    //    Tween.Rotation(cube, targetRot, config);
    //    Tween.Move(cube, targetPos, config)
    //    .AddCallback(() =>
    //    {
    //        Tween.Rotation(cube, startRot, configBack);
    //        Tween.Move(cube, startPos, configBack).AddCallback(() => _activeTweens.Remove(cube));
    //    });
    //}

    //ITween tween = Tween.CreateTween(0f, 1f, val => SetPositionAndRotation(cube, startPos, targetPos, startRot, targetRot, val), config);

    //tween.AddCallback(() =>
    //{
    //    ITween tweenBack = Tween.CreateTween(0f, 1f, val => SetPositionAndRotation(cube, targetPos, startPos, targetRot, startRot, val), configBack);
    //    tweenBack.AddCallback(() =>
    //    {
    //        _activeTweens.Remove(cube);
    //    });
    //    tweenBack.Run();
    //});

    //tween.Run();

    void SetPositionAndRotation(Transform transform, Vector3 startPos, Vector3 targetPos, Quaternion startRot, Quaternion targetRot, float t)
    {
        transform.position = Vector3.Lerp(startPos, targetPos, t);
        transform.rotation = Quaternion.Lerp(startRot, targetRot, t);
    }

    // ------------------------------------------------------

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
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class SplineFollow : MonoBehaviour
{
    [SerializeField] private Transform m_Target;
    [SerializeField] private SplineContainer m_Container;
    [SerializeField] private float baseSpeed = 1f;
    [SerializeField] private AnimationCurve m_SpeedCurve = AnimationCurve.Linear(0, 1, 1, 1);
    [SerializeField] private SplinePathData m_PathData;
    [SerializeField] private SplineMovementController.LoopMode m_LoopMode = SplineMovementController.LoopMode.Loop;
    private ISplineFeedback[] _feedbackComponents;


    private SplineMovementController _movementController;
    public SplineContainer Container => m_Container;
    public SplinePathData PathData => m_PathData;

    public SplineMovementController Controller => _movementController;
    private void Start()
    {
        _feedbackComponents = GetComponents<ISplineFeedback>();
        _movementController = new SplineMovementController();
        SetupPath();
    }

    private void SetupPath()
    {
        var path = SplinePathBuilder.BuildPath(m_Container, m_PathData.slices.ToList(), out var totalLength);
        _movementController.SetPath(path, totalLength, baseSpeed, m_SpeedCurve, m_LoopMode);
    }

    private void Update()
    {
        if (_movementController.IsPaused || _movementController.IsCompleted)
            return;

        _movementController.UpdateMovement(Time.deltaTime);
        Vector3 position = _movementController.EvaluatePosition();
        Vector3 direction = _movementController.EvaluateDirection();
        m_Target.position = position;
        m_Target.LookAt(transform.position + direction);

        foreach (var feedback in _feedbackComponents)
            feedback.EvaluateFeedback(position, direction);
    }

    public void SwitchTo(SplinePath path, float pathLength)
    {
        _movementController.SetPath(path, pathLength, baseSpeed, m_SpeedCurve, m_LoopMode);
    }
}

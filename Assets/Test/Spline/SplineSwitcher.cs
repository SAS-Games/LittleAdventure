using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class SplineSwitcher : MonoBehaviour
{
    [SerializeField] private bool m_WaitForCurrentPathToComplete = true;

    private SplinePath _nextPath;
    private float _nextPathLength;

    public void SwitchToPath(SplineFollow splineFollow, List<int> activePathDataIndices)
    {
        var activeSet = new HashSet<int>(activePathDataIndices);
        for (int i = 0; i < splineFollow.PathData.slices.Length; i++)
        {
            splineFollow.PathData.slices[i].isEnabled = activeSet.Contains(i);
        }

        // Build the new spline path
        _nextPath = SplinePathBuilder.BuildPath(splineFollow.Container,
            splineFollow.PathData.slices.ToList(), out _nextPathLength);

        if (!m_WaitForCurrentPathToComplete)
        {
            splineFollow.SwitchTo(_nextPath, _nextPathLength);
            _nextPath = null;
        }
        else
        {
            // Subscribe once to traversal completion
            splineFollow.Controller.OnTraversalComplete += OnTraversalCompleteHandler;

            void OnTraversalCompleteHandler()
            {
                splineFollow.Controller.OnTraversalComplete -= OnTraversalCompleteHandler;
                splineFollow.SwitchTo(_nextPath, _nextPathLength);
                _nextPath = null;
            }
        }
    }

}

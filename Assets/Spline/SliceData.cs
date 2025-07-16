using UnityEngine.Splines;

[System.Serializable]
public class SplinePathData
{
    public SliceData[] slices;
}

[System.Serializable]
public class SliceData
{
    public int splineIndex;
    public SplineRange range;

    // Can store more useful information
    public bool isEnabled = true;
    public float sliceLength;
    public float distanceFromStart;
}
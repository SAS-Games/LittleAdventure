using System.Collections.Generic;
using System.Linq;
using UnityEngine.Splines;

public static class SplinePathBuilder
{
    public static SplinePath BuildPath(SplineContainer container, List<SliceData> slices, out float totalLength)
    {
        var localToWorld = container.transform.localToWorldMatrix;
        var enabledSlices = slices.Where(s => s.isEnabled).ToList();
        var splineSlices = new List<SplineSlice<Spline>>();
        totalLength = 0f;

        foreach (var slice in enabledSlices)
        {
            var spline = container.Splines[slice.splineIndex];
            var splineSlice = new SplineSlice<Spline>(spline, slice.range, localToWorld);
            slice.sliceLength = splineSlice.GetLength();
            slice.distanceFromStart = totalLength;
            totalLength += slice.sliceLength;
            splineSlices.Add(splineSlice);
        }

        return new SplinePath(splineSlices);
    }
}


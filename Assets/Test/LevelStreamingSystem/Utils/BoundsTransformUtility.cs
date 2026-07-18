using UnityEngine;

namespace LevelStreaming
{
    public static class BoundsTransformUtility
    {
        /// <summary>
        /// Transforms an axis-aligned bounds and returns the axis-aligned bounds that
        /// encloses it in the destination space. Handles rotation and non-uniform or
        /// negative scale.
        /// </summary>
        public static Bounds Transform(Bounds source, Matrix4x4 matrix)
        {
            Vector3 center = matrix.MultiplyPoint3x4(source.center);
            Vector3 extents = source.extents;

            Vector3 axisX = Abs(matrix.MultiplyVector(new Vector3(extents.x, 0f, 0f)));
            Vector3 axisY = Abs(matrix.MultiplyVector(new Vector3(0f, extents.y, 0f)));
            Vector3 axisZ = Abs(matrix.MultiplyVector(new Vector3(0f, 0f, extents.z)));
            return new Bounds(center, (axisX + axisY + axisZ) * 2f);
        }

        public static Bounds InverseTransform(Bounds source, Matrix4x4 localToWorld)
        {
            return Transform(source, localToWorld.inverse);
        }

        private static Vector3 Abs(Vector3 value)
        {
            return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
        }
    }
}

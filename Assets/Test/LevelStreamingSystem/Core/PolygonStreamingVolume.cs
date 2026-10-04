using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace LevelStreaming
{
    /// <summary>
    /// A world-space polygon on the XY plane, extruded between two Z coordinates.
    /// The polygon may be concave, but must be simple (its edges may not cross).
    /// </summary>
    [Serializable]
    public sealed class PolygonStreamingVolume : StreamingVolume
    {
        private const float GeometryEpsilon = 0.00001f;

        [SerializeField] private List<Vector2> vertices = new()
        {
            new Vector2(-1f, -1f),
            new Vector2(-1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, -1f)
        };
        [FormerlySerializedAs("minY")]
        [SerializeField] private float minZ = -1f;
        [FormerlySerializedAs("maxY")]
        [SerializeField] private float maxZ = 1f;

        public PolygonStreamingVolume() { }

        public PolygonStreamingVolume(Bounds bounds)
        {
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            vertices = new List<Vector2>
            {
                new(min.x, min.y),
                new(min.x, max.y),
                new(max.x, max.y),
                new(max.x, min.y)
            };
            minZ = min.z;
            maxZ = max.z;
        }

        public PolygonStreamingVolume(IEnumerable<Vector2> vertices, float minZ, float maxZ)
        {
            this.vertices = vertices != null ? new List<Vector2>(vertices) : new List<Vector2>();
            this.minZ = minZ;
            this.maxZ = maxZ;
        }

        public IReadOnlyList<Vector2> Vertices => vertices;
        public float MinZ { get => minZ; set => minZ = value; }
        public float MaxZ { get => maxZ; set => maxZ = value; }

        public bool IsValid =>
            vertices != null &&
            vertices.Count >= 3 &&
            IsFinite(minZ) &&
            IsFinite(maxZ) &&
            maxZ > minZ &&
            Mathf.Abs(SignedArea(vertices)) > GeometryEpsilon &&
            AreVerticesFinite(vertices) &&
            !HasSelfIntersections(vertices);

        public override Bounds BroadphaseBounds
        {
            get
            {
                if (vertices == null || vertices.Count == 0 || !IsFinite(minZ) || !IsFinite(maxZ))
                    return default;

                bool foundVertex = false;
                Vector2 min = default;
                Vector2 max = default;
                foreach (Vector2 vertex in vertices)
                {
                    if (!IsFinite(vertex))
                        continue;

                    if (!foundVertex)
                    {
                        min = max = vertex;
                        foundVertex = true;
                    }
                    else
                    {
                        min = Vector2.Min(min, vertex);
                        max = Vector2.Max(max, vertex);
                    }
                }

                if (!foundVertex)
                    return default;

                float back = Mathf.Min(minZ, maxZ);
                float front = Mathf.Max(minZ, maxZ);
                return new Bounds(
                    new Vector3((min.x + max.x) * 0.5f, (min.y + max.y) * 0.5f, (back + front) * 0.5f),
                    new Vector3(max.x - min.x, max.y - min.y, front - back));
            }
        }

        public override bool Contains(Vector3 point)
        {
            if (!IsValid || point.z < minZ || point.z > maxZ)
                return false;

            return ContainsPoint(vertices, new Vector2(point.x, point.y));
        }

        public override bool TryIntersects(IStreamingVolume other, out bool intersects)
        {
            switch (other)
            {
                case BoxStreamingVolume box:
                    intersects = IntersectsBox(box.Bounds);
                    return true;
                case PolygonStreamingVolume polygon:
                    intersects = IntersectsPolygon(polygon);
                    return true;
                default:
                    intersects = false;
                    return false;
            }
        }

        public void SetVertex(int index, Vector2 value)
        {
            if (vertices == null)
                vertices = new List<Vector2>();
            vertices[index] = value;
        }

        public void AddVertex(Vector2 value)
        {
            vertices ??= new List<Vector2>();
            vertices.Add(value);
        }

        public void InsertVertex(int index, Vector2 value)
        {
            vertices ??= new List<Vector2>();
            vertices.Insert(index, value);
        }

        public void RemoveVertexAt(int index) => vertices?.RemoveAt(index);

        public bool HasSelfIntersections() => vertices != null && HasSelfIntersections(vertices);

        private bool IntersectsBox(Bounds bounds)
        {
            if (!IsValid || bounds.max.z < minZ || bounds.min.z > maxZ)
                return false;

            Vector2 rectangleMin = new(bounds.min.x, bounds.min.y);
            Vector2 rectangleMax = new(bounds.max.x, bounds.max.y);
            return PolygonIntersectsRectangle(vertices, rectangleMin, rectangleMax);
        }

        private bool IntersectsPolygon(PolygonStreamingVolume other)
        {
            if (!IsValid || other == null || !other.IsValid ||
                other.maxZ < minZ || other.minZ > maxZ)
                return false;

            return PolygonsIntersect(vertices, other.vertices);
        }

        private static bool PolygonIntersectsRectangle(IReadOnlyList<Vector2> polygon,
            Vector2 rectangleMin, Vector2 rectangleMax)
        {
            for (int i = 0; i < polygon.Count; i++)
            {
                Vector2 vertex = polygon[i];
                if (vertex.x >= rectangleMin.x && vertex.x <= rectangleMax.x &&
                    vertex.y >= rectangleMin.y && vertex.y <= rectangleMax.y)
                    return true;
            }

            var corners = new[]
            {
                rectangleMin,
                new Vector2(rectangleMin.x, rectangleMax.y),
                rectangleMax,
                new Vector2(rectangleMax.x, rectangleMin.y)
            };

            for (int i = 0; i < corners.Length; i++)
            {
                if (ContainsPoint(polygon, corners[i]))
                    return true;
            }

            for (int polygonIndex = 0; polygonIndex < polygon.Count; polygonIndex++)
            {
                Vector2 polygonStart = polygon[polygonIndex];
                Vector2 polygonEnd = polygon[(polygonIndex + 1) % polygon.Count];
                for (int rectangleIndex = 0; rectangleIndex < corners.Length; rectangleIndex++)
                {
                    Vector2 rectangleStart = corners[rectangleIndex];
                    Vector2 rectangleEnd = corners[(rectangleIndex + 1) % corners.Length];
                    if (SegmentsIntersect(polygonStart, polygonEnd, rectangleStart, rectangleEnd))
                        return true;
                }
            }

            return false;
        }

        private static bool PolygonsIntersect(IReadOnlyList<Vector2> first, IReadOnlyList<Vector2> second)
        {
            for (int firstIndex = 0; firstIndex < first.Count; firstIndex++)
            {
                Vector2 firstStart = first[firstIndex];
                Vector2 firstEnd = first[(firstIndex + 1) % first.Count];
                for (int secondIndex = 0; secondIndex < second.Count; secondIndex++)
                {
                    Vector2 secondStart = second[secondIndex];
                    Vector2 secondEnd = second[(secondIndex + 1) % second.Count];
                    if (SegmentsIntersect(firstStart, firstEnd, secondStart, secondEnd))
                        return true;
                }
            }

            return ContainsPoint(first, second[0]) || ContainsPoint(second, first[0]);
        }

        private static bool ContainsPoint(IReadOnlyList<Vector2> polygon, Vector2 point)
        {
            bool inside = false;
            for (int current = 0, previous = polygon.Count - 1;
                 current < polygon.Count;
                 previous = current++)
            {
                Vector2 a = polygon[previous];
                Vector2 b = polygon[current];
                if (PointOnSegment(point, a, b))
                    return true;

                bool crossesRay = (a.y > point.y) != (b.y > point.y);
                if (crossesRay && point.x <
                    (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x)
                    inside = !inside;
            }

            return inside;
        }

        private static bool HasSelfIntersections(IReadOnlyList<Vector2> polygon)
        {
            if (polygon.Count < 4)
                return false;

            for (int first = 0; first < polygon.Count; first++)
            {
                int firstNext = (first + 1) % polygon.Count;
                for (int second = first + 1; second < polygon.Count; second++)
                {
                    int secondNext = (second + 1) % polygon.Count;
                    if (first == second || firstNext == second || secondNext == first)
                        continue;
                    if (SegmentsIntersect(polygon[first], polygon[firstNext],
                            polygon[second], polygon[secondNext]))
                        return true;
                }
            }

            return false;
        }

        private static bool SegmentsIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            float abC = Cross(b - a, c - a);
            float abD = Cross(b - a, d - a);
            float cdA = Cross(d - c, a - c);
            float cdB = Cross(d - c, b - c);

            if (((abC > GeometryEpsilon && abD < -GeometryEpsilon) ||
                 (abC < -GeometryEpsilon && abD > GeometryEpsilon)) &&
                ((cdA > GeometryEpsilon && cdB < -GeometryEpsilon) ||
                 (cdA < -GeometryEpsilon && cdB > GeometryEpsilon)))
                return true;

            return Mathf.Abs(abC) <= GeometryEpsilon && PointOnSegment(c, a, b) ||
                   Mathf.Abs(abD) <= GeometryEpsilon && PointOnSegment(d, a, b) ||
                   Mathf.Abs(cdA) <= GeometryEpsilon && PointOnSegment(a, c, d) ||
                   Mathf.Abs(cdB) <= GeometryEpsilon && PointOnSegment(b, c, d);
        }

        private static bool PointOnSegment(Vector2 point, Vector2 start, Vector2 end)
        {
            if (Mathf.Abs(Cross(end - start, point - start)) > GeometryEpsilon)
                return false;

            return point.x >= Mathf.Min(start.x, end.x) - GeometryEpsilon &&
                   point.x <= Mathf.Max(start.x, end.x) + GeometryEpsilon &&
                   point.y >= Mathf.Min(start.y, end.y) - GeometryEpsilon &&
                   point.y <= Mathf.Max(start.y, end.y) + GeometryEpsilon;
        }

        private static float SignedArea(IReadOnlyList<Vector2> polygon)
        {
            float twiceArea = 0f;
            for (int i = 0; i < polygon.Count; i++)
            {
                Vector2 current = polygon[i];
                Vector2 next = polygon[(i + 1) % polygon.Count];
                twiceArea += current.x * next.y - next.x * current.y;
            }
            return twiceArea * 0.5f;
        }

        private static float Cross(Vector2 first, Vector2 second) =>
            first.x * second.y - first.y * second.x;

        private static bool AreVerticesFinite(IReadOnlyList<Vector2> polygon)
        {
            for (int i = 0; i < polygon.Count; i++)
            {
                if (!IsFinite(polygon[i]))
                    return false;
            }
            return true;
        }

        private static bool IsFinite(Vector2 value) => IsFinite(value.x) && IsFinite(value.y);
        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}

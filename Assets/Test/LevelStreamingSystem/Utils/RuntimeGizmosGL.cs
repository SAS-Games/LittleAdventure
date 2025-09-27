using System.Collections.Generic;
using UnityEngine;
#if UNITY_AI
using UnityEngine.AI;
#endif

[DefaultExecutionOrder(10000)]
public class RuntimeGizmosGL : MonoBehaviour
{
    // ------------ Internal Data ------------
    struct Line { public Vector3 a, b; public Color color; public float time; }
    struct Quad { public Vector3 a, b, c, d; public Color color; public float time; }
    struct Tri { public Vector3 a, b, c; public Color color; public float time; }

    private static readonly List<Line> lines = new List<Line>();
    private static readonly List<Quad> quads = new List<Quad>();
    private static readonly List<Tri> tris = new List<Tri>();

    private static Material lineMat;
    
    private static RuntimeGizmosGL instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInit()
    {
        if (instance == null)
        {
            var go = new GameObject("RuntimeGizmosGL (auto)");
            go.hideFlags = HideFlags.HideAndDontSave;
            instance = go.AddComponent<RuntimeGizmosGL>();
            DontDestroyOnLoad(go);
        }
    }

    void Awake()
    {
        if (!lineMat)
        {
            // Hidden Unity shader, supports vertex colors + transparency
            var shader = Shader.Find("Hidden/Internal-Colored");
            lineMat = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            lineMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMat.SetInt("_ZWrite", 0);
        }
    }

    void LateUpdate()
    {
        // Decrement line lifetimes
        for (int i = lines.Count - 1; i >= 0; i--)
        {
            var l = lines[i]; l.time -= Time.deltaTime;
            if (l.time <= 0) lines.RemoveAt(i); else lines[i] = l;
        }

        for (int i = quads.Count - 1; i >= 0; i--)
        {
            var q = quads[i]; q.time -= Time.deltaTime;
            if (q.time <= 0) quads.RemoveAt(i); else quads[i] = q;
        }

        for (int i = tris.Count - 1; i >= 0; i--)
        {
            var t = tris[i]; t.time -= Time.deltaTime;
            if (t.time <= 0) tris.RemoveAt(i); else tris[i] = t;
        }
    }

    void OnPostRender()
    {
        if (lines.Count == 0 && quads.Count == 0 && tris.Count == 0) return;

        lineMat.SetPass(0);
        GL.PushMatrix();
        GL.MultMatrix(Matrix4x4.identity);

        // Lines
        if (lines.Count > 0)
        {
            GL.Begin(GL.LINES);
            foreach (var l in lines)
            {
                GL.Color(l.color);
                GL.Vertex(l.a);
                GL.Vertex(l.b);
            }
            GL.End();
        }

        // Quads
        if (quads.Count > 0)
        {
            GL.Begin(GL.QUADS);
            foreach (var q in quads)
            {
                GL.Color(q.color);
                GL.Vertex(q.a);
                GL.Vertex(q.b);
                GL.Vertex(q.c);
                GL.Vertex(q.d);
            }
            GL.End();
        }

        // Triangles
        if (tris.Count > 0)
        {
            GL.Begin(GL.TRIANGLES);
            foreach (var t in tris)
            {
                GL.Color(t.color);
                GL.Vertex(t.a);
                GL.Vertex(t.b);
                GL.Vertex(t.c);
            }
            GL.End();
        }

        GL.PopMatrix();
    }

    // ------------ Public API (Lines) ------------
    public static void DrawLine(Vector3 a, Vector3 b, Color color, float duration = 0f)
    {
        EnsureInstance();
        lines.Add(new Line
        {
            a = a, b = b,
            color = color,
            time = duration <= 0 ? Time.deltaTime : duration
        });
    }

    public static void DrawRay(Vector3 origin, Vector3 dir, Color color, float duration = 0f)
        => DrawLine(origin, origin + dir, color, duration);

    // ------------ Public API (Wire Shapes) ------------
    public static void DrawWireCube(Vector3 center, Vector3 size, Color color, float duration = 0f)
    {
        Vector3 ext = size * 0.5f;
        Vector3[] p =
        {
            center + new Vector3(-ext.x,-ext.y,-ext.z),
            center + new Vector3( ext.x,-ext.y,-ext.z),
            center + new Vector3( ext.x,-ext.y, ext.z),
            center + new Vector3(-ext.x,-ext.y, ext.z),
            center + new Vector3(-ext.x, ext.y,-ext.z),
            center + new Vector3( ext.x, ext.y,-ext.z),
            center + new Vector3( ext.x, ext.y, ext.z),
            center + new Vector3(-ext.x, ext.y, ext.z),
        };
        // bottom
        DrawLine(p[0], p[1], color, duration);
        DrawLine(p[1], p[2], color, duration);
        DrawLine(p[2], p[3], color, duration);
        DrawLine(p[3], p[0], color, duration);
        // top
        DrawLine(p[4], p[5], color, duration);
        DrawLine(p[5], p[6], color, duration);
        DrawLine(p[6], p[7], color, duration);
        DrawLine(p[7], p[4], color, duration);
        // sides
        DrawLine(p[0], p[4], color, duration);
        DrawLine(p[1], p[5], color, duration);
        DrawLine(p[2], p[6], color, duration);
        DrawLine(p[3], p[7], color, duration);
    }

    public static void DrawWireSphere(Vector3 center, float radius, Color color, int segments = 32, float duration = 0f)
    {
        segments = Mathf.Max(8, segments);
        void Ring(Vector3 axisA, Vector3 axisB)
        {
            Vector3 prev = center + axisA * radius;
            for (int i = 1; i <= segments; i++)
            {
                float t = (i / (float)segments) * Mathf.PI * 2f;
                Vector3 p = center + (Mathf.Cos(t) * axisA + Mathf.Sin(t) * axisB) * radius;
                DrawLine(prev, p, color, duration);
                prev = p;
            }
        }
        Ring(Vector3.right, Vector3.up);
        Ring(Vector3.up, Vector3.forward);
        Ring(Vector3.forward, Vector3.right);
    }

    public static void DrawFrustum(Vector3 pos, Quaternion rot, float fov, float aspect, float near, float far, Color color, float duration = 0f)
    {
        Matrix4x4 m = Matrix4x4.TRS(pos, rot, Vector3.one);
        float halfFov = fov * 0.5f * Mathf.Deg2Rad;
        float hNear = 2f * Mathf.Tan(halfFov) * near;
        float wNear = hNear * aspect;
        float hFar = 2f * Mathf.Tan(halfFov) * far;
        float wFar = hFar * aspect;

        Vector3[] nc = {
            new Vector3(-wNear/2,-hNear/2,near), new Vector3(wNear/2,-hNear/2,near),
            new Vector3(wNear/2,hNear/2,near), new Vector3(-wNear/2,hNear/2,near)};
        Vector3[] fc = {
            new Vector3(-wFar/2,-hFar/2,far), new Vector3(wFar/2,-hFar/2,far),
            new Vector3(wFar/2,hFar/2,far), new Vector3(-wFar/2,hFar/2,far)};

        for (int i=0;i<4;i++) { nc[i] = m.MultiplyPoint(nc[i]); fc[i] = m.MultiplyPoint(fc[i]); }

        // near
        DrawLine(nc[0], nc[1], color, duration);
        DrawLine(nc[1], nc[2], color, duration);
        DrawLine(nc[2], nc[3], color, duration);
        DrawLine(nc[3], nc[0], color, duration);
        // far
        DrawLine(fc[0], fc[1], color, duration);
        DrawLine(fc[1], fc[2], color, duration);
        DrawLine(fc[2], fc[3], color, duration);
        DrawLine(fc[3], fc[0], color, duration);
        // connect
        for (int i=0;i<4;i++) DrawLine(nc[i], fc[i], color, duration);
    }

    public static void DrawWireCapsule(Vector3 p1, Vector3 p2, float radius, Color color, int segments = 24, float duration = 0f)
    {
        Vector3 up = (p2 - p1).normalized;
        float height = Vector3.Distance(p1, p2);
        if (height < radius * 2f)
        {
            DrawWireSphere((p1 + p2) * 0.5f, radius, color, segments, duration);
            return;
        }
        Vector3 forward = Vector3.Slerp(up, -up, 0.5f).normalized;
        Vector3 right = Vector3.Cross(up, forward).normalized;

        // cylinder sides
        for (int i = 0; i < segments; i++)
        {
            float t1 = i * Mathf.PI * 2f / segments;
            float t2 = (i+1) * Mathf.PI * 2f / segments;

            Vector3 r1 = Mathf.Cos(t1) * right * radius + Mathf.Sin(t1) * forward * radius;
            Vector3 r2 = Mathf.Cos(t2) * right * radius + Mathf.Sin(t2) * forward * radius;

            DrawLine(p1 + r1, p2 + r1, color, duration);
            DrawLine(p1 + r1, p1 + r2, color, duration);
            DrawLine(p2 + r1, p2 + r2, color, duration);
        }

        // hemispheres
        void Hemisphere(Vector3 center, Vector3 dir)
        {
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, dir);
            for (int i = 0; i < segments/2; i++)
            {
                float t1 = i * Mathf.PI / (segments/2);
                float t2 = (i+1) * Mathf.PI / (segments/2);
                for (int j = 0; j < segments; j++)
                {
                    float a1 = j * Mathf.PI * 2f / segments;
                    float a2 = (j+1) * Mathf.PI * 2f / segments;

                    Vector3 v1 = new Vector3(Mathf.Cos(a1) * Mathf.Sin(t1), Mathf.Cos(t1), Mathf.Sin(a1) * Mathf.Sin(t1));
                    Vector3 v2 = new Vector3(Mathf.Cos(a1) * Mathf.Sin(t2), Mathf.Cos(t2), Mathf.Sin(a1) * Mathf.Sin(t2));
                    Vector3 v3 = new Vector3(Mathf.Cos(a2) * Mathf.Sin(t1), Mathf.Cos(t1), Mathf.Sin(a2) * Mathf.Sin(t1));

                    v1 = rot * v1 * radius + center;
                    v2 = rot * v2 * radius + center;
                    v3 = rot * v3 * radius + center;

                    DrawLine(v1, v2, color, duration);
                    DrawLine(v1, v3, color, duration);
                }
            }
        }
        Hemisphere(p1, -up);
        Hemisphere(p2, up);
    }

    // ------------ Public API (Solid Shapes) ------------
    public static void DrawSolidBox(Vector3 center, Vector3 size, Color color, float duration = 0f)
    {
        Vector3 ext = size * 0.5f;
        Vector3[] p =
        {
            center + new Vector3(-ext.x,-ext.y,-ext.z),
            center + new Vector3( ext.x,-ext.y,-ext.z),
            center + new Vector3( ext.x, ext.y,-ext.z),
            center + new Vector3(-ext.x, ext.y,-ext.z),
            center + new Vector3(-ext.x,-ext.y, ext.z),
            center + new Vector3( ext.x,-ext.y, ext.z),
            center + new Vector3( ext.x, ext.y, ext.z),
            center + new Vector3(-ext.x, ext.y, ext.z),
        };
        AddQuad(p[0], p[1], p[2], p[3], color, duration); // back
        AddQuad(p[5], p[4], p[7], p[6], color, duration); // front
        AddQuad(p[4], p[0], p[3], p[7], color, duration); // left
        AddQuad(p[1], p[5], p[6], p[2], color, duration); // right
        AddQuad(p[3], p[2], p[6], p[7], color, duration); // top
        AddQuad(p[4], p[5], p[1], p[0], color, duration); // bottom
    }

    public static void DrawSolidPolygon(IList<Vector3> points, Color color, float duration = 0f)
    {
        EnsureInstance();
        if (points == null || points.Count < 3) return;
        Vector3 origin = points[0];
        for (int i = 1; i < points.Count - 1; i++)
        {
            tris.Add(new Tri
            {
                a = origin, b = points[i], c = points[i + 1],
                color = color,
                time = duration <= 0 ? Time.deltaTime : duration
            });
        }
    }

#if UNITY_AI
    public static void DrawNavMesh(Color color, float duration = 0f)
    {
        NavMeshTriangulation tri = NavMesh.CalculateTriangulation();
        for (int i = 0; i < tri.indices.Length; i += 3)
        {
            DrawSolidPolygon(new Vector3[]
            {
                tri.vertices[tri.indices[i]],
                tri.vertices[tri.indices[i + 1]],
                tri.vertices[tri.indices[i + 2]]
            }, color, duration);
        }
    }
#endif

    // ------------ Helpers ------------
    private static void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color color, float duration)
    {
        quads.Add(new Quad
        {
            a = a, b = b, c = c, d = d,
            color = color,
            time = duration <= 0 ? Time.deltaTime : duration
        });
    }

    private static void EnsureInstance()
    {
        if (instance != null) return;
        AutoInit();
    }
}

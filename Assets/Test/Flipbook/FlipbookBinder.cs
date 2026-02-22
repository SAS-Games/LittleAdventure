using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class FlipbookBinder : MonoBehaviour
{
    public FlipbookData data;
    public Material targetMaterial;

    const int MAX_FRAMES = 64;

    void OnEnable()
    {
        Apply();
    }

    public void Apply()
    {
        if (data == null || targetMaterial == null)
            return;

        int count = Mathf.Min(data.frames.Length, MAX_FRAMES);

        Vector4[] uvRects = new Vector4[MAX_FRAMES];
        Vector4[] frameInfo = new Vector4[MAX_FRAMES];
        Vector2 referenceSize = data.frames[0].textureRect.size;

        for (int i = 0; i < count; i++)
        {
            Sprite s = data.frames[i];

            Rect r = s.textureRect;
            Texture tex = s.texture;

            // UV rect
            uvRects[i] = new Vector4(
                r.xMin / tex.width,
                r.yMin / tex.height,
                r.xMax / tex.width,
                r.yMax / tex.height
            );

            // size + pivot normalization
            Vector2 size = r.size;
            Vector2 pivot = s.pivot;

            Vector2 normalizedPivot =
                (pivot / size) - new Vector2(0.5f, 0.5f);

            frameInfo[i] = new Vector4(
                size.x / referenceSize.x,
                size.y / referenceSize.y,
                normalizedPivot.x,
                normalizedPivot.y
            );
        }

        targetMaterial.SetInt("_FrameCount", count);
        targetMaterial.SetVectorArray("_FrameUVs", uvRects);
        targetMaterial.SetVectorArray("_FrameInfo", frameInfo);
        targetMaterial.SetFloat("_Speed", data.defaultSpeed);
    }
}
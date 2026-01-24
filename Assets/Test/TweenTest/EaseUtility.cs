using Unity.Mathematics;

public static class EaseUtility
{
    public static float Evaluate(EaseType easeType, float t)
    {
        t = math.saturate(t);

        switch (easeType)
        {
            case EaseType.EaseInQuad:
                return t * t;

            case EaseType.EaseOutQuad:
                return 1f - (1f - t) * (1f - t);

            case EaseType.EaseInOutQuad:
                return t < 0.5f
                    ? 2f * t * t
                    : 1f - math.pow(-2f * t + 2f, 2f) * 0.5f;

            case EaseType.EaseOutBack:
                const float c1 = 1.70158f;
                const float c3 = c1 + 1f;
                return 1f + c3 * math.pow(t - 1f, 3f) + c1 * math.pow(t - 1f, 2f);

            default:
                return t;
        }
    }
}


public enum EaseType
{
    EaseInQuad,
    EaseOutQuad,
    EaseInOutQuad,
    EaseOutBack
}
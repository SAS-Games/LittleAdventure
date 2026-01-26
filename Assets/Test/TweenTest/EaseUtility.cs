using System.Runtime.CompilerServices;
using Unity.Mathematics;

public static class EaseUtility
{
    public static float Evaluate(EaseType type, float t)
    {
        t = math.saturate(t);

        switch (type)
        {
            case EaseType.Linear: return t;

            case EaseType.EaseInQuad: return InQuad(t);
            case EaseType.EaseOutQuad: return OutQuad(t);
            case EaseType.EaseInOutQuad: return InOutQuad(t);

            case EaseType.EaseInCubic: return InCubic(t);
            case EaseType.EaseOutCubic: return OutCubic(t);
            case EaseType.EaseInOutCubic: return InOutCubic(t);

            case EaseType.EaseInQuart: return InQuart(t);
            case EaseType.EaseOutQuart: return OutQuart(t);
            case EaseType.EaseInOutQuart: return InOutQuart(t);

            case EaseType.EaseInQuint: return InQuint(t);
            case EaseType.EaseOutQuint: return OutQuint(t);
            case EaseType.EaseInOutQuint: return InOutQuint(t);

            case EaseType.EaseInSine: return InSine(t);
            case EaseType.EaseOutSine: return OutSine(t);
            case EaseType.EaseInOutSine: return InOutSine(t);

            case EaseType.EaseInExpo: return InExpo(t);
            case EaseType.EaseOutExpo: return OutExpo(t);
            case EaseType.EaseInOutExpo: return InOutExpo(t);

            case EaseType.EaseInCirc: return InCirc(t);
            case EaseType.EaseOutCirc: return OutCirc(t);
            case EaseType.EaseInOutCirc: return InOutCirc(t);

            case EaseType.Spring: return Spring(t);

            case EaseType.EaseInBounce: return InBounce(t);
            case EaseType.EaseOutBounce: return OutBounce(t);
            case EaseType.EaseInOutBounce: return InOutBounce(t);

            case EaseType.EaseInBack: return InBack(t);
            case EaseType.EaseOutBack: return OutBack(t);
            case EaseType.EaseInOutBack: return InOutBack(t);

            case EaseType.EaseInElastic: return InElastic(t);
            case EaseType.EaseOutElastic: return OutElastic(t);
            case EaseType.EaseInOutElastic: return InOutElastic(t);

            default:
                return t;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float InQuad(float t) => t * t;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float OutQuad(float t) => 1f - (1f - t) * (1f - t);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float InOutQuad(float t)
    {
        return t < 0.5f
            ? 2f * t * t
            : 1f - math.pow(-2f * t + 2f, 2f) * 0.5f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float InCubic(float t) => t * t * t;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float OutCubic(float t)
    {
        t -= 1f;
        return t * t * t + 1f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float InOutCubic(float t)
        => t < 0.5f
            ? 4f * t * t * t
            : 1f - math.pow(-2f * t + 2f, 3f) * 0.5f;

   
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float InQuart(float t) => t * t * t * t;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float OutQuart(float t)
    {
        t -= 1f;
        return 1f - t * t * t * t;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float InOutQuart(float t)
    {
        return t < 0.5f
            ? 8f * t * t * t * t
            : 1f - math.pow(-2f * t + 2f, 4f) * 0.5f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float InQuint(float t) => t * t * t * t * t;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float OutQuint(float t)
    {
        t -= 1f;
        return t * t * t * t * t + 1f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float InOutQuint(float t)
    {
        return t < 0.5f
            ? 16f * t * t * t * t * t
            : 1f - math.pow(-2f * t + 2f, 5f) * 0.5f;
    }
    

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float InSine(float t) => 1f - math.cos(t * math.PI * 0.5f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float OutSine(float t) => math.sin(t * math.PI * 0.5f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float InOutSine(float t) => -(math.cos(math.PI * t) - 1f) * 0.5f;

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float InExpo(float t) => t == 0f ? 0f : math.pow(2f, 10f * t - 10f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float OutExpo(float t) => t == 1f ? 1f : 1f - math.pow(2f, -10f * t);

    static float InOutExpo(float t)
    {
        if (t == 0f) return 0f;
        if (t == 1f) return 1f;

        return t < 0.5f
            ? math.pow(2f, 20f * t - 10f) * 0.5f
            : (2f - math.pow(2f, -20f * t + 10f)) * 0.5f;
    }

  
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float InCirc(float t) => 1f - math.sqrt(1f - t * t);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float OutCirc(float t)
    {
        t -= 1f;
        return math.sqrt(1f - t * t);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float InOutCirc(float t)
    {
        return t < 0.5f
            ? (1f - math.sqrt(1f - 4f * t * t)) * 0.5f
            : (math.sqrt(1f - math.pow(-2f * t + 2f, 2f)) + 1f) * 0.5f;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float OutBounce(float t)
    {
        if (t < 1f / 2.75f) return 7.5625f * t * t;

        if (t < 2f / 2.75f)
        {
            t -= 1.5f / 2.75f;
            return 7.5625f * t * t + 0.75f;
        }

        if (t < 2.5f / 2.75f)
        {
            t -= 2.25f / 2.75f;
            return 7.5625f * t * t + 0.9375f;
        }

        t -= 2.625f / 2.75f;
        return 7.5625f * t * t + 0.984375f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float InBounce(float t) => 1f - OutBounce(1f - t);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float InOutBounce(float t)
    {
        return t < 0.5f
            ? (1f - OutBounce(1f - 2f * t)) * 0.5f
            : (1f + OutBounce(2f * t - 1f)) * 0.5f;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float InBack(float t)
    {
        const float s = 1.70158f;
        return t * t * ((s + 1f) * t - s);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float OutBack(float t)
    {
        const float s = 1.70158f;
        t -= 1f;
        return t * t * ((s + 1f) * t + s) + 1f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float InOutBack(float t)
    {
        const float s = 1.70158f * 1.525f;

        return t < 0.5f
            ? (math.pow(2f * t, 2f) * ((s + 1f) * 2f * t - s)) * 0.5f
            : (math.pow(2f * t - 2f, 2f) * ((s + 1f) * (t * 2f - 2f) + s) + 2f) * 0.5f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float InElastic(float t)
    {
        if (t == 0f || t == 1f) return t;
        return -math.pow(2f, 10f * t - 10f)
               * math.sin((t * 10f - 10.75f) * (2f * math.PI) / 3f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float OutElastic(float t)
    {
        if (t == 0f || t == 1f) return t;
        return math.pow(2f, -10f * t)
            * math.sin((t * 10f - 0.75f) * (2f * math.PI) / 3f) + 1f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float InOutElastic(float t)
    {
        if (t == 0f || t == 1f) return t;

        return t < 0.5f
            ? -0.5f * math.pow(2f, 20f * t - 10f)
                    * math.sin((20f * t - 11.125f) * (2f * math.PI) / 4.5f)
            : 1f + 0.5f * math.pow(2f, -20f * t + 10f)
                        * math.sin((20f * t - 11.125f) * (2f * math.PI) / 4.5f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float Spring(float t)
    {
        return (math.sin(t * math.PI * (0.2f + 2.5f * t * t * t))
                   * math.pow(1f - t, 2.2f) + t)
               * (1f + 1.2f * (1f - t));
    }
}

public enum EaseType
{
    Linear,
    EaseInQuad,
    EaseOutQuad,
    EaseInOutQuad,
    EaseInCubic,
    EaseOutCubic,
    EaseInOutCubic,
    EaseInQuart,
    EaseOutQuart,
    EaseInOutQuart,
    EaseInQuint,
    EaseOutQuint,
    EaseInOutQuint,
    EaseInSine,
    EaseOutSine,
    EaseInOutSine,
    EaseInExpo,
    EaseOutExpo,
    EaseInOutExpo,
    EaseInCirc,
    EaseOutCirc,
    EaseInOutCirc,
    EaseInBack,
    EaseOutBack,
    EaseInOutBack,
    EaseInBounce,
    EaseOutBounce,
    EaseInOutBounce,
    EaseInElastic,
    EaseOutElastic,
    EaseInOutElastic,
    Spring
}
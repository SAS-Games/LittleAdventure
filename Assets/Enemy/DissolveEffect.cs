using UnityEngine;

public class DissolveEffect : DissolveBaseEffect
{
    [SerializeField] private float dissolveStartHeight = 20f;
    [SerializeField] private float dissolveTargetHeight = -10f;

    protected override void SetEffectProperties(out float startHeight, out float targetHeight)
    {
        startHeight = dissolveStartHeight;
        targetHeight = dissolveTargetHeight;
    }
}

using UnityEngine;

public class AppearEffect : DissolveBaseEffect
{
    [SerializeField] private float appearStartHeight = -10f;
    [SerializeField] private float appearTargetHeight = 20f;

    protected override void SetEffectProperties(out float startHeight, out float targetHeight)
    {
        startHeight = appearStartHeight;
        targetHeight = appearTargetHeight;
    }
}

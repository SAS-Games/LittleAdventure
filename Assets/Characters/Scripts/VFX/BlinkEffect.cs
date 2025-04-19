using SAS.Utilities.TagSystem;
using UnityEngine;

public class BlinkEffect : MonoBehaviour
{
    [FieldRequiresSelf] private IEventDispatcher _eventDispatcher;
    [SerializeField] private string m_EffectEventName = "BeingHit";
    [SerializeField] private Renderer m_MeshRenderer;
    [SerializeField] private string m_BlinkPropertyName = "_blink";
    [SerializeField] private float m_BlinkValue = 0.4f;
    [SerializeField] private float m_BlinkDuration = 2f;
    [SerializeField] private float m_BlinkInterval = 0.2f;

    private MaterialPropertyBlock _materialPropertyBlock;

    void Start()
    {
        this.Initialize();
        _materialPropertyBlock = new MaterialPropertyBlock();
        m_MeshRenderer.GetPropertyBlock(_materialPropertyBlock);
        _eventDispatcher.Subscribe(m_EffectEventName, PlayBlinkEffect);
    }

    async void PlayBlinkEffect()
    {
        float elapsedTime = 0f;

        while (elapsedTime < m_BlinkDuration)
        {
            SetBlinkIntensity(m_BlinkValue);
            await Awaitable.WaitForSecondsAsync(m_BlinkInterval);
            elapsedTime += m_BlinkInterval;

            SetBlinkIntensity(0.0f);
            await Awaitable.WaitForSecondsAsync(m_BlinkInterval);
            elapsedTime += m_BlinkInterval;
        }
    }

    private void SetBlinkIntensity(float value)
    {
        _materialPropertyBlock.SetFloat(m_BlinkPropertyName, value);
        m_MeshRenderer.SetPropertyBlock(_materialPropertyBlock);
    }
}

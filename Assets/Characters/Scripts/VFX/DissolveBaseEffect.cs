using SAS.StateMachineGraph;
using SAS.Utilities.TagSystem;
using UnityEngine;

public abstract class DissolveBaseEffect : MonoBehaviour, IAwaitableStateAction
{
    [SerializeField] private Renderer m_MeshRenderer;
    [SerializeField] private string m_DissolvePropertyName = "_dissolve_height";
    [SerializeField] private float m_InitialDelay = 2;
    [SerializeField] private float m_Duration = 1f;
    [SerializeField] private float m_StartHeight;
    [SerializeField] private float m_TargetHeight;

    private MaterialPropertyBlock _materialPropertyBlock;

    public bool IsCompleted { get; private set; }

    void IStateAction.OnInitialize(Actor actor, Tag tag, string key)
    {
        actor.Initialize(this);
        _materialPropertyBlock = new MaterialPropertyBlock();
        m_MeshRenderer.GetPropertyBlock(_materialPropertyBlock);
    }

    void IStateAction.Execute(ActionExecuteEvent executeEvent)
    {
        PlayEffect();
    }

    protected abstract void SetEffectProperties(out float startHeight, out float targetHeight);

    private async void PlayEffect()
    {
        SetEffectProperties(out m_StartHeight, out m_TargetHeight);
        IsCompleted = false;
        await Awaitable.WaitForSecondsAsync(m_InitialDelay);
        float elapsedTime = 0f;

        while (elapsedTime < m_Duration)
        {
            float currentHeight = Mathf.Lerp(m_StartHeight, m_TargetHeight, elapsedTime / m_Duration);
            SetPropertyBlock(currentHeight);
            elapsedTime += Time.deltaTime;
            await Awaitable.EndOfFrameAsync();
        }

        IsCompleted = true;
    }

    private void SetPropertyBlock(float value)
    {
        _materialPropertyBlock.SetFloat(m_DissolvePropertyName, value);
        m_MeshRenderer.SetPropertyBlock(_materialPropertyBlock);
    }
}

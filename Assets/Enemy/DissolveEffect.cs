using SAS.StateMachineGraph;
using SAS.Utilities.TagSystem;
using UnityEngine;

public class DissolveEffect : MonoBehaviour, IAwaitableStateAction
{
    [SerializeField] private Renderer m_MeshRenderer;
    [SerializeField] private string m_DissolvePropertyName = "_dissolve_height";
    [SerializeField] private float m_InitialDelay = 2;
    [SerializeField] private float m_DessolveDuration = 1f;
    [SerializeField] private float m_StartHeight = 20f;
    [SerializeField] private float m_TargetHeight = -10f;

    private MaterialPropertyBlock _materialPropertyBlock;

    public bool IsCompleted { get; private set; }

    void IStateAction.OnInitialize(Actor actor, Tag tag, string key)
    {
        this.Initialize();
        _materialPropertyBlock = new MaterialPropertyBlock();
        m_MeshRenderer.GetPropertyBlock(_materialPropertyBlock);
    }

    void IStateAction.Execute(ActionExecuteEvent executeEvent)
    {
        PlayEffect();
    }

    async void PlayEffect()
    {
        IsCompleted = false;
        await Awaitable.WaitForSecondsAsync(m_InitialDelay);
        float elapsedTime = 0f;
        float currentHight;

        while (elapsedTime < m_DessolveDuration)
        {
            currentHight = Mathf.Lerp(m_StartHeight, m_TargetHeight, elapsedTime / m_DessolveDuration);
            SetPropertyBlock(currentHight);
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

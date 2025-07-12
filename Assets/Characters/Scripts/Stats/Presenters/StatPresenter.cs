using SAS.Utilities.TagSystem;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

public abstract class StatPresenter<TModel> : MonoBehaviour where TModel : IStatModel
{
    [SerializeField] private int m_MaxValue;
    [SerializeField] protected UnityEvent m_OnEmpty;
    [Inject] protected TModel _model;
    protected abstract IProxyView<float> View { get; }

    protected virtual void Awake()
    {
        this.Initialize();
        _model.Setup(m_MaxValue);
        _model.Current.Subscribe(OnValueChanged).AddTo(this);
    }

    protected virtual void OnValueChanged(float current)
    {
        View?.OnValueChanged(current);
        if (current <= 0)
            m_OnEmpty?.Invoke();
    }
}
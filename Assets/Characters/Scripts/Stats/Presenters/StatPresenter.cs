using SAS.Utilities.TagSystem;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

public abstract class StatPresenter<TModel> : MonoBehaviour where TModel : IStatModel
{
    [SerializeField] private int m_MaxValue;
    [SerializeField] protected UnityEvent m_OnEmpty;
    [Inject] protected TModel _model;
    protected abstract IProxyView View { get; }
    private IProxyView<float> _proxyView;
    private IRangeProxyView<float> _rangeProxyView;

    protected virtual void Awake()
    {
        this.Initialize();

        /*Todo:
         Need a better way. may be one can create the base class StatPresenterBase<TModel, TView>
         where TView can be either IProxyView<float> or IRangeProxyView<float>
         then there will be two separate classes like StatPresenter<TModel, IProxyView<float>> and
         RangeStatePresenter<TModel, IRangeProxyView<float>>
         */
        if (View is IRangeProxyView<float> rangeView)
            _rangeProxyView = rangeView;
        else if (View is IProxyView<float> proxyView)
            _proxyView = proxyView;

        _model.Setup(m_MaxValue);
        _model.Current.Subscribe(_ => OnValueChanged()).AddTo(this);
        _model.Max.Subscribe(_ => OnValueChanged()).AddTo(this);
    }

    protected virtual void OnValueChanged()
    {
        if (_rangeProxyView != null)
            _rangeProxyView.OnValueChanged(_model.Current.Value, _model.Max.Value);
        else
            _proxyView?.OnValueChanged(_model.Current.Value);

        if (_model.Current.Value <= 0)
            m_OnEmpty?.Invoke();
    }

    public void Increase(float value)
    {
        _model.Increase(value);
    }

    public void IncreaseMax(float value)
    {
        _model.UpdateMax(_model.Max.Value + value);
    }
}
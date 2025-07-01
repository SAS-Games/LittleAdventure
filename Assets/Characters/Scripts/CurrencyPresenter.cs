using SAS.Utilities.TagSystem;
using UniRx;
using UnityEngine;

public interface ICurrencyPresenter
{
    ICurrencyModel CurrencyModel { get; }
    void Collect();
}

public class CurrencyPresenter : MonoBehaviour, ICurrencyPresenter
{
    [SerializeField] private Tag m_ProxyViewTag;
    [SerializeField] private string _currencyType = "Coins";
    private IProxyView<float> _currencyView;
    ICurrencyModel ICurrencyPresenter.CurrencyModel => _currencyModel;

    private ICurrencyModel _currencyModel;

    void Awake()
    {
        _currencyView = this.GetComponent<IProxyView<float>>(m_ProxyViewTag);
        _currencyModel = new CurrencyModel();
        _currencyModel.Value.Add(_currencyType, 0);
        _currencyModel.GetValue(_currencyType).Subscribe(current => { _currencyView?.OnValueChanged(current); }).AddTo(this);
    }

    public void Collect()
    {
        _currencyModel.Value[_currencyType]++;
    }
}
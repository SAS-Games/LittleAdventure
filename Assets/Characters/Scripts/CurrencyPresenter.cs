using SAS.Utilities.TagSystem;
using UniRx;
using UnityEngine;

public interface ICurrencyPresenter
{
    void Collect();
    int GetValue();
    void SetValue(int value);
}

public class CurrencyPresenter : MonoBehaviour, ICurrencyPresenter
{
    [Inject] private ICurrencyModel _currencyModel;
    [SerializeField] private Tag m_ProxyViewTag;
    [SerializeField] private string _currencyType = "Coins";
    private IProxyView<float> _currencyView;
    
    void Awake()
    {
        this.Initialize();
        _currencyView = this.GetComponent<IProxyView<float>>(m_ProxyViewTag);
        _currencyModel.Value.Add(_currencyType, 0);
        _currencyModel.GetValue(_currencyType).Subscribe(current => { _currencyView?.OnValueChanged(current); }).AddTo(this);
    }

    public void Collect()
    {
        _currencyModel.Value[_currencyType]++;
    }

    int ICurrencyPresenter.GetValue()
    {
        return _currencyModel.Value[_currencyType];
    }

    void ICurrencyPresenter.SetValue(int value)
    {
        _currencyModel.Value[_currencyType] = value;
    }
}
using SAS.Core.TagSystem;
using UniRx;
using UnityEngine;

public class ActivatableProxyView : MonoBehaviour, IProxyView<bool>, ServiceLocator.IService
{
    private ReactiveProperty<bool> _value = new(false);
    IReadOnlyReactiveProperty<bool> IProxyView<bool>.Value => _value;
    int IProxyView.ProxyControlID { get; set; }

    void IProxyView<bool>.OnValueChanged(bool value)
    {
        _value.Value = value;
        gameObject.SetActive(value);
    }

    private void OnEnable()
    {
        (this as IProxyView<bool>).OnValueChanged(true);
    }

    private void OnDisable()
    {
        (this as IProxyView<bool>).OnValueChanged(false);
    }
}
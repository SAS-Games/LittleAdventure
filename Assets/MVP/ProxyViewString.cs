using SAS.Utilities.TagSystem;
using UniRx;
using UnityEngine;
using Debug = SAS.Debug;

public class ProxyViewString : MonoBehaviour, IProxyView<string>, ServiceLocator.IService
{
    private ReactiveProperty<string> _value = new(string.Empty);
    public IReadOnlyReactiveProperty<string> Value => _value;

    int IProxyView.ProxyControlID { get; set; } = -1;

    void IProxyView<string>.OnValueChanged(string value)
    {
        _value.Value = value;
        Debug.Log($"Value: {value}", this.GetTag().ToString());
    }
}
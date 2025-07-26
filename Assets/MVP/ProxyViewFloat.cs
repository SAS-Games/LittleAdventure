using SAS.Utilities.TagSystem;
using UniRx;
using UnityEngine;
using Debug = SAS.Debug;

public class ProxyViewFloat : MonoBehaviour, IProxyView<float>, ServiceLocator.IService
{
    private ReactiveProperty<float> _value = new(0);
    public IReadOnlyReactiveProperty<float> Value => _value;

    int IProxyView.ProxyControlID { get; set; } = -1;

    void IProxyView<float>.OnValueChanged(float value)
    {
        _value.Value = value;
        Debug.Log($"Value: {value}", this.GetTag().ToString());
    }
}
using UniRx;
using UnityEngine;
using Debug = SAS.Debug;

public class ProxyViewFloat : MonoBehaviour, IProxyView<float>
{
    private ReactiveProperty<float> _value = new(0);
    public IReadOnlyReactiveProperty<float> Value => _value;

    void IProxyView<float>.OnValueChanged(float value)
    {
        _value.Value = value;
        Debug.Log($"Value: {value}");
    }
}
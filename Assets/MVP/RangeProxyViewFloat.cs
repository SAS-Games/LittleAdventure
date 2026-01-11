using SAS.Core.TagSystem;
using UniRx;
using UnityEngine;
using Debug = SAS.Debug;

public class RangeProxyViewFloat : MonoBehaviour, IRangeProxyView<float>, ServiceLocator.IService
{
    private readonly ReactiveProperty<float> _value = new(0);
    private readonly ReactiveProperty<float> _maxValue = new(0);
    public IReadOnlyReactiveProperty<float> Value => _value;
    public IReadOnlyReactiveProperty<float> MaxValue => _maxValue;

    int IProxyView.ProxyControlID { get; set; } = -1;

    void IRangeProxyView<float>.OnValueChanged(float value, float maxValue)
    {
        _value.Value = value;
        _maxValue.Value = maxValue; 
        Debug.Log($"Value: {value}", this.GetTag().ToString());
    }
}
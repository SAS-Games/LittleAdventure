using SAS.Utilities.TagSystem;
using UniRx;
using UnityEngine;

public interface IProxyView<T>
{
    int ProxyControlID { get; set; }
    void OnValueChanged(T value);
    IReadOnlyReactiveProperty<T> Value { get; }
}
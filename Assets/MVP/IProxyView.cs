using SAS.Utilities.TagSystem;
using UniRx;

public interface IProxyView<T> : ServiceLocator.IService
{
    void OnValueChanged(T value);
    IReadOnlyReactiveProperty<T> Value { get; }
}
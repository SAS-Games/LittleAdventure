using UniRx;

public interface IProxyView<T>
{
    void OnValueChanged(T value);
    IReadOnlyReactiveProperty<T> Value { get; }
}
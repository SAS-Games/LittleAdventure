using UniRx;

public interface IProxyView
{ 
    int ProxyControlID { get; set; }
}

public interface IProxyView<T>: IProxyView
{
    void OnValueChanged(T value);
    IReadOnlyReactiveProperty<T> Value { get; }
}
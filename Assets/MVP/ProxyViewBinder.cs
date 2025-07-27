using UniRx;

public abstract class ProxyViewBinder<T> : ProxyViewBinderBase<IProxyView<T>>
{
    protected override void Bind(IProxyView<T> view, CompositeDisposable disposable)
    {
        view.Value.Subscribe(OnValueChanged).AddTo(disposable);
    }

    protected abstract void OnValueChanged(T value);
}
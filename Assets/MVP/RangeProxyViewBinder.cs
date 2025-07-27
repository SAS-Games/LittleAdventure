using UniRx;

public abstract class RangeProxyViewBinder<T> : ProxyViewBinderBase<IRangeProxyView<T>>
{
    protected override void Bind(IRangeProxyView<T> view, CompositeDisposable disposable)
    {
        view.Value.CombineLatest(view.MaxValue, (cur, max) => (cur, max))
            .Subscribe(t => OnValueChanged(t.cur, t.max))
            .AddTo(disposable);
    }

    protected abstract void OnValueChanged(T current, T max);
}
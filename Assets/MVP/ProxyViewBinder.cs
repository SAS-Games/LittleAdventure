using UnityEngine;
using UniRx;
using SAS.Utilities.TagSystem;
using Debug = SAS.Debug;

public abstract class ProxyViewBinder<T> : MonoBehaviour, MetaLocator.IHandler
{
    private CompositeDisposable _disposable = new();

    public virtual void OnCoreLoaded(MetaLocator metaLocator)
    {
        if (metaLocator.TryGet(out IProxyView<T> proxyView, this.GetTag()))
        {
            proxyView.Value.Subscribe(OnValueChanged).AddTo(_disposable);
        }
        else
        {
            Debug.LogError(
                $"[ProxyViewBinder<{typeof(T).Name}>] No ProxyView found with tag '{this.GetTag()}' on '{gameObject.name}'",
                "ProxyViewBinder");
        }
    }

    public void OnMetaLoaded(MetaLocator metaLocator)
    {
    }

    protected abstract void OnValueChanged(T value);

    protected virtual void OnDestroy()
    {
        _disposable.Dispose();
    }
}
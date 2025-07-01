using UnityEngine;
using UniRx;
using SAS.Utilities.TagSystem;
using Debug = SAS.Debug;
using ZLinq;

public abstract class ProxyViewBinder<T> : MonoBehaviour, MetaLocator.IHandler
{
    [SerializeField] private int _proxyControlID = -1;
    private CompositeDisposable _disposable = new();


    public virtual void OnCoreLoaded(MetaLocator metaLocator)
    {
        IProxyView<T> matchedProxyView = null;

        if (_proxyControlID == -1)
        {
            metaLocator.TryGet(out matchedProxyView, this.GetTag());
        }
        else
        {
            var proxyViews = metaLocator.GetAll<IProxyView<T>>(this.GetTag());
            matchedProxyView = proxyViews.AsValueEnumerable()
                                         .FirstOrDefault(p => p.ProxyControlID == _proxyControlID);
        }

        if (matchedProxyView != null)
            matchedProxyView.Value.Subscribe(OnValueChanged).AddTo(_disposable);
        else
            Debug.LogError($"[ProxyViewBinder<{typeof(T).Name}>] No ProxyView found with tag '{this.GetTag()}' and ControlID '{_proxyControlID}' on '{gameObject.name}'", "ProxyViewBinder");
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
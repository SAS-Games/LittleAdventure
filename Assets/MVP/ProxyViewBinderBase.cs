using UnityEngine;
using UniRx;
using SAS.Core.TagSystem;
using Debug = SAS.Debug;
using ZLinq;

public abstract class ProxyViewBinderBase<TView> : MonoBehaviour, MetaLocator.IHandler where TView : class, IProxyView
{
    [SerializeField] private int _proxyControlID = -1;
    private CompositeDisposable _disposable = new();

    public virtual void OnCoreLoaded(MetaLocator metaLocator)
    {
        TView matchedView = null;

        if (_proxyControlID == -1)
        {
            metaLocator.TryGet(out matchedView, this.GetTag());
        }
        else
        {
            var views = metaLocator.GetAll<TView>(this.GetTag());
            matchedView = views.AsValueEnumerable()
                .FirstOrDefault(p => p.ProxyControlID == _proxyControlID);
        }

        if (matchedView != null)
            Bind(matchedView, _disposable);
        else
            Debug.LogWarning($"[{nameof(ProxyViewBinderBase<TView>)}<{typeof(TView).Name}>] No view found with tag '{this.GetTag()}' and ControlID '{_proxyControlID}' on '{gameObject.name}'", "ProxyViewBinder");
    }

    public void OnMetaLoaded(MetaLocator metaLocator) { }

    protected abstract void Bind(TView view, CompositeDisposable disposable);

    protected virtual void OnDestroy() => _disposable.Dispose();
}
using UnityEngine;
using UniRx;
using SAS.Utilities.TagSystem;
using UnityEngine.UI;
using Debug = SAS.Debug;

[RequireComponent(typeof(Button))]
public class ProxyButtonBinder : MonoBehaviour, MetaLocator.IHandler
{
    private bool _isSubscribed = false;

    public void OnCoreLoaded(MetaLocator metaLocator)
    {
        TrySubscribe(metaLocator);
    }

    public void OnMetaLoaded(MetaLocator metaLocator)
    {
        TrySubscribe(metaLocator);
    }

    private void TrySubscribe(MetaLocator metaLocator)
    {
        if (_isSubscribed)
            return;

        if (metaLocator.TryGet(out IProxyButton proxyButton, this.GetTag()))
        {
            GetComponent<Button>()
                .OnClickAsObservable()
                .Subscribe(_ => proxyButton.OnClick())
                .AddTo(this);

            _isSubscribed = true;
        }
        else
        {
            Debug.LogError(
                $"[ProxyButtonBinder<{name}>] No ProxyButton found with tag '{this.GetTag()}' on '{gameObject.name}'",
                "ProxyButtonBinder");
        }
    }
}
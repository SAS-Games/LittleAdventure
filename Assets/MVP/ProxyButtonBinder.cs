using UnityEngine;
using UniRx;
using SAS.Core.TagSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ProxyButtonBinder : MonoBehaviour, MetaLocator.IHandler
{
    private bool _isSubscribed = false;

    public void OnCoreLoaded(MetaLocator metaLocator)
    {
        TrySubscribe(metaLocator, isCore: true);
    }

    public void OnMetaLoaded(MetaLocator metaLocator)
    {
        TrySubscribe(metaLocator, isCore: false);
    }

    private void TrySubscribe(MetaLocator metaLocator, bool isCore)
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
            var message = $"[ProxyButtonBinder<{name}>] No ProxyButton found with tag '{this.GetTag()}' on '{gameObject.name}'";
            if (isCore)
                Debug.LogError($"OnCoreLoaded : {message}");
            else
                Debug.LogWarning($"OnMetaLoaded : {message}");
        }
    }
}
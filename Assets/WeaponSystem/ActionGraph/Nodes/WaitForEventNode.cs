using System;
using System.Threading;
using UnityEngine;

namespace SAS.ActionGraph.WeaponSystem
{
    [Serializable]
    public class WaitForEventData
    {
        public string eventName = "OnAttackAction";
        public float timeoutSeconds = -1f;
        public bool throwOnTimeout;
    }

    [NodeBinding(typeof(WaitForEventNode))]
    [Serializable]
    public class WaitForEventProvider : ActionDataProvider<WaitForEventData>
    {
    }

    [ActionNodeMenu("Event/Wait For Event", "Pauses graph execution until the named owner event fires or the timeout expires.")]
    public class WaitForEventNode : ActionNode<WaitForEventData>
    {
        public WaitForEventNode(ActionDataProvider<WaitForEventData> dataProvider) : base(dataProvider)
        {
        }

        public override async Awaitable ExecuteAsync(ActionContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            WaitForEventData data = _selector.GetNext();
            if (data == null || string.IsNullOrEmpty(data.eventName))
                return;

            IEventDispatcher dispatcher = context.Owner != null
                ? context.Owner.GetComponentInParent<IEventDispatcher>()
                : null;

            if (dispatcher == null)
            {
                if (data.throwOnTimeout)
                    throw new InvalidOperationException(
                        "WaitForEventNode requires an IEventDispatcher on the owner hierarchy.");

                return;
            }

            bool fired = false;
            Action callback = () => fired = true;
            dispatcher.Subscribe(data.eventName, callback);

            try
            {
                float elapsed = 0f;
                while (!fired)
                {
                    token.ThrowIfCancellationRequested();

                    if (data.timeoutSeconds >= 0f && elapsed >= data.timeoutSeconds)
                    {
                        if (data.throwOnTimeout)
                            throw new TimeoutException(
                                $"WaitForEventNode timed out waiting for event '{data.eventName}'.");

                        return;
                    }

                    await Awaitable.NextFrameAsync(token);
                    elapsed += Time.deltaTime;
                }
            }
            finally
            {
                dispatcher.Unsubscribe(data.eventName, callback);
            }
        }
    }
}

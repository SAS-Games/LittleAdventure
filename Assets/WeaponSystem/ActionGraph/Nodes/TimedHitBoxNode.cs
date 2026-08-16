using System;
using System.Threading;
using UnityEngine;

namespace SAS.ActionGraph.WeaponSystem
{
    [Serializable]
    public class WeaponTimedHitBoxData
    {
        public Bounds hitBox = new Bounds(new Vector3(0f, 0.75f, 0.5f), new Vector3(1.2f, 1.5f, 1f));
        public LayerMask layers = -1;
        public float startTime = 0.17f;
        public float endTime = 0.35f;
        public string stateTag = "Attack";
        public int layer;
        public int maxHits = 10;
        public bool ignoreOwner = true;
        public bool groupHitsByRoot;
        public QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal;
    }

    [NodeBinding(typeof(TimedHitBoxNode))]
    [Serializable]
    public class WeaponTimedHitBoxProvider : ActionDataProvider<WeaponTimedHitBoxData>, IIndexedActionDataProvider
    {
    }

    [ActionNodeMenu("Weapon/Timed Hit Box", "Collects unique targets inside a box during the configured animation time window.")]
    public class TimedHitBoxNode : WeaponActionNode<WeaponTimedHitBoxData>
    {
        private Collider[] _results = new Collider[10];

        public TimedHitBoxNode(ActionDataProvider<WeaponTimedHitBoxData> dataProvider) : base(dataProvider)
        {
        }

        public override async Awaitable ExecuteAsync(ActionContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var weaponContext = RequireWeaponContext(context);
            var data = GetAttackData(weaponContext) ?? new WeaponTimedHitBoxData();
            Bounds hitBox = data.hitBox;
            LayerMask layers = data.layers;
            float startTime = data.startTime;
            float endTime = data.endTime;
            string stateTag = data.stateTag;

            weaponContext.ClearHits();

            if (_results == null || _results.Length != Mathf.Max(1, data.maxHits))
                _results = new Collider[Mathf.Max(1, data.maxHits)];

            if (weaponContext.Animator == null)
            {
                Detect(weaponContext, data, hitBox, layers);
                return;
            }

            bool enteredAttackState = false;
            while (true)
            {
                token.ThrowIfCancellationRequested();

                AnimatorStateInfo stateInfo = weaponContext.Animator.GetCurrentAnimatorStateInfo(data.layer);
                bool inAttackState = stateInfo.IsTag(stateTag);

                if (inAttackState)
                    enteredAttackState = true;
                else if (enteredAttackState)
                    return;

                if (inAttackState && stateInfo.normalizedTime >= startTime && stateInfo.normalizedTime <= endTime)
                    Detect(weaponContext, data, hitBox, layers);

                if (inAttackState && stateInfo.normalizedTime > endTime)
                    return;

                await Awaitable.NextFrameAsync(token);
            }
        }

        private void Detect(WeaponContext weaponContext, WeaponTimedHitBoxData data, Bounds hitBox, LayerMask layers)
        {
            Transform origin = weaponContext.OriginTransform;
            if (origin == null)
                return;

            Vector3 forward = WeaponNodeUtility.GetOwnerForward(weaponContext);
            Vector3 center = origin.position +
                             forward * hitBox.center.z +
                             origin.right * hitBox.center.x +
                             origin.up * hitBox.center.y;

            int hitCount = Physics.OverlapBoxNonAlloc(
                center,
                hitBox.extents,
                _results,
                origin.rotation,
                layers,
                data.triggerInteraction);

            for (int i = 0; i < hitCount; i++)
            {
                Collider collider = _results[i];
                if (collider == null)
                    continue;

                if (data.ignoreOwner &&
                    weaponContext.Owner != null &&
                    collider.transform.root == weaponContext.Owner.transform.root)
                {
                    continue;
                }

                weaponContext.TryRegisterHit(collider, collider.ClosestPoint(center), data.groupHitsByRoot);
            }
        }
    }
}

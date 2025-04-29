using SAS.Pool;
using SAS.TweenManagement;
using UnityEngine;

namespace SAS.Collectables
{
    public class Coin : BaseCollectable<Coin>, ISpawnable
    {
        private TweenMonoBase _tweenMonoBase;

        protected override void Awake()
        {
            base.Awake();
            _tweenMonoBase = GetComponent<TweenMonoBase>();
        }

        void ISpawnable.OnSpawn(object data)
        {
            if (data is SpawnData spawnData)
                this.transform.SetPositionAndRotation(spawnData.Point.transform.position,
                    spawnData.Point.transform.rotation);

            if (_tweenMonoBase != null)
            {
                var speed = _tweenMonoBase.UpdateAndGetRefToParamConfig().DurationOrSpeed;
                _tweenMonoBase.UpdateAndGetRefToParamConfig().Speed(speed * Random.Range(1f, 1.5f));
                _tweenMonoBase.Play();
            }
        }

        void ISpawnable.OnDespawn()
        {
            _tweenMonoBase.TweenInstance.Stop(true);
        }
    }
}
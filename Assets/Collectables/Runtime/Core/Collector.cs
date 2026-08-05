using UnityEngine;

namespace SAS.Collectables
{
    public class Collector : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            var collectable = other.GetComponentInParent<ICollectable>();
            if (collectable != null)
                Collect(collectable);
        }

        protected virtual void Collect(ICollectable collectable)
        {
            collectable.Collect(this);
        }
    }
}

using System;
using UnityEngine;

namespace SAS.Checkpoints
{
    [DisallowMultipleComponent]
    public sealed class SpawnPoint : MonoBehaviour
    {
        [SerializeField] private Transform m_SpawnTransform;
        public GameObject SpawnedObject { get; private set; }
        public bool IsOccupied => SpawnedObject != null;
        public Vector3 Position => SpawnTransform.position;
        public Quaternion Rotation => SpawnTransform.rotation;
        private Transform SpawnTransform => m_SpawnTransform != null ? m_SpawnTransform : transform;
        
        public void Assign(GameObject spawnedObject)
        {
            if (spawnedObject == null)
                throw new ArgumentNullException(nameof(spawnedObject));

            SpawnedObject = spawnedObject;
        }

        public void Release(GameObject spawnedObject)
        {
            if (spawnedObject != null && SpawnedObject == spawnedObject)
                SpawnedObject = null;
        }

        public void ForceRelease()
        {
            SpawnedObject = null;
        }

        private void OnDrawGizmos()
        {
            Transform spawnTransform = SpawnTransform;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnTransform.position, 0.35f);
            Gizmos.DrawLine(spawnTransform.position, spawnTransform.position + spawnTransform.forward);
        }
    }
}

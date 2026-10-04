using UnityEngine;

namespace LevelStreaming
{
    public class SetTarget : MonoBehaviour
    {
        [SerializeField] RegionStreamingController m_RegionStreamingController;

        void Start()
        {
            if (m_RegionStreamingController == null)
            {
                Debug.LogError("No RegionStreamingController assigned.", this);
                return;
            }

            var provider = GetComponent<IStreamingVolumeProvider>();
            if (provider == null)
            {
                Debug.LogError("No IStreamingVolumeProvider found on this object.", this);
                return;
            }

            m_RegionStreamingController.SetStreamingVolumeProvider(provider);
        }
    }
}

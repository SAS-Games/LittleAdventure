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

            var provider = GetComponent<DefaultStreamingBoundsProvider>();
            if (provider == null)
            {
                Debug.LogError("No DefaultStreamingBoundsProvider found on this object.", this);
                return;
            }

            m_RegionStreamingController.SetRegionLoadBoundsProvider(provider);
        }
    }
}

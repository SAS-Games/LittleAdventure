using UnityEngine;

namespace LevelStreaming
{
    public class SetTarget : MonoBehaviour
    {
        [SerializeField] RegionStreamingController m_RegionStreamingController;

        void Start()
        {
            m_RegionStreamingController.SetRegionLoadBoundsProvider(GetComponent<DefaultStreamingBoundsProvider>());
        }
    }
}

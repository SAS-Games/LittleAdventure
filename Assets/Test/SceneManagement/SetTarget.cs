using UnityEngine;

public class SetTarget : MonoBehaviour
{
    [SerializeField] StreamingController streamingController;
    void Start()
    {
        streamingController.SetRegionLoadBoundsProvider(GetComponent<PlayerRegionBounds>());
    }
}

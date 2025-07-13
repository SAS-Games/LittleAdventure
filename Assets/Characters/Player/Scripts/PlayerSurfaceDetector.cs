using FMODUnity;
using UnityEngine;
using Debug = SAS.Debug;

public class PlayerSurfaceDetector : MonoBehaviour
{
    [SerializeField] private Transform m_PlayerTransform;
    [SerializeField] private float m_CheckDistance = 1.5f;
    [SerializeField] private LayerMask m_ChecksurfaceLayerMask = ~0;
    [SerializeField] private EventReference m_EventRef;
    [SerializeField] private string m_MaterialTag = "Material";
    private float _material;

    void Start()
    {
        //FMOD.Studio.ma
    }

    private void FixedUpdate()
    {
        DetectSurfaceBelow();
    }

    private void DetectSurfaceBelow()
    {
        if (Physics.Raycast(m_PlayerTransform.position, Vector3.down, out var hit, m_CheckDistance,
                m_ChecksurfaceLayerMask, QueryTriggerInteraction.Ignore))
        {
            GameObject surface = hit.collider.gameObject;

            Debug.DrawRay(m_PlayerTransform.position, Vector3.down * m_CheckDistance, Color.green);
            Debug.Log("Surface Tag: " + surface.tag);
            if (hit.collider.CompareTag("Earth"))
                _material = 1;
            else if (hit.collider.CompareTag("Water"))
                _material = 2;
            else
                _material = 1;
        }
        else
        {
            Debug.DrawRay(m_PlayerTransform.position, Vector3.down * m_CheckDistance, Color.red);
            Debug.Log("No surface detected below player.");
        }
    }

    private void PlayFootstepEvent()
    {
        var eventInstance = RuntimeManager.CreateInstance(m_EventRef);
        eventInstance.start();
        eventInstance.setParameterByName(m_MaterialTag, _material);
        eventInstance.release();
    }
}
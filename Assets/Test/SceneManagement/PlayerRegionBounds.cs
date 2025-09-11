using UnityEngine;
using UnityEngine.Serialization;

public class PlayerRegionBounds : MonoBehaviour, IRegionLoadBoundsProvider
{
    [Header("Load/Unload Settings")]
    [SerializeField] private Vector3 m_LoadBoundsSize = new(20, 10, 20);
    [SerializeField] private Vector3 m_ActivateBoundsSize = new(10, 5, 10);
    [SerializeField] private Vector3 m_UnloadBoundsSize = new(30, 15, 30);

    // Cached bounds
    private Bounds _loadBounds;
    private Bounds _activateBounds;
    private Bounds _unloadBounds;

    private void Awake()
    {
       UpdateCachedBounds();
    }

    private void OnValidate()
    {
        UpdateCachedBounds();
    }

    private void UpdateCachedBounds()
    {
        _loadBounds.size = m_LoadBoundsSize;
        _activateBounds.size = m_ActivateBoundsSize;
        _unloadBounds.size = m_UnloadBoundsSize;

        _loadBounds.center = transform.position;
        _activateBounds.center = transform.position;
        _unloadBounds.center = transform.position;
    }

    public Bounds GetLoadBounds()
    {
        _loadBounds.center = transform.position;
        return _loadBounds;
    }

    public Bounds GetUnloadBounds()
    {
        _unloadBounds.center = transform.position;
        return _unloadBounds;
    }

    public Bounds GetActivateBounds()
    {
        _activateBounds.center = transform.position;
        return _activateBounds;
    }
}

using UnityEngine;

public class PlayerRegionBounds : MonoBehaviour, IRegionLoadBoundsProvider
{
    [Header("Load/Unload Settings")]
    [SerializeField] private Vector3 loadBoundsSize = new(20, 10, 20);
    [SerializeField] private Vector3 unloadBoundsSize = new(30, 15, 30);

    public Bounds GetLoadBounds()
    {
        return new Bounds(transform.position, loadBoundsSize);
    }

    public Bounds GetUnloadBounds()
    {
        return new Bounds(transform.position, unloadBoundsSize);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        Gizmos.DrawCube(transform.position, loadBoundsSize);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, loadBoundsSize);

        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawCube(transform.position, unloadBoundsSize);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, unloadBoundsSize);
    }
#endif
}
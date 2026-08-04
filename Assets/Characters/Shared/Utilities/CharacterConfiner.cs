using UnityEngine;
using System.Collections.Generic;

public class DistanceLimiter : MonoBehaviour, IObjectSpawnedListener
{
    [SerializeField] private float m_MaxDistance = 10f;

    [Tooltip("If true, only the moving player is corrected. If false, both players are corrected equally.")]
    [SerializeField] private bool m_UseAsymmetricCorrection = true;

    private List<CharacterController> _controllers = new();
    private Dictionary<CharacterController, Vector3> _lastPositions = new();

    public void OnDespawn(GameObject gameObject)
    {
        if (gameObject.TryGetComponent(out CharacterController controller))
        {
            _controllers.Remove(controller);
            _lastPositions.Remove(controller);
        }
    }

    public void OnSpawn(GameObject gameObject)
    {
        if (gameObject.TryGetComponent(out CharacterController controller))
        {
            _controllers.Add(controller);
            _lastPositions[controller] = controller.transform.position;
        }
    }

    private void LateUpdate()
    {
        if (_controllers.Count < 2) return;

        for (int i = 0; i < _controllers.Count; i++)
        {
            for (int j = i + 1; j < _controllers.Count; j++)
            {
                var a = _controllers[i];
                var b = _controllers[j];

                Vector3 aPos = a.transform.position;
                Vector3 bPos = b.transform.position;

                float distance = Vector3.Distance(aPos, bPos);
                if (distance > m_MaxDistance)
                {
                    Vector3 midpoint = (aPos + bPos) * 0.5f;
                    float clampedDistance = m_MaxDistance * 0.5f;

                    Vector3 dirA = (aPos - midpoint).normalized;
                    Vector3 dirB = (bPos - midpoint).normalized;

                    Vector3 newAPos = midpoint + dirA * clampedDistance;
                    Vector3 newBPos = midpoint + dirB * clampedDistance;

                    Vector3 aCorrection = newAPos - aPos;
                    Vector3 bCorrection = newBPos - bPos;

                    if (m_UseAsymmetricCorrection)
                    {
                        float aDelta = (aPos - _lastPositions[a]).sqrMagnitude;
                        float bDelta = (bPos - _lastPositions[b]).sqrMagnitude;

                        if (aDelta > bDelta)
                            a.Move(aCorrection);
                        else
                            b.Move(bCorrection);
                    }
                    else
                    {
                        a.Move(aCorrection);
                        b.Move(bCorrection);
                    }
                }
            }
        }

        foreach (var controller in _controllers)
        {
            _lastPositions[controller] = controller.transform.position;
        }
    }
}

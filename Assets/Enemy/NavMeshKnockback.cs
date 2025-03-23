using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class NavMeshKnockback : KnockbackBase
{
    private NavMeshAgent _agent;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    protected override void ApplyKnockback(Vector3 movement)
    {
        if (_agent == null)
            return;

        _agent.isStopped = true;

        transform.position += movement * Time.deltaTime;

        if (movement == Vector3.zero)
            _agent.isStopped = false;
    }
}

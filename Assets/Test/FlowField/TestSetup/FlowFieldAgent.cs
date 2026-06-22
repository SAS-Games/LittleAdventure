using UnityEngine;
using Unity.Mathematics;

public class FlowFieldAgent : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;

    [Header("Avoidance")]
    public float radius = 0.5f;
    public float neighbourRadius = 2f;
    public float separationWeight = 4f;

    // Dedicated global buffer array used exclusively by ComputeSeparation
    private static readonly Collider[] SeparationHits = new Collider[32];
    
    // Dedicated buffer to completely prevent memory overwrite bugs inside WouldCollide
    private static readonly Collider[] CollisionHits = new Collider[8];

    private void Update()
    {
        var sampler = FlowFieldManager.Instance.ActiveSampler;
        if (sampler == null)
            return;

        float dt = Time.deltaTime;

        // 1. Core Flow Field Vector Goal
        float2 flow = sampler.SampleDirection(transform.position);
        Vector3 desiredVelocity = new Vector3(flow.x, 0f, flow.y) * speed;

        // 2. Compute Push Separation Forces
        Vector3 separation = ComputeSeparation();

        // 3. Initial Blended Path Velocity
        Vector3 velocity = desiredVelocity + (separation * separationWeight);

        // 4. Predictive Local Avoiding Sweep Pass
        Vector3 nextPosition = transform.position + (velocity * dt);
        
        if (TryPredictiveDodge(nextPosition, out Vector3 dodgeForce))
        {
            velocity += dodgeForce * speed;
        }

        // 5. CRITICAL WALL SAFETY LOOK-AHEAD:
        // Validate if our intended velocity vector pushes us into a wall cell
        velocity = RestrictVelocityByGrid(velocity, dt);

        // 6. Final Clamp & Apply Posture Positions
        if (velocity.sqrMagnitude > 0.001f)
        {
            velocity = Vector3.ClampMagnitude(velocity, speed);
        }
        else
        {
            velocity = Vector3.zero;
        }

        transform.position += velocity * dt;
    }

    private Vector3 ComputeSeparation()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, neighbourRadius, SeparationHits);
        Vector3 force = Vector3.zero;

        for (int i = 0; i < count; i++)
        {
            Collider hit = SeparationHits[i];
            if (hit == null || hit.transform == transform) continue;

            FlowFieldAgent other = hit.GetComponent<FlowFieldAgent>();
            if (other == null) continue;

            Vector3 offset = transform.position - other.transform.position;
            offset.y = 0f;

            float distance = offset.magnitude;
            if (distance < 0.001f) continue;

            float strength = 1f - Mathf.Clamp01(distance / neighbourRadius);
            force += (offset / distance) * strength;
        }

        return force;
    }

    private bool TryPredictiveDodge(Vector3 nextPosition, out Vector3 dodgeForce)
    {
        dodgeForce = Vector3.zero;
        int count = Physics.OverlapSphereNonAlloc(nextPosition, radius, CollisionHits);
        bool imminentCollision = false;

        for (int i = 0; i < count; i++)
        {
            Collider hit = CollisionHits[i];
            if (hit == null || hit.transform == transform) continue;

            FlowFieldAgent other = hit.GetComponent<FlowFieldAgent>();
            if (other == null) continue;

            Vector3 pushAway = transform.position - other.transform.position;
            pushAway.y = 0f;
            
            dodgeForce += pushAway.normalized;
            imminentCollision = true;
        }

        if (imminentCollision)
        {
            dodgeForce = dodgeForce.normalized;
        }

        return imminentCollision;
    }

    /// <summary>
    /// Checks the grid cost ahead of the agent. If the intended velocity points 
    /// directly into a wall block, it slides the vector along the wall face instead.
    /// </summary>
    private Vector3 RestrictVelocityByGrid(Vector3 currentVelocity, float dt)
    {
        if (FlowFieldManager.Instance.ActiveFlowField == null) return currentVelocity;

        var grid = FlowFieldManager.Instance.ActiveFlowField.Grid;
        if (!grid.Cost.IsCreated) return currentVelocity;

        // Test position slightly ahead based on agent's physical footprint radius
        Vector3 lookAheadPos = transform.position + currentVelocity.normalized * radius;

        int2 cell = FlowFieldGridUtility.WorldToCell(lookAheadPos, grid.Origin, grid.CellSize);

        // If target cell is out of grid boundaries or is a hard obstacle block
        if (!FlowFieldGridUtility.IsValidCell(cell, grid.Width, grid.Height) || 
            grid.Cost[cell.x + cell.y * grid.Width] == byte.MaxValue)
        {
            // Project the velocity out on X and Z axes independently to see which axis can still move (sliding mechanics)
            Vector3 targetX = new Vector3(currentVelocity.x, 0f, 0f);
            Vector3 targetZ = new Vector3(0f, 0f, currentVelocity.z);

            Vector3 lookAheadX = transform.position + targetX.normalized * radius;
            Vector3 lookAheadZ = transform.position + targetZ.normalized * radius;

            int2 cellX = FlowFieldGridUtility.WorldToCell(lookAheadX, grid.Origin, grid.CellSize);
            int2 cellZ = FlowFieldGridUtility.WorldToCell(lookAheadZ, grid.Origin, grid.CellSize);

            bool xValid = FlowFieldGridUtility.IsValidCell(cellX, grid.Width, grid.Height) && grid.Cost[cellX.x + cellX.y * grid.Width] != byte.MaxValue;
            bool zValid = FlowFieldGridUtility.IsValidCell(cellZ, grid.Width, grid.Height) && grid.Cost[cellZ.x + cellZ.y * grid.Width] != byte.MaxValue;

            if (xValid && !zValid) return targetX;
            if (!xValid && zValid) return targetZ;
            
            return Vector3.zero; // Completely stuck in a corner block, force full stop to save physics colliders
        }

        return currentVelocity;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, neighbourRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}

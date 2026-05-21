using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNavigation : MonoBehaviour
{
    // If true, NavMeshAgent will move the transform itself (not used in your current setup)
    // If false, we use the agent only for pathfinding/steering and your CharacterController moves the enemy.
    private static readonly bool AGENT_HANDLES_MOVEMENT = false;

    private NavMeshAgent agent;
    private Enemy enemy;

    bool IsDead => enemy.IsDead;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemy = GetComponent<Enemy>();

        // Manual movement mode: agent does NOT move or rotate the transform
        agent.updatePosition = AGENT_HANDLES_MOVEMENT;
        agent.updateRotation = AGENT_HANDLES_MOVEMENT;
        agent.updateUpAxis = true; // keep this true for normal humanoids

        // Auto-align agent to navmesh height
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            agent.nextPosition = transform.position;
        }
        else
        {
            Debug.LogError($"{name}: Could not find navmesh under enemy!", this);
        }

    }
    void LateUpdate()
    {
        if (!agent.isOnNavMesh) return;

        agent.nextPosition = transform.position;
    }
    /// <summary>
    /// Returns the current desired movement direction as a 2D vector (x,z),
    /// based on the NavMeshAgent's steering target / desired velocity.
    /// This is used by EnemyController to set MoveDirection for the CharacterController.
    /// </summary>
    public Vector2 MoveDirection()
    {
        if (IsDead) return Vector2.zero;

        // If agent has no path yet (or path is done), no movement
        if (!agent.hasPath) return Vector2.zero;

        // Primary option: use desiredVelocity (already a good steering vector)
        Vector3 dir = agent.desiredVelocity;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
        {
            // Fallback: use steeringTarget - our position
            Vector3 toCorner = agent.steeringTarget - transform.position;
            toCorner.y = 0f;

            if (toCorner.sqrMagnitude < 0.0001f) return Vector2.zero;

            dir = toCorner;
        }

        dir.Normalize();
        return new Vector2(dir.x, dir.z);
    }

    /// <summary>
    /// Request a path to targetPos. Returns true if the request was accepted.
    /// Note: This does NOT mean the path is valid yet (use QueryPathTo for that).
    /// </summary>
    public bool MoveTo(Vector3 targetPos)
    {
        if (IsDead) return false;

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning($"{name}: NavMeshAgent is not on a NavMesh. Cannot MoveTo().", this);
            return false;
        }

        bool success = agent.SetDestination(targetPos);

        if (success) agent.isStopped = false;

        return success;
    }

    /// <summary>
    /// Pick a random point projected onto the NavMesh near 'origin' within 'radius'.
    /// Used for patrol.
    /// </summary>
    public bool TryGetPatrolPoint(Vector3 origin, float radius, float sampleMaxDistance, int maxTries, out Vector3 point)
    {
        point = origin;

        if (IsDead)
            return false;

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning($"{name}: NavMeshAgent is not on a NavMesh. Cannot sample patrol points.", this);
            return false;
        }

        for (int i = 0; i < maxTries; i++)
        {
            float r = Random.Range(radius * 0.4f, radius);
            float ang = Random.Range(0f, Mathf.PI * 2f);
            Vector3 candidate = origin + new Vector3(Mathf.Cos(ang) * r, 0f, Mathf.Sin(ang) * r);

            if (NavMesh.SamplePosition(candidate, out var hit, sampleMaxDistance, NavMesh.AllAreas))
            {
                point = hit.position;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// True when we've effectively arrived at the agent destination.
    /// This is used by EnemyController for patrol / walk logic.
    /// </summary>
    public bool HasReachedDestination()
    {
        if (IsDead) return true;

        if (agent.pathPending) return false;

        // remainingDistance is valid even when updatePosition=false (it uses internal nextPosition)
        return agent.hasPath && agent.remainingDistance <= agent.stoppingDistance;
    }
}

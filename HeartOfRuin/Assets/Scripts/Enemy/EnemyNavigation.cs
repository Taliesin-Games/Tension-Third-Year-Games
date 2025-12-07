using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;


[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNavigation : MonoBehaviour
{
    static bool AGENT_HANDLES_MOVEMENTY = false;

    NavMeshAgent agent;
    Enemy enemy;
    Transform target;
    bool HasPath => agent.hasPath;

    [SerializeField] bool debugPath = true;

    bool IsDead => enemy.IsDead;

    void Awake()
    {
        //cache the navmesh agent
        agent = GetComponent<NavMeshAgent>();

        enemy = GetComponent<Enemy>();

        // TODO, this needs integrating properly. It disables nav agent movement and just uses it for dection making.
        agent.updatePosition = AGENT_HANDLES_MOVEMENTY;
        agent.updateRotation = AGENT_HANDLES_MOVEMENTY;
        agent.updateUpAxis = true;
    }

    public Vector2 MoveDirection()
    {
        if (!agent.hasPath) return Vector2.zero;

        Vector3 toCorner = agent.steeringTarget - transform.position;
        toCorner.y = 0;

        if (toCorner.sqrMagnitude < 0.01f) return Vector2.zero;

        toCorner.Normalize();
        return new Vector2(toCorner.x, toCorner.z);
    }

    public bool MoveTo(Vector3 targetPos)
    {
        // Set the agent's destination
        bool success = agent.SetDestination(targetPos);

        return success;
    }

    // Synchronously query a path from current position to a target and report if it truly reaches it.
    public PathQueryResult QueryPathTo(Vector3 targetPos, float endTolerance = 0.25f)
    {
        // If dead, no path
        if (IsDead)
        {
            return new PathQueryResult
            {
                Found = false,
                Status = NavMeshPathStatus.PathInvalid,
                EndPosition = agent.transform.position,
                ReachesTarget = false
            };
        }

        NavMeshPath path = new NavMeshPath();
        bool ok = NavMesh.CalculatePath(agent.transform.position, targetPos, NavMesh.AllAreas, path); // true if a path was found


        // Determine the actual end position of the path
        Vector3 end = agent.transform.position;
        if (path.corners != null && path.corners.Length > 0)
        {
            end = path.corners[path.corners.Length - 1];
        }

        NavMeshPathStatus status = path.status;
        // Determine if the path actually reaches the target
        bool reachesTarget = ok && status == NavMeshPathStatus.PathComplete &&
                             (end - targetPos).sqrMagnitude <= (endTolerance * endTolerance);

        // Debug draw the path
        if (debugPath && path.corners != null && path.corners.Length > 1)
        {
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                Debug.DrawLine(path.corners[i], path.corners[i + 1], reachesTarget ? Color.green : Color.yellow);
            }
        }

        // Return the result
        return new PathQueryResult
        {
            Found = ok,
            Status = status,
            EndPosition = end,
            ReachesTarget = reachesTarget
        };
    }

    // Pick a random point projected onto the NavMesh near 'origin' within 'radius'.
    public bool TryGetPatrolPoint(Vector3 origin, float radius, float sampleMaxDistance, int maxTries, out Vector3 point)
    {
        if (IsDead)
        {
            point = origin;
            return false;
        }

        // Try several times to find a valid point
        for (int i = 0; i < maxTries; i++)
        {
            //pick a random point in the circle
            float r = Random.Range(radius * 0.4f, radius);
            float ang = Random.Range(0f, Mathf.PI * 2f);
            Vector3 candidate = origin + new Vector3(Mathf.Cos(ang) * r, 0f, Mathf.Sin(ang) * r);

            //if it's on the navmesh, return it
            if (NavMesh.SamplePosition(candidate, out var hit, sampleMaxDistance, NavMesh.AllAreas))
            {
                point = hit.position;
                return true;
            }
        }

        point = origin;
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        

        if (IsDead)
        {
            agent.isStopped = true;
            return;
        }

        agent.nextPosition = transform.position;

        if (!agent.hasPath && !debugPath)
        {
            return;
        }

        // Debug draw the current path if any
        NavMeshPath path = agent.path;
        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);
        }
    }

    public bool HasReachedDestination()
    {
        if (IsDead) return true;

        if (agent.pathPending) return false;

        return agent.remainingDistance <= agent.stoppingDistance;
    }

}

// Result of a path query
public struct PathQueryResult
{
    public bool Found;                  // CalculatePath succeeded
    public NavMeshPathStatus Status;    // Complete / Partial / Invalid
    public Vector3 EndPosition;         // End of the computed path
    public bool ReachesTarget;          // True only if the path actually reaches the target
}
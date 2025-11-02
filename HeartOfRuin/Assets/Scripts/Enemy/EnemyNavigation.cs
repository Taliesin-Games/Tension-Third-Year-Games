using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNavigation : MonoBehaviour
{
    NavMeshAgent agent;
    Transform target;
    bool HasPath => agent.hasPath;

    [SerializeField] bool debugPath = true;

    bool isDead = false;

    void Awake()
    {
        //cache the navmesh agent
        agent = GetComponent<NavMeshAgent>();
    }


    public bool MoveTo(Vector3 targetPos)
    {
        // Set the agent's destination
        // just a wrapper around SetDestination for nowq
        return agent.SetDestination(targetPos);
    }

    // Synchronously query a path from current position to a target and report if it truly reaches it.
    public PathQueryResult QueryPathTo(Vector3 targetPos, float endTolerance = 0.25f)
    {
        // If dead, no path
        if (isDead)
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
        if (isDead)
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

        if (isDead)
        {
            agent.isStopped = true;
            return;
        }

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
        // If dead, consider reached.
        if (isDead)
        {
            return true;
        }

        // Check if the agent has reached its destination
        if (agent.pathPending)
        {
            return false;
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                return true;
            }
        }
        return false;
    }

    public void Die()
    {
        isDead = true;
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
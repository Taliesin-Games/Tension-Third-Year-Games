using BMD.ProcGen;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshTester : MonoBehaviour
{
    [Header("Agent")]
    [SerializeField] float agentSpeed = 3.5f;
    [SerializeField] float agentRadius = 0.5f;
    [SerializeField] float agentHeight = 2f;

    [Header("Random Destination")]
    [SerializeField] float randomSearchRadius = 25f;
    [SerializeField] float destinationReachedDistance = 0.5f;
    [SerializeField] float waitAfterArrival = 2f;

    [Header("Mouse Click")]
    [SerializeField] Camera raycastCamera;
    [SerializeField] LayerMask clickMask = ~0;

    [Header("Debug")]
    [SerializeField] float destinationMarkerSize = 0.5f;
    [SerializeField] Color destinationColor = Color.green;
    [SerializeField] Color pathColor = Color.cyan;

    NavMeshAgent agent;
    Vector3 currentDestination;
    bool waiting;
    bool started = false;

    private void Start()
    {
        

        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        while (TerrainGenerator.Instance?.TerrainReady == false)
            yield return new WaitForSeconds(0.1f);

        if (raycastCamera == null) raycastCamera = Camera.main;

        SpawnAgent();
        SetRandomDestination();
        started = true;
        Camera.main.transform.SetParent(agent.transform);
    }

    private void Update()
    {
        if (!started) return;

        HandleMouseClick();
        DrawDebugVisuals();

        if (!waiting && HasReachedDestination())
            StartCoroutine(WaitThenChooseNewDestination());
    }

    private void SpawnAgent()
    {
        GameObject agentObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        agentObject.name = "NavMeshTestAgent";

        Vector3 spawnPosition = transform.position;

        if (NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, randomSearchRadius, NavMesh.AllAreas))
            spawnPosition = hit.position;

        agentObject.transform.position = spawnPosition;
        agentObject.transform.localScale = new Vector3(1f, 1f, 1f);

        agent = agentObject.AddComponent<NavMeshAgent>();
        agent.speed = agentSpeed;
        agent.radius = agentRadius;
        agent.height = agentHeight;
    }

    private void HandleMouseClick()
    {
        if (!Input.GetMouseButtonDown(0) || raycastCamera == null) return;

        Ray ray = raycastCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, clickMask)) return;

        if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
            SetDestination(navHit.position);
    }

    private void SetRandomDestination()
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = agent.transform.position + Random.insideUnitSphere * randomSearchRadius;
            randomPoint.y = agent.transform.position.y;

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, randomSearchRadius, NavMesh.AllAreas))
            {
                SetDestination(hit.position);
                return;
            }
        }

        Debug.LogWarning("NavMeshTester could not find a random destination on the NavMesh.");
    }

    private void SetDestination(Vector3 destination)
    {
        waiting = false;
        StopAllCoroutines();

        currentDestination = destination;
        agent.SetDestination(currentDestination);
    }

    private bool HasReachedDestination()
    {
        if (agent == null || agent.pathPending)
            return false;

        if (agent.remainingDistance > destinationReachedDistance)
            return false;

        return !agent.hasPath || agent.velocity.sqrMagnitude < 0.01f;
    }

    private IEnumerator WaitThenChooseNewDestination()
    {
        waiting = true;

        yield return new WaitForSeconds(waitAfterArrival);

        SetRandomDestination();
        waiting = false;
    }

    private void DrawDebugVisuals()
    {
        if (agent == null) return;

        Debug.DrawLine(
            currentDestination + Vector3.up * destinationMarkerSize,
            currentDestination - Vector3.up * destinationMarkerSize,
            destinationColor
        );

        Debug.DrawLine(
            currentDestination + Vector3.left * destinationMarkerSize,
            currentDestination + Vector3.right * destinationMarkerSize,
            destinationColor
        );

        Debug.DrawLine(
            currentDestination + Vector3.forward * destinationMarkerSize,
            currentDestination + Vector3.back * destinationMarkerSize,
            destinationColor
        );

        if (agent.path == null || agent.path.corners.Length < 2) return;

        Vector3[] corners = agent.path.corners;

        for (int i = 0; i < corners.Length - 1; i++)
        {
            Debug.DrawLine(
                corners[i] + Vector3.up * 0.1f,
                corners[i + 1] + Vector3.up * 0.1f,
                pathColor
            );
        }
    }
}
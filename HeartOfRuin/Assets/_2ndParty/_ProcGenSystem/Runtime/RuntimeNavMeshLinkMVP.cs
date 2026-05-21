using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class RuntimeNavMeshLinkMVP : MonoBehaviour
{
    [Header("Two points on different NavMesh islands")]
    public Transform startPoint;
    public Transform endPoint;

    [Header("Link settings")]
    public float width = 1.0f;
    public bool bidirectional = true;
    public int area = 0; // 0 = Walkable by default

    private NavMeshLink link;
    void Start()
    {
        CreateLink();
    }

    public void CreateLink()
    {
        if (link != null) Destroy(link.gameObject);

        GameObject linkObject = new GameObject("Runtime NavMesh Link");
        linkObject.transform.position = startPoint.position;

        link = linkObject.AddComponent<NavMeshLink>();

        // NavMeshLink uses local offsets relative to the link object's transform.
        link.startPoint = Vector3.zero;
        link.endPoint = endPoint.position - startPoint.position;

        link.width = width;
        link.bidirectional = bidirectional;
        link.area = area;

        // Use default agent type.
        link.agentTypeID = 0;
    }
}
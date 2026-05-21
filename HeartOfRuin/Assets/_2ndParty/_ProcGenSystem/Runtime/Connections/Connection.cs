using Codice.Client.BaseCommands;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
namespace BMD.ProcGen
{

    public class Connection : MonoBehaviour
    {
        const float NAVMESHLINK_WIDTH = 1f;
        const float NAVMESHLINK_LENGTH = 2f;
        #region Configuration
        [SerializeField] ConnectionDirection direction = ConnectionDirection.Auto;
        [SerializeField] GameObject editorVisualisation;
        [SerializeField] bool keepVisualisationOnPlay = false;
        #endregion

        #region Cached References
        Connection linked;
        Breadcrumbs breadcrumbs;
        Node parent;
        #endregion

        #region Runtime Variables
        Vector3 parentOffset;
        string originalName;
        ConnectionDirection defaultDirection;
        (Connection, Connection) lastTestedConnections;
        bool navmeshLinked = false;
        #endregion

        #region Properties
        public ConnectionDirection Direction => direction;
        public ConnectionDirection DefaultDirection => defaultDirection;
        #endregion

        private void Awake()
        {
            if (parent == null)
            {
                Debug.LogError($"{name}: Connection has not been initialised with a parent node.");
                return;
            }

            RemoveEditorVisualisation();
            FindBreadcrumbs();
            GetParentOffset();
            SetDirection();
            defaultDirection = direction;
            originalName = name;
            SetName();
        }
        void FindBreadcrumbs()
        {
            breadcrumbs = GetComponentInChildren<Breadcrumbs>();
        }
        public void RemoveBreadcrumbs()
        {
            if (!breadcrumbs) return;

            // If breadcrumbs parenting has been changed we keep them
            if (breadcrumbs.transform.parent != transform) return;

            Destroy(breadcrumbs.gameObject);
        }
        public void KeepBreadcrumbs()
        {
            if (!breadcrumbs) return;

            // Attach to the connectors parent
            breadcrumbs.transform.parent = transform.parent;

            if (transform.parent.TryGetComponent<Node>(out Node node))
            {
                node.AddBreadcrumbs(breadcrumbs);
            }
        }
        void RemoveEditorVisualisation()
        {
            if (keepVisualisationOnPlay) return;

            if (!editorVisualisation) return;

            Destroy(editorVisualisation.gameObject);
        }
        void SetName()
        {
            name = $"{direction.ToString()}_{originalName}";
        }
        private void GetParentOffset()
        {
            parentOffset = transform.position - parent.transform.position;
            //Debug.Log($"{name}: Connection initialised with parent {parent.name} and offset {parentOffset}");
        }
        public void RotateConnection(bool reverse)
        {
            GetParentOffset();
            direction = (direction, reverse) switch
            {
                (ConnectionDirection.North, false) => ConnectionDirection.East,
                (ConnectionDirection.East,  false) => ConnectionDirection.South,
                (ConnectionDirection.South, false) => ConnectionDirection.West,
                (ConnectionDirection.West,  false) => ConnectionDirection.North,
                (ConnectionDirection.North, true)  => ConnectionDirection.East,
                (ConnectionDirection.East,  true)  => ConnectionDirection.South,
                (ConnectionDirection.South, true)  => ConnectionDirection.West,
                (ConnectionDirection.West,  true)  => ConnectionDirection.North,
                _ => direction, // Fallback to no change
            };
            SetName();
        }
        public void ResetConnectionRotation()
        {
            direction = defaultDirection;
        }
        public void FullReset()
        {
            ResetConnectionRotation();
            if (breadcrumbs) breadcrumbs.transform.parent = transform;

            if (lastTestedConnections.Item2 == null)
            {
                linked = null;
                lastTestedConnections = new();
            }
        }
        private void SetDirection()
        {
            if (direction != ConnectionDirection.Auto) return;

            Vector3 dir = parentOffset.normalized;
            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.z))
            {
                direction = dir.x > 0 ? ConnectionDirection.North : ConnectionDirection.South;
            }
            else
            {
                direction = dir.z > 0 ? ConnectionDirection.West : ConnectionDirection.East;
            }
            //Debug.Log($"{name}: Auto-set connection direction to {direction}");
        }
        public void Initialise(Node parent)
        {
            this.parent = parent;
        }
        /// <summary>
        /// Moves objects without creating links. Should usually be followed up with Link or CompleteTestLinks
        /// </summary>
        /// <param name="conA"></param>
        /// <param name="conB"></param>
        public static void TestLink(Connection conA, Connection conB)
        {
            Vector3 parentBNewPos = conA.transform.position - conB.parentOffset;

            conB.parent.transform.position = parentBNewPos;
            
            conA.lastTestedConnections = (conA, conB);            
        }
        public static bool CompleteTestLinks(List<Connection> list)
        {
            bool success = false;
            foreach (Connection con in list)
            {
                if (con.lastTestedConnections.Item1 == null) continue;

                // This is to check that at least one link has been made
                success = success || Link(con.lastTestedConnections.Item1, con.lastTestedConnections.Item2);
            }

            return success;
        }
        public static bool Link(Connection conA, Connection conB)
        {
            if(conA == null || conB == null)
            {
                string conAMsg = 
                    conA == null 
                    ? "null" 
                    : $"Connection A: {conA.name}, Parent of A: {conA.transform.parent.name}\n";
                string conBMsg = 
                    conB == null
                    ? "null"
                    : $"Connection B: {conB.name}, Parent of B: {conB.transform.parent.name}\n";
                
                Debug.LogError($"Attemnpting to link connections where one or more is null: \n" +
                    $"{conAMsg}" +
                    $"{conBMsg}");
                return false;
            }

            if (conA.linked != null || conB.linked != null)
            {
                Debug.LogError($"Cannot link {conA.name} and {conB.name} because one of them is already linked.");
                return false;
            }
            conA.linked = conB;
            conB.linked = conA;

            conA.KeepBreadcrumbs();
            conB.KeepBreadcrumbs();

            //Debug.Log($"Linked {conA.name} ({conA.direction}) to {conB.name} ({conB.direction})");

            Vector3 parentBNewPos = conA.transform.position - conB.parentOffset;

            conB.parent.transform.position = parentBNewPos;

            return true;
        }
        public static bool LinkNavmesh(Connection conA)
        {
            if (conA == null || conA.linked == null) return false;

            // If already linked we assume the navmesh is also linked, this is to avoid doing expensive navmesh checks multiple times for the same connection during generation
            if (conA.navmeshLinked || conA.linked.navmeshLinked) return true;

            Transform t = conA.transform;
            Transform tParent = t.parent;

            Vector3 selfDirection = t.position - tParent.position;
            selfDirection.y = 0; // Ignore vertical component for direction
            selfDirection.Normalize();

            Vector3 linkedDirection = conA.linked.transform.position - conA.linked.transform.parent.position;
            linkedDirection.y = 0; // Ignore vertical component for direction
            linkedDirection.Normalize();

            Vector3 rawStartWorld = t.position - selfDirection * (NAVMESHLINK_LENGTH / 2f);
            Vector3 rawEndWorld = t.position - linkedDirection * (NAVMESHLINK_LENGTH / 2f);

            if (!TrySnapToNavMesh(rawStartWorld, out Vector3 startWorld)) return false;

            if (!TrySnapToNavMesh(rawEndWorld, out Vector3 endWorld)) return false;

            NavMeshLink link = conA.gameObject.AddComponent<NavMeshLink>();

            link.startPoint = t.InverseTransformPoint(startWorld);
            link.endPoint = t.InverseTransformPoint(endWorld);

            link.width = NAVMESHLINK_WIDTH;
            link.bidirectional = true;
            link.area = 0;      // Walkable by default

            conA.navmeshLinked = true;
            conA.linked.navmeshLinked = true;

            return true;
        }

        private static bool TrySnapToNavMesh(Vector3 worldPoint, out Vector3 snappedPoint)
        {
            const float maxSnapDistance = 10f;

            if (NavMesh.SamplePosition(
                worldPoint,
                out NavMeshHit hit,
                maxSnapDistance,
                NavMesh.AllAreas))
            {
                snappedPoint = hit.position;
                return true;
            }

            snappedPoint = worldPoint;
            return false;
        }
    }
}
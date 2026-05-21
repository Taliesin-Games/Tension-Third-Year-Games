using System.Collections.Generic;
using UnityEngine;
namespace BMD.ProcGen
{

    public class Connection : MonoBehaviour
    {
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
    }
}
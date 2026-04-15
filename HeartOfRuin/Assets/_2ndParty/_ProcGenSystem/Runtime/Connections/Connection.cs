using UnityEngine;
namespace BMD.ProcGen
{

    public class Connection : MonoBehaviour
    {
        #region Configuration
        [SerializeField] ConnectionDirection direction = ConnectionDirection.Auto;
        #endregion

        #region Cached References
        Connection linked;
        Node parent;
        #endregion

        #region Runtime Variables
        Vector3 parentOffset;
        string originalName;
        #endregion

        #region Properties
        public ConnectionDirection Direction => direction;
        #endregion

        private void Awake()
        {
            if (parent == null)
            {
                Debug.LogError($"{name}: Connection has not been initialised with a parent node.");
                return;
            }

            GetParentOffset();

            SetDirection();

            originalName = name;
            SetName();
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
        public static void Link(Connection conA, Connection conB)
        {
            if (conA.linked != null || conB.linked != null)
            {
                Debug.LogError($"Cannot link {conA.name} and {conB.name} because one of them is already linked.");
                return;
            }
            conA.linked = conB;
            conB.linked = conA;
            Debug.Log($"Linked {conA.name} ({conA.direction}) to {conB.name} ({conB.direction})");

            Vector3 parentBNewPos = conA.transform.position - conB.parentOffset;

            conB.parent.transform.position = parentBNewPos;
        }
        
    }
}
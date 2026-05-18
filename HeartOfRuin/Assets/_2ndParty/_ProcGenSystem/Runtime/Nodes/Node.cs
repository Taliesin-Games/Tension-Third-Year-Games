using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace BMD.ProcGen
{
    public class Node : MonoBehaviour
    {
        const string UnboundedTerrainTag = "UnboundedTerrain";
        static List<Type> skippedTypes = new()
        {
            typeof(BMD.ProcGen.Connection),
            typeof(ParticleSystem),
            typeof(Breadcrumbs)
        };

        #region Configuration
        [SerializeField] GameObject editorVisualisation;
        [SerializeField] bool keepVisualisationOnPlay = false;
        [Tooltip("This will be used to fit terrain together.\n" +
            "If left empty it will be detected from geometry.")]
        [SerializeField] BoxCollider terrainBoundsCollider;
        [Tooltip("In case of flat area how much should we expand the bounding box upwards.\n" +
            "This is not used if there is a collider assigned")]
        [SerializeField] Vector3 headroom = new(0, 1, 0);
        [Tooltip("Amount to increase the terrain bounds by to force free space around the object.\n" +
            "This will not affect connector positioning, only overlapping geometry from other rooms/nodes.")]
        [SerializeField] Vector3 margins = new(0.2f, 0.2f, 0.2f);
        [SerializeField] NodeRotationOptions[] validRotations = new NodeRotationOptions[4] 
        {
            NodeRotationOptions.Deg0,
            NodeRotationOptions.Deg90,
            NodeRotationOptions.Deg180,
            NodeRotationOptions.Deg270
        };
        #endregion

        #region References
        List<Connection> connections = new();
        List<Breadcrumbs> breadcrumbs = new();
        #endregion
        #region Properties and acessor methods
        public List<Connection> Connections => connections;
        public List<Connection> NorthConnections => connections.Where(c => c.Direction == ConnectionDirection.North).ToList();
        public List<Connection> SouthConnections => connections.Where(c => c.Direction == ConnectionDirection.South).ToList();
        public List<Connection> Eastonnections   => connections.Where(c => c.Direction == ConnectionDirection.East).ToList();
        public List<Connection> WestConnections  => connections.Where(c => c.Direction == ConnectionDirection.West).ToList();
        public List<Connection> GetConnectionsByDirection(ConnectionDirection direction)
        {
            return connections.Where(c => c.Direction == direction).ToList();
        }
        public List<NodeRotationOptions> ValidRotations => validRotations.ToList();
        public Bounds Bounds => terrainBounds;
        #endregion

        #region Runtime Variables
        Bounds terrainBounds;
        Quaternion startingRotation;
        #endregion

        public void Awake()
        {
            connections.AddRange(GetComponentsInChildren<Connection>());
            foreach (var connection in connections)
            {
                connection.Initialise(this);
            }

            RemoveEditorVisualisation();
            GetTerrainBounds();
            startingRotation = transform.rotation;
        }
        void RemoveEditorVisualisation()
        {
            if (keepVisualisationOnPlay) return;

            if (!editorVisualisation) return;

            Destroy(editorVisualisation.gameObject);
        }
        void GetTerrainBounds()
        {
            if (terrainBounds == null) terrainBounds = new Bounds();

            if (terrainBoundsCollider == null)
            {
                CalculateTerrainBounds(); 
            }
            else
            {    
                terrainBounds.center = terrainBoundsCollider.center;
                terrainBounds.size = terrainBoundsCollider.size;
            }
                
#if !UNITY_EDITOR
            Destroy(terrainBoundsCollider);
#endif
        }
        void CalculateTerrainBounds()
        {
            terrainBounds = new(); // reset the bounds if calculating.

            // First get a list of all transform children + self
            List<GameObject> allGameObjects = new();
            allGameObjects.AddRange(GetComponentsInChildren<Transform>(true).Select(t => t.gameObject));

            HashSet<GameObject> gameObjectsToRemove = new();

            foreach (GameObject obj in allGameObjects)
            {
                if (!ShouldSkip(obj)) continue;

                gameObjectsToRemove.UnionWith(obj.GetComponentsInChildren<Transform>(true).Select(t => t.gameObject));
            }
            
            allGameObjects.RemoveAll(go => gameObjectsToRemove.Contains(go));

            foreach (var h in allGameObjects)
            {
                if (h.TryGetComponent<Renderer>(out Renderer r)) terrainBounds.Encapsulate(r.bounds);
                if (h.TryGetComponent<Collider>(out Collider c)) terrainBounds.Encapsulate(c.bounds);
            }

            // Allow space above any detected meshes to allow space for the player
            terrainBounds.center += headroom * 0.5f;
            terrainBounds.size += headroom;

            terrainBounds.size += margins;

        }
        public void AddBreadcrumbs(Breadcrumbs breadcrumbs)
        {
            this.breadcrumbs.Add(breadcrumbs);
        }
        public void Clear()
        {
            // Clear any state or references here if needed
            throw new System.NotImplementedException("Clear method is not implemented yet.");
        }
        public void Rotate(bool reverse = false)
        {
            int rotateBy = !reverse ? 90 : -90;
            transform.Rotate(new Vector3(0, rotateBy, 0));

            // Don't forget to update the terrain bounds
            // TODO Cheaper than recalculating but need to implement detection if we add rotations other than 90 degrees
            terrainBounds = RotateBounds90Y(terrainBounds);

            foreach (var connection in connections)
            {
                connection.RotateConnection(reverse);
            }
        }
        public void ResetRotation()
        {
            transform.rotation = startingRotation;

            foreach (var connection in connections)
            {
                connection.ResetConnectionRotation();
            }
        }

        void OnValidate()
        {
            GetTerrainBounds();
        }
        void OnDrawGizmos()
        {
            if (terrainBounds == null) return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(terrainBounds.center + transform.position, terrainBounds.size);
        }
        static bool ShouldSkip(GameObject obj)
        {
            if (obj.CompareTag(UnboundedTerrainTag)) return true;

            if (skippedTypes == null) return false;

            foreach (Type type in skippedTypes)
            {
                if (type == null) continue;

                if (obj.GetComponent(type) != null) return true;
            }

            return false;
            
        }

        Bounds RotateBounds90Y(Bounds bounds)
        {
            Vector3 extents = bounds.extents;
            return new Bounds(bounds.center, new Vector3(extents.z * 2f, extents.y * 2f, extents.x * 2f));
        }
    }

}

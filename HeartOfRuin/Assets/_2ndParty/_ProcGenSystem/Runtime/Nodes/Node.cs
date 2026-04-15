using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BMD.ProcGen
{
    public class Node : MonoBehaviour
    {
        List<Connection> connections = new();

        public List<Connection> Connections => connections;
        public List<Connection> NorthConnections => connections.Where(c => c.Direction == ConnectionDirection.North).ToList();
        public List<Connection> SouthConnections => connections.Where(c => c.Direction == ConnectionDirection.South).ToList();
        public List<Connection> Eastonnections => connections.Where(c => c.Direction == ConnectionDirection.East).ToList();
        public List<Connection> WestConnections => connections.Where(c => c.Direction == ConnectionDirection.West).ToList();
        public List<Connection> GetConnectionsByDirection(ConnectionDirection direction)
        {
            return connections.Where(c => c.Direction == direction).ToList();
        }
        public void Awake()
        {
            connections.AddRange(GetComponentsInChildren<Connection>());
            foreach (var connection in connections)
            {
                connection.Initialise(this);
            }
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

            foreach (var connection in connections)
            {
                connection.RotateConnection(reverse);
            }
        }
    }

}

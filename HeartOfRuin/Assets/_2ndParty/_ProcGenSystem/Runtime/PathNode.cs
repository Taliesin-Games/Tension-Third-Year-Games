using Codice.Client.Common.TreeGrouper;
using System.Collections.Generic;
using UnityEngine;

namespace BMD.ProcGen
{

    public class PathNode
    {
        public Node self;

        public PathNode Parent;                 // optional, useful for backtracking
        public PathNode GoldenChild;            // marks the child which represents the main path
        public List<PathNode> Children = new(); // branch exits from this node

        public void AddChild(PathNode child, bool isGoldenChild = false)
        {
            child.Parent = this;
            Children.Add(child);

            // If this child is the golden child or if there is no golden child yet, set it as the golden child
            if (GoldenChild == null || isGoldenChild) GoldenChild = child;

        }
    }
}
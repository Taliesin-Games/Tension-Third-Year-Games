using System.Collections.Generic;

namespace BMD.ProcGen
{

    public class PathMapNode
    {
        public Node self;
        public string PrefabName;

        public PathMapNode Parent;                 // optional, useful for backtracking
        public PathMapNode GoldenChild;            // marks the child which represents the main path
        public List<PathMapNode> Children = new(); // branch exits from this node

        public void AddChild(PathMapNode child, bool isGoldenChild = false)
        {
            child.Parent = this;
            Children.Add(child);

            // If this child is the golden child or if there is no golden child yet, set it as the golden child
            if (GoldenChild == null || isGoldenChild) GoldenChild = child;

        }
    }
}
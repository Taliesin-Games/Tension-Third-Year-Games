using UnityEngine;

namespace BMD.ProcGen
{
    /// <summary>
    /// Used to bridge the gap between other node types.
    /// </summary>
    public class PathNode : Node
    {
        [Range(0, 10)] 
        [SerializeField] int length = 1;
        [SerializeField] PathNodeDirection direction = PathNodeDirection.Straight;

        public int Length => length;
        public PathNodeDirection Direction => direction;
    }
}
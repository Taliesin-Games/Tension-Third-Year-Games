using System;
using UnityEngine;

namespace BMD.ProcGen
{
    public partial class TerrainGenerator : MonoBehaviour
    {
        private void SanityChecks()
        {
            if (rootPrefabs.Length == 0) Debug.LogError("No root prefabs assigned. The generator needs at least one prefab to create the starting node.");
            if (pathNodePrefabs.Length == 0) Debug.LogError("No path node prefabs assigned. The generator needs at least one prefab to create the paths between rooms.");
            if (roomNodePrefabs.Length == 0) Debug.LogError("No room node prefabs assigned. The generator needs at least one prefab to create the rooms in the level.");
            if (endRoomPrefabs.Length == 0) Debug.LogError("No end room prefabs assigned. The generator needs at least one prefab to create the final room of the main path.");

            if (allowedBranchDirections.Count == 0)
            {
                Debug.LogWarning("No allowed branch directions selected. The generator will not be able to create branches.");
                allowedBranchDirections.AddRange(new[] { ConnectionDirection.North, ConnectionDirection.East, ConnectionDirection.South, ConnectionDirection.West });
            }

            int count = Array.FindAll(pathNodePrefabs, go => {
                PathNode node = go.GetComponent<PathNode>();
                return node != null && node.Length == bridgeLength.Min;
            }).Length;

            if (count == 0) Debug.LogWarning($"No Path Nodes specified with a minimum length that matches bridgeLength.Min:{bridgeLength.Min}. There must be at least one that matches the minimum");
        }
    }
}


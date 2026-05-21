using BMD.DataTypes;
using System.Collections.Generic;
using UnityEngine;

namespace BMD.ProcGen
{
    public partial class TerrainGenerator : MonoBehaviour
    {
        #region Configuration
        [Header("Generation settings"), Tooltip("Length is in number of rooms, not total nodes, connecting paths will be added automatically.")]

        [SerializeField] IntRange roomsOnMainPath = new(5, 10);
        [SerializeField] int branchesPerPath = 2;
        [SerializeField] IntRange roomsOnBranches = new(3, 5);
        [Tooltip("Normal operation this is steps per frame, Update loop. \n" +
            "With slow generation eneabled this is frames per step, FixedUpdate loop.")]
        [SerializeField] int GenerationThrottleAmount = 2;     // Limit how many nodes are generated each frame to avoid performance spikes
        [Tooltip("This slows down generation considerably for debugging purposes")]
        [SerializeField] bool slowGeneration = false;
        [SerializeField] float terrainCamHeight = 30f;
        [SerializeField] bool stepThroughGeneration = false;
        [SerializeField] int randomSeed = 0;                // Seed for random number generation, set to 0 for a random seed based on current time

        [Tooltip("Directions the map can generate in.\n\n If no valid directions are selected, or the selected ones are not available, then any valid connection will be selected.")]
        [SerializeField] List<ConnectionDirection> allowedBranchDirections = new() { ConnectionDirection.North, ConnectionDirection.East, ConnectionDirection.South, ConnectionDirection.West };
        [Tooltip("Directions the generator will prefer when creating connections. This is not a hard requirement, just a bias.\n\n If no valid directions are selected, or the selected ones are not available, then any valid connection will be selected.")]
        [SerializeField] List<ConnectionDirection> directionalBias = new() { ConnectionDirection.North, ConnectionDirection.West };
        [Range(0, 1), Tooltip("Value between 0 and 1 that determines how strong the directional bias is when selecting connections.\n\n 0 means no bias.\n1 means only select from the biased directions")]
        [SerializeField] float directionalBiasStrength = 0.5f; // Value between 0 and 1 that determines how strong the directional bias is when selecting connections. 0 means no bias, 1 means only select from the biased directions
        [SerializeField] IntRange bridgeLength = new(1, 3);
        [Range(0, 1)]
        [SerializeField] float roomMaxOverlap = 0;
        [Range(0, 1), Tooltip("Warning, setting below 0.05 is likely to fail due to floating point rounding errors")]
        [SerializeField] float pathMaxOverlap = 0.1f;
        [Range(0, 1), Tooltip("If generation fails and retries should we add leniency to the overlap for the last attempt.")]
        [SerializeField] float retryLeniency = 0.1f;

        [Header("Node prefabs")]
        [Tooltip("The starting node of the game path. This is where the player will spawn")]
        [SerializeField] GameObject[] rootPrefabs;
        [Tooltip("Path variants used just to exit the root.\n\n If this is empty a normal path piece will be used.")]
        [SerializeField] GameObject[] rootPathPrefabs;
        [Tooltip("Path pieces used to connect each room node")]
        [SerializeField] GameObject[] pathNodePrefabs;
        [Tooltip("Room nodes used to build the level")]
        [SerializeField] GameObject[] roomNodePrefabs;
        [Tooltip("The final node of the main path. This is where the boss will be located")]
        [SerializeField] GameObject[] endRoomPrefabs;
        [Tooltip("Branch end nodes. These are used to end the branches that come out of the main path.\n\n If this is empty a normal end room will be used.")]
        [SerializeField] GameObject[] branchEndPrefabs;
        #endregion

    }
}


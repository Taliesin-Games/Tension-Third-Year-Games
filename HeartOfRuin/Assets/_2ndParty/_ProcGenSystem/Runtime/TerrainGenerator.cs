using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace BMD.ProcGen
{

    public class TerrainGenerator : MonoBehaviour
    {
        public static TerrainGenerator Instance { get; private set; }

        #region Configuration
        [Header("Generation settings"), Tooltip("Length is in number of rooms, not total nodes, connecting paths will be added automatically.")]
        [SerializeField, Min(1)] int minPathLength = 5;
        [SerializeField, Min(2)] int maxPathLength = 10;
        [SerializeField] int branchesPerPath = 2;
        [SerializeField, Min(1)] int minBranchLength = 3;
        [SerializeField, Min(2)] int maxBranchLength = 5;
        [SerializeField] int GenerateNodesPerFrame = 2; // Limit how many nodes are generated each frame to avoid performance spikes
        [SerializeField] int randomSeed = 0; // Seed for random number generation, set to 0 for a random seed based on current time

        [Tooltip("Directions the map can generate in.\n\n If no valid directions are selected, or the selected ones are not available, then any valid connection will be selected.")]
        [SerializeField] List<ConnectionDirection> allowedBranchDirections = new() { ConnectionDirection.North, ConnectionDirection.East, ConnectionDirection.South, ConnectionDirection.West };
        [Tooltip("Directions the generator will prefer when creating connections. This is not a hard requirement, just a bias.\n\n If no valid directions are selected, or the selected ones are not available, then any valid connection will be selected.")]
        [SerializeField] List<ConnectionDirection> directionalBias = new() { ConnectionDirection.North, ConnectionDirection.West };
        [Range(0,1), Tooltip("Value between 0 and 1 that determines how strong the directional bias is when selecting connections.\n\n 0 means no bias.\n1 means only select from the biased directions")]
        [SerializeField] float directionalBiasStrength = 0.5f; // Value between 0 and 1 that determines how strong the directional bias is when selecting connections. 0 means no bias, 1 means only select from the biased directions

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

        #region References
        // Key: (x, y) coordinates of the node, x = branch index, y = depth level in the path
        Dictionary<(int, int), PathNode> generatedNodes = new();
        Dictionary<int, int> branchLengths = new();
        PathNode currentPlayerNode;   // Location of the player.
        PathNode currentBossNode;     // Location of the boss.
        #endregion

        #region Runtime variables
        System.Random rng;
        Coroutine generationCoroutine;
        bool isGenerating = false;
        bool generationComplete = false;
        int generationStepsThisFrame; // Counter to track how many nodes have been generated in the current frame
        #endregion
        #region Properties
        public bool TerrainReady => !isGenerating && generationComplete;
        private bool PauseGeneration {
            get
            {
                generationStepsThisFrame++;
                if(generationStepsThisFrame >= GenerateNodesPerFrame)
                {
                    generationStepsThisFrame = 0; // Reset the counter for the next frame
                    return true; // Pause generation to wait for the next frame
                }
                return false; // Continue generation in the current frame
            } 
        }
        #endregion
        private void Awake()
        {
            CreateInstance();
            SetRandomSeed();
            SanityChecks();
        }
        private void CreateInstance()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

        }
        private void SetRandomSeed()
        {
            if(randomSeed == 0)
            {
                randomSeed = System.Environment.TickCount; // Use current time as seed if 0 is specified
                Debug.Log($"Random seed set to {randomSeed} based on current time.");
            }
            rng = new System.Random();
        }
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
        }
        private void Start()
        {
            generationCoroutine = StartCoroutine(GenerateLevel());
        }
        IEnumerator GenerateLevel()
        {
            if (isGenerating)
            {
                Debug.LogWarning("Terrain generation is already in progress.");
                yield break;
            }

            isGenerating = true;
            
            ClearOldTerrain();


            // Generate the main path
            int mainPathLength = Random.Range(minPathLength, maxPathLength + 1);
            int lengthIncludingConnections = mainPathLength * 2 + 1; // Each room is connected by a path, so total nodes = rooms + paths = 2*rooms + 1
            yield return GenerateMainPath(lengthIncludingConnections);

            //// Generate branches
            //for (int branchIndex = 0; branchIndex < branchesPerPath; branchIndex++)
            //{
            //    int branchLength = Random.Range(minBranchLength, maxBranchLength + 1);
            //    GenerateBranch(branchIndex, branchLength);
            //}

            yield return ConnectPaths();

            isGenerating = false;
            generationComplete = true;
 
        }
        private void ClearOldTerrain()
        {
            foreach (var node in generatedNodes.Values)
            {
                node?.self.Clear();
            }
            generatedNodes.Clear();

            branchLengths.Clear();
        }
        IEnumerator GenerateMainPath(int length)
        {
            // Create the root node
            generatedNodes[(0,0)] = CreateNode(rootPrefabs, null, "0:0");
            branchLengths[0] = length + 2; // Store the length of the main path in the branch lengths dictionary with branch index 0 representing the main path. Add one for start and one for end

            if (PauseGeneration) yield return null; // Wait a frame to allow the root node to initialize before we start adding more nodes

            // Add the first path node right after the root. This ensures we have a clear exit from the starting area.
            generatedNodes[(0, 1)] = CreateNode(rootPathPrefabs.Length > 0 ? rootPathPrefabs : pathNodePrefabs, generatedNodes[(0,0)], "0:1");
            
            for (int i = 2; i <= length; i++)
            {
                // check if i is even or odd to determine if we are placing a path node or a room node
                GameObject[] prefabsToUse = (i % 2 == 0) ? roomNodePrefabs : pathNodePrefabs;
                generatedNodes[(0, i)] = CreateNode(prefabsToUse, generatedNodes[(0, i - 1)], $"0:{i}");
                if(PauseGeneration) yield return null; // Wait a frame after placing each node to allow it to initialize before placing the next one
            }

            // Add the end room at the end of the main path
            generatedNodes[(0, length + 1)] = CreateNode(endRoomPrefabs, generatedNodes[(0, length)], $"0:{length+1}");
            yield return null; // Wait a frame to allow the end room to initialize, always wait on the last node

        }
        PathNode CreateNode(GameObject[] nodes, PathNode parent = null, string prefix = "x:x")
        {
            GameObject prefab = nodes[Random.Range(0, nodes.Length)];
            PathNode pathNode = new PathNode
            {
                self = Instantiate(prefab, transform).GetComponent<Node>()

            };
            pathNode.self.name = $"{prefix}_{pathNode.self.name}";
            parent?.AddChild(pathNode);

            return pathNode;
        }
        IEnumerator ConnectPaths()
        {
            foreach(int key in branchLengths.Keys)
            {
           
                for (int i = 0; i < branchLengths[key] - 1; i++)
                {
                    Debug.Log(i);
                    PathNode currentNode = generatedNodes[(key, i)]; // Get the child nodes of the current node as possible connections
                    PathNode nextNode = generatedNodes[(key,i + 1)];

                    ConnectNodePair(currentNode, nextNode); 

                    if (PauseGeneration) yield return null; // Wait if we have done enough steps this frame to avoid performance spikes
                }
                if (PauseGeneration) yield return null; // Wait if we have done enough steps this frame to avoid performance spikes
            }

            yield return null;
        }
        void ConnectNodePair(PathNode firstNode, PathNode secondNode)
        {
            float biasRoll = (float)rng.NextDouble();
            List<ConnectionDirection> allowedDirections = new List<ConnectionDirection>(allowedBranchDirections);
            List<ConnectionDirection> biasDirections = new List<ConnectionDirection>(directionalBias);

            List<ConnectionDirection> selectedDirectionList;
            ConnectionDirection selectedDirection;
            ConnectionDirection reverseDirection = ConnectionDirection.North;

            while (allowedDirections.Count + biasDirections.Count > 0)
            {
  
                // If either direction list is empty use the other (can never both be empty
                // If both have items then select based on bias strength
                if (biasDirections.Count == 0)         selectedDirectionList = allowedDirections;
                else if (allowedDirections.Count == 0) selectedDirectionList = biasDirections;
                else selectedDirectionList = biasRoll <= directionalBiasStrength ? biasDirections : allowedDirections;

                selectedDirection = selectedDirectionList[rng.Next(selectedDirectionList.Count)];
                reverseDirection = selectedDirection switch
                {
                    ConnectionDirection.South => ConnectionDirection.North,
                    ConnectionDirection.North => ConnectionDirection.South,
                    ConnectionDirection.East => ConnectionDirection.West,
                    _ => ConnectionDirection.East,
                };

                List<Connection> firstNodeConnections = new(firstNode.self.GetConnectionsByDirection(selectedDirection));
                

                // Select a random connection from the list of available connections
                // If there are none remove the direction from the list and repeat selection
                if (firstNodeConnections.Count == 0) continue;
                Connection firstNodeConnection = firstNodeConnections[rng.Next(firstNodeConnections.Count)];
                if (firstNodeConnection == null)
                {
                    selectedDirectionList.Remove(selectedDirection);
                    continue;
                }

                List<Connection> secondNodeConnections = new();
                Connection secondNodeConnection = null;
                int rotationCount = 0;
                while (secondNodeConnection == null && rotationCount < 4)
                {
                    rotationCount++;
                    // Attempt to get second connection from second node.
                    // If we fail we rotate and attempt to get it again.
                    // If we fail 4 times we decide that the connecton is impossible and remove the connection direction
                    secondNodeConnections.Clear();
                    secondNodeConnections = new(secondNode.self.GetConnectionsByDirection(reverseDirection));

                    if (secondNodeConnections.Count > 0) 
                        secondNodeConnection = secondNodeConnections[rng.Next(secondNodeConnections.Count)];
                    if (secondNodeConnection == null)
                    {
                        secondNode.self.Rotate();
                        
                    }
                    else break;
                } 
                if (rotationCount >= 4)
                {
                    selectedDirectionList.Remove(selectedDirection);
                    continue;
                }

                Connection.Link(firstNodeConnection, secondNodeConnection);
                break;
            }


        }
        Connection GetValidConnection(PathNode node, ConnectionDirection direction, List<Connection> connections)
        {
            foreach (Connection connection in connections) 
            {
                if (direction != connection.Direction) connections.Remove(connection);
            }

            if (connections.Count == 0) return null;

            return connections[rng.Next(connections.Count)];
        }
        public void SetPlayerLocation(PathNode node)
        {
            currentPlayerNode = node;
        }
        public void SetBossLocation(PathNode node)
        {
            currentBossNode = node;
        }
    }
}
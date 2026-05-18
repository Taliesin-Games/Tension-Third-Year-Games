using System.Collections;
using UnityEngine;


namespace BMD.ProcGen
{

    public partial class TerrainGenerator : MonoBehaviour
    {
        void Update()
        {
            // For manual step through, sometimes two throttles are called on the same frame This protects against that.
            debugStepDoneThisFrame = false;
        }
        IEnumerator GenerateLevel()
        {
            if (isGenerating)
            {
                Debug.LogWarning("Terrain generation is already in progress.");
                yield break;
            }

            isGenerating = true;
            growthLog = "Starting terrain generation.\n";
            generationStepUIOutput = "Starting Generation";
            yield return slowTextUpdate;
            ClearOldTerrain();

            // Generate the main path
            int numberOfRooms = rng.Next(roomsOnMainPath.Min, roomsOnMainPath.Max + 1);
            generationStepUIOutput = "Generating Main Branch";
            yield return GenerateBranch(numberOfRooms);

            // TODO add generating branches from route
            //// Generate branches
            generationStepUIOutput = "Generating Side Branches";
            yield return slowTextUpdate;

            // TODO add navmesh links
            generationStepUIOutput = "Linking NavMesh";
            yield return slowTextUpdate;

            generationStepUIOutput = "Scattering breadcrumbs";
            yield return LeaveBreadcrumbs();
            yield return slowTextUpdate;

            // TODO trigger boss
            generationStepUIOutput = "Annoying boss...";
            yield return slowTextUpdate;

            isGenerating = false;
            generationComplete = true;
            generationStepUIOutput = "Generation Complete";
            Debug.Log($"Finished terrain generation with {generatedNodes.Count} nodes. Main path length: {branchLengths[0]}");
            Debug.Log($"Growth log:\n{growthLog}"); // Print the growth log after generation is complete

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
        IEnumerator GenerateBranch(int length, PathMapNode growFrom = null)
        {
            // Creates the seed point of the tree if growFrom is null and performs additional validity checks.
            if (!CheckGrowFromIsValid(growFrom)) yield break;

            int totalPathLength = 0;

            if (SetThrottleYield()) yield return Throttle;

            for (int i = 0; i <= length; i++)               // Iterates based on the number of rooms
            {
                GrowthParameters gp = new(i);

                if(i == length) gp.roomType = RoomType.Boss;   // Set the last room to be the boss room

                yield return GrowBud(gp);
                if (!gp.success) Debug.Log("Failed to grow bud for main path, skipping. This may cause issues with future growth.");
                else totalPathLength += gp.growth;

                if (SetThrottleYield()) yield return Throttle;
            }

            branchLengths[0] = totalPathLength + 1;

            if (SetThrottleYield()) yield return Throttle;

        }
        IEnumerator GrowBud(GrowthParameters parameters, int retries = 0)
        {

            parameters.growth = 0;    // Reset growth for this attempt

            // This fails if retries is too high
            if (!TryCreateGrowthAttempt(parameters, retries, out GrowthAttempt attempt)) yield break;
            if (SetThrottleYield()) yield return Throttle;

            yield return TryBuildGrowthSegments(attempt);
            if (!attempt.BuildSucceeded) yield break;
            if (SetThrottleYield()) yield return Throttle;

            // Next we need to lay out the nodes.
            if(!TryConnectGrowth(attempt))
            {
                if (SetThrottleYield()) yield return Throttle;
                CleanupAttempt(attempt);
                yield return GrowBud(parameters, retries + 1);
                yield break;
            }
            if (SetThrottleYield()) yield return Throttle;

            // Next check for any overlapping nodes
            yield return IsGrowthValid(attempt, retries);
            if (!attempt.OverlapsValid)
            {
                if (SetThrottleYield()) yield return Throttle;
                CleanupAttempt(attempt);
                yield return GrowBud(parameters, retries + 1);
                yield break;
            }
            if (SetThrottleYield()) yield return Throttle;

            // Now we have tested the geometry finalise the connection links
            if (!FinaliseConnections(attempt))
            {
                Debug.LogError($"Failed to finalise connections for branch {attempt.BranchID}, source node {attempt.SourceNodeID}");
                yield break;
            }
            if (SetThrottleYield()) yield return Throttle;

            yield return FinaliseGrowth(attempt);            
            if (SetThrottleYield(true)) yield return Throttle;


            parameters.growth = attempt.TotalGrowth;    // Update growth with the total growth from the attempt.
            parameters.success = true;

            growthLog += $"####\n" +
                $"Finished growth attempt from {attempt.BranchID}:{attempt.SourceNodeID}:\n" +
                $"{attempt.GenerationLog}\n";

            if (SetThrottleYield()) yield return Throttle;
        }
        bool CheckGrowFromIsValid(PathMapNode growFrom)
        {
            if (growFrom == null && generatedNodes.Count > 0)
            {
                Debug.LogError($"A terrain origin has already been seeded, you  must specify a node to growFrom. Randomly selecting a node to grow from is not yet supported.");
                return false;
            }

            if (growFrom == null) growFrom = SeedOriginPoint();
            
            if (growFrom != null)
            {
                // Check if growFrom has type "Node"
                if (growFrom.self is not RoomNode)
                {
                    Debug.Log($"growFrom specified but contained object of {growFrom.self.name} is not a RoomNode. Growing from other node types is not yet supported.");
                    return false;
                }
            }

            return true;
        }
        PathMapNode SeedOriginPoint()
        {
            GameObject prefab = rootPrefabs[rng.Next(0, rootPrefabs.Length)];
            PathMapNode pathNode = new PathMapNode
            {
                self = Instantiate(prefab, transform).GetComponent<Node>(),
                PrefabName = prefab.gameObject.name

            };

            pathNode.self.name = $"0:0:0_{pathNode.self.name}";

            // Create the root node
            generatedNodes[new(0, 0)] = pathNode;
            branchLengths[0] = 1;   // Store the length of the main path in the branch lengths dictionary with branch index 0 representing the main path. Add one for start and one for end

            return generatedNodes[new(0, 0)];
        }
        bool TryCreateTestConnection(PathMapNode firstNode, PathMapNode secondNode)
        {
            // These are preallocated and cleared.
            // This is faster since we create and remove these many times during terrain generation
            allowedDirections.Clear();
            biasDirections.Clear();

            allowedDirections.AddRange(allowedBranchDirections);
            biasDirections.AddRange(directionalBias);

            ConnectionDirection selectedDirection;
            ConnectionDirection reverseDirection;

            while (allowedDirections.Count + biasDirections.Count > 0)
            {
                selectedDirectionList = SelectDirectionPool();

                selectedDirection = selectedDirectionList[rng.Next(selectedDirectionList.Count)];
                reverseDirection = Reverse(selectedDirection);

                Connection firstConnection = GetRandomConnection(firstNode.self, selectedDirection);

                if (firstConnection == null)
                {
                    selectedDirectionList.Remove(selectedDirection);
                    continue;
                }

                Connection secondConnection = FindConnectionWithRotation(secondNode.self, reverseDirection);

                if (secondConnection == null)
                {
                    selectedDirectionList.Remove(selectedDirection);
                    continue;
                }

                Connection.TestLink(firstConnection, secondConnection);
                
                return true;
            }

            return false;
        }
        public void SetPlayerLocation(PathMapNode node)
        {
            currentPlayerNode = node;
        }
        public void SetBossLocation(PathMapNode node)
        {
            currentBossNode = node;
        }
        
        
    }
}
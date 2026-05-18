using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static PlasticGui.GetProcessName;

namespace BMD.ProcGen
{
    public partial class TerrainGenerator : MonoBehaviour
    {
        bool TryCreateGrowthAttempt(GrowthParameters parameters, int retries, out GrowthAttempt attempt)
        {
            if (retries > MAX_GROWTH_RETRIES)
            {
                Debug.LogError($"GrowBud failing repeatedly, please check settings. \n" +
                    $"SourceNodeID: {parameters.sourceNodeID}, " +
                    $"BranchID: {parameters.branchID}, " +
                    $"retries: {retries}");
                attempt = null;
                return false;
            }

            if (!TryGetLastNodeOnBranch(parameters.branchID, out PathMapNode sourceNode))
            {
                Debug.LogError($"Unable to find last node on branch. \n" +
                    $"SourceNodeID: {parameters.sourceNodeID}, " +
                    $"BranchID: {parameters.branchID}");
                attempt = null;
                return false;
            }

            attempt = new() { Parameters = parameters };

            attempt.SourceNode = sourceNode;

            // Create a room, and initialise it.
            GameObject roomPrefab;

            switch (attempt.RoomType)
            {
                case RoomType.BranchEnd:
                    roomPrefab = branchEndPrefabs[rng.Next(0, branchEndPrefabs.Length)];
                    break;
                case RoomType.Boss:
                    roomPrefab = endRoomPrefabs.Length > 0 ?
                        endRoomPrefabs[rng.Next(0, endRoomPrefabs.Length)] :
                        roomNodePrefabs[rng.Next(0, roomNodePrefabs.Length)];
                    break;
                case RoomType.Standard:
                default:
                    roomPrefab = roomNodePrefabs[rng.Next(0, roomNodePrefabs.Length)];
                    break;
            }

            PathMapNode newBud = new PathMapNode
            {
                self = Instantiate(roomPrefab, transform).GetComponent<Node>(),
                PrefabName = roomPrefab.name
            };

            if (newBud.self.TryGetComponent(out Node node)) SetRandomNodeRotation(node);

            newBud.self.name = $"X:X:X_{newBud.PrefabName}";

            attempt.NewBud = newBud;

            attempt.TargetLength = rng.Next(bridgeLength.Min, bridgeLength.Max + 1) + retries + (attempt.NewBud == null ? 0 : 1);      // We add the retries with the assumption that extra space will create a higher success chance
            attempt.GenerationLog = $"Growth attempt created. \n" +
                $"TargetLength: {attempt.TargetLength}, " +
                $"RoomType: {attempt.RoomType}, " +
                $"NewBudPrefab: {attempt.NewBud.PrefabName}\n";

            return true;
        }

        bool TryGetLastNodeOnBranch(int branchID, out PathMapNode sourceNode)
        {
            // Gets the most recent node on the branch with the given ID

            sourceNode = null;
            int highestDepth = int.MinValue;

            foreach (var kvp in generatedNodes)
            {
                if (kvp.Key.Branch != branchID) continue;

                if (kvp.Key.Depth > highestDepth)
                {
                    highestDepth = kvp.Key.Depth;
                    sourceNode = kvp.Value;
                }
            }

            return sourceNode != null;
        }
        bool TrySelectPathPrefab(GrowthAttempt attempt, out GameObject selectedPrefab)
        {
            validPathPrefabs.Clear();

            GameObject[] prefabPool = ShouldUseRootPathPrefab(attempt.Segments)
                    ? rootPathPrefabs
                    : pathNodePrefabs;

            foreach (GameObject prefab in prefabPool)
            {
                if (prefab.TryGetComponent(out PathNode pathNode) && pathNode.Length <= attempt.RemainingGrowth)
                {
                    validPathPrefabs.Add(prefab);
                }
            }

            if (validPathPrefabs.Count == 0)
            {
                selectedPrefab = null;
                attempt.GenerationLog += $"!##! Failed to find a valid path prefab\n.";
                return false;
            }

            selectedPrefab = validPathPrefabs[rng.Next(validPathPrefabs.Count)];
            attempt.GenerationLog += $"Path prefab selected succesfully: {selectedPrefab.name}\n";
            return true;
        }

        IEnumerator TryBuildGrowthSegments(GrowthAttempt attempt)
        {
            int loopCounter = 0;

            while (!attempt.GrowthComplete && loopCounter++ < LOOP_PROTECTION_LIMIT)
            {

                GameObject[] prefabPool = ShouldUseRootPathPrefab(attempt.Segments)
                    ? rootPathPrefabs
                    : pathNodePrefabs;

                if (!TrySelectPathPrefab(attempt, out GameObject segmentPrefab))
                {
                    if (SetThrottleYield()) yield return Throttle;
                    CleanupAttempt(attempt);
                    Debug.LogError($"Branch grow loop exited after failing to create segments \n" +
                        $"SourceNodeID: {attempt.Parameters.sourceNodeID}, " +
                        $"BranchID: {attempt.Parameters.branchID}, ");
                    yield break;
                }

                PathMapNode segment = new PathMapNode
                {
                    self = Instantiate(segmentPrefab, transform).GetComponent<Node>(),
                    PrefabName = segmentPrefab.gameObject.name
                };

                if (segment.self.TryGetComponent(out Node node)) SetRandomNodeRotation(node); 

                segment.self.name = $"X:X:X_{segment.PrefabName}";
                attempt.Segments.Add(segment);

                if (SetThrottleYield()) yield return Throttle;
            }
            // Now we have generated the new bud and growth segments move the bud to the bottom in hierarchy to give a consistent order
            attempt.NewBud.self.transform.SetAsLastSibling();

            if (loopCounter >= LOOP_PROTECTION_LIMIT)
            {
                attempt.GenerationLog += $"!##! Loop protection limit reached while building growth segments. Segment count: {attempt.Segments.Count}\n";
                Debug.LogError($"Loop protection limit reached, Branch grow loop exited after failing to create segments \n" +
                    $"SourceNodeID: {attempt.Parameters.sourceNodeID}, " +
                    $"BranchID: {attempt.Parameters.branchID}, ");
                yield break;
            }
            attempt.BuildSucceeded = true;
            attempt.GenerationLog += $"Growth segments built. Segment count: {attempt.Segments.Count}\n";
        }

        bool TryConnectGrowth(GrowthAttempt attempt)
        {
            // If no growth segments connect directly
            if (attempt.Segments.Count == 0)
            {
                if (!TryCreateTestConnection(attempt.SourceNode, attempt.NewBud))
                {
                    attempt.GenerationLog += $"!##! Failed to connect source node directly to new bud.\n";
                    return false;
                }
                attempt.GenerationLog += $"Source node connected directly to new bud.\n";
                return true;    // Direct connection successful
            }

            // Else
            // Connect source node with first growth node
            bool success = TryCreateTestConnection(attempt.SourceNode, attempt.Segments[0]);

            // Connect each growth node to each other, stop at count - 1 as the last segment will be connected to the new bud
            for (int i = 0; i < attempt.Segments.Count - 1; i++)
            {
                success = success && TryCreateTestConnection(attempt.Segments[i], attempt.Segments[i + 1]);
            }

            // Connect last growth node with the new bud
            success = success && TryCreateTestConnection(attempt.Segments.Last(), attempt.NewBud);
            if (!success)
            {
                attempt.GenerationLog += $"!##! Failed to connect growth segments together.\n";
                return false;
            }

            attempt.GenerationLog += $"Growth segments connected.\n";
            return true;
        }

        IEnumerator IsGrowthValid(GrowthAttempt attempt, int retries)
        {
            
            // Check for room overlap 
            float allowedRoomOverlap = roomMaxOverlap;
            float allowedPathOverlap = pathMaxOverlap;
            if (retries >= MAX_GROWTH_RETRIES - 1)
            {
                allowedRoomOverlap += retryLeniency;
                allowedPathOverlap += retryLeniency;
            }

            float largestOverlap = 0;
            float overlap = GetBoundsOverlap(attempt.SourceNode.self, attempt.NewBud.self);
            if (overlap > allowedRoomOverlap) 
            { 
                if (attempt.SourceNode.self.TryGetComponent<Node>(out Node node)) node.Connections.ForEach(c => c.FullReset());
                attempt.GenerationLog += $"!##! Failed room overlap check between source node and new bud. Overlap: {overlap}, Allowed: {allowedRoomOverlap}\n";
                Debug.LogWarning($"Overlapping rooms detected but no handler");
                yield break;
            }
            largestOverlap = overlap;


            // Now check if paths overlap, both with each other and the rooms.
            if (attempt.Segments.Count > 0)
            {
                

                // Check first room and first path
                overlap = GetBoundsOverlap(attempt.SourceNode.self, attempt.Segments[0].self);
                largestOverlap = overlap > largestOverlap ? overlap : largestOverlap;

                if (overlap > pathMaxOverlap)
                {
                    attempt.GenerationLog += $"!##! Failed path overlap check between source node and first segment. Overlap: {overlap}, Allowed: {pathMaxOverlap}\n";
                    Debug.LogWarning($"Overlapping paths detected but no handler");
                }

                // Check each path against the next
                for (int i = 0; i < attempt.Segments.Count - 1; i++)
                {
                    overlap = GetBoundsOverlap(attempt.Segments[i].self, attempt.Segments[i + 1].self);
                    largestOverlap = overlap > largestOverlap ? overlap : largestOverlap;

                    if (overlap > pathMaxOverlap) 
                    {
                        attempt.GenerationLog += $"!##! Failed path overlap check between segments {i} and {i + 1}. Overlap: {overlap}, Allowed: {pathMaxOverlap}\n";
                        Debug.LogWarning($"Overlapping paths detected but no handler");
                    } 

                    if (SetThrottleYield()) yield return Throttle;
                }

                // Connect last path vs new room
                overlap = GetBoundsOverlap(attempt.Segments.Last().self, attempt.NewBud.self);
                largestOverlap = overlap > largestOverlap ? overlap : largestOverlap;

                if (overlap > pathMaxOverlap)
                {
                    attempt.GenerationLog += $"!##! Failed path overlap check between last segment and new bud. Overlap: {overlap}, Allowed: {pathMaxOverlap}\n";
                    Debug.LogWarning($"Overlapping paths detected but no handler");
                }

            }

            // If we have reached this point then the growth attempt is valid.
            attempt.OverlapsValid = true;
            attempt.GenerationLog += $"Growth overlaps valid. LargestOverlap: {largestOverlap}, Allowed: {roomMaxOverlap}/{pathMaxOverlap}\n";
        }

        bool FinaliseConnections(GrowthAttempt attempt)
        {
            // Connect source node with first growth node, or new bud if no growth nodes
            if (!Connection.CompleteTestLinks(attempt.SourceNode.self.Connections)) return false;

            // Connect each growth node to each other
            for (int i = 0; i < attempt.Segments.Count; i++)   
            {
                PathMapNode segment = attempt.Segments[i];
                if (!Connection.CompleteTestLinks(segment.self.Connections))
                {
                    attempt.GenerationLog += $"!##! Failed to complete test links for segment {i}\n" +
                        $"Branch: {attempt.BranchID}, Source Node: {attempt.SourceNodeID}\n";
                    return false;
                }
                segment.self.name = $"{attempt.BranchID}:{attempt.SourceNodeID + 1}:{i}_{segment.PrefabName}";

            }

            attempt.NewBud.self.name = $"{attempt.BranchID}:{attempt.SourceNodeID + 1}:{attempt.Segments.Count}" + $"_{attempt.NewBud.PrefabName}";

            attempt.GenerationLog += $"Connections finalised - Starting at: {attempt.SourceNode.self.name} Ending at: {attempt.NewBud.self.name}\n";
            return true;
        }

     
        IEnumerator FinaliseGrowth(GrowthAttempt attempt)
        {
            // Make parent/child links to show growth order.
            SetupNodeParentChildLinks(attempt);
            if (SetThrottleYield()) yield return Throttle;

            // Finally add to the generated nodes path
            // Find the key first. This is more stable than tracking the index with random lengths and possible retries. But more expensive
            NodeAddress foundKey = new(0, 0);

            foreach (var kvp in generatedNodes)
            {
                if (kvp.Value == attempt.SourceNode)
                {
                    foundKey = kvp.Key;
                    break;
                }
                if (SetThrottleYield(true)) yield return Throttle;
            }

            int branchIndex = foundKey.Branch;
            int pathDepthIndex = foundKey.Depth;
            int nextNodeIndex = pathDepthIndex + 1;

            // Now loop through growth segments adding to generated nodes.
            for (int i = 0; i < attempt.Segments.Count; i++)   // Dont include last member
            {
                generatedNodes[new(branchIndex, nextNodeIndex)] = attempt.Segments[i];
                nextNodeIndex++;

                if (SetThrottleYield(true)) yield return Throttle;
            }
            generatedNodes[new(branchIndex, nextNodeIndex)] = attempt.NewBud;  // Add the new bud last
            attempt.GenerationLog += $"Growth finalised. Total nodes added: {attempt.Segments.Count + 1}\n";
        }

        private static void SetupNodeParentChildLinks(GrowthAttempt attempt)
        {
            // Source node first
            if (attempt.Segments.Count > 0) attempt.SourceNode.AddChild(attempt.Segments.First());
            else attempt.SourceNode.AddChild(attempt.NewBud);       // Add new bud directly to source if no growth segments

            // Next each segment in order
            for (int i = 0; i < attempt.Segments.Count - 1; i++)   // Dont include last member
            {
                attempt.Segments[i].AddChild(attempt.Segments[i + 1]);
            }

            if (attempt.Segments.Count > 0) attempt.Segments.Last().AddChild(attempt.NewBud);
        }

        IEnumerator LeaveBreadcrumbs()
        {
            foreach (PathMapNode node in generatedNodes.Values)
            {
                if (node == null) continue;
                node.self.Connections.ForEach(c => c.RemoveBreadcrumbs());

                if (SetThrottleYield()) yield return Throttle;
            }
        }
    }
}

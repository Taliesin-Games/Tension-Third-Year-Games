using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BMD.ProcGen
{

    public partial class TerrainGenerator : MonoBehaviour
    {
        #region Helper Methods
        AudioClip CreateDebugBeep()
        {
            int sampleRate = 44100;
            float duration = 0.05f;
            int samples = Mathf.CeilToInt(sampleRate * duration);

            float[] data = new float[samples];

            float frequency = 880f;

            for (int i = 0; i < samples; i++)
            {
                data[i] = Mathf.Sin(2 * Mathf.PI * frequency * i / sampleRate) * 0.25f;
            }

            AudioClip clip = AudioClip.Create("DebugBeep", samples, 1, sampleRate, false);
            clip.SetData(data, 0);

            return clip;
        }
        List<ConnectionDirection> SelectDirectionPool()
        {
            // No need to check if BOTH are empty, this is done in a previous step
            if (biasDirections.Count == 0) return allowedDirections;
            if (allowedDirections.Count == 0) return biasDirections;

            return rng.NextDouble() <= directionalBiasStrength ? biasDirections : allowedDirections;
        }
        ConnectionDirection Reverse(ConnectionDirection direction)
        {
            return direction switch
            {
                ConnectionDirection.North => ConnectionDirection.South,
                ConnectionDirection.South => ConnectionDirection.North,
                ConnectionDirection.East => ConnectionDirection.West,
                ConnectionDirection.West => ConnectionDirection.East,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
        Connection GetRandomConnection(Node node, ConnectionDirection direction)
        {
            selectedConnections.Clear();
            selectedConnections.AddRange(node.GetConnectionsByDirection(direction));

            return selectedConnections.Count == 0
                ? null
                : selectedConnections[rng.Next(selectedConnections.Count)];
        }
        Connection FindConnectionWithRotation(Node node, ConnectionDirection direction)
        {
            for (int i = 0; i < 4; i++)
            {
                // TODO check if rotation is valid and skip if not
                Connection connection = GetRandomConnection(node, direction);

                // End when we have found a valid rotation
                if (connection != null) return connection;

                node.Rotate();
            }

            node.ResetRotation();

            return null;
        }
        float GetBoundsOverlap(Node nodeA, Node nodeB)
        {

            Bounds boundsA = new Bounds(
                nodeA.transform.TransformPoint(nodeA.Bounds.center),    // Convert local coordinates for centre to global coordinates
                nodeA.Bounds.size                                       // Size remains the same
                );
            Bounds boundsB = new Bounds(
                nodeB.transform.TransformPoint(nodeB.Bounds.center),    // Convert local coordinates for centre to global coordinates
                nodeB.Bounds.size                                       // Size remains the same
                );

            float overlapPercent = 0f;

            // If either box is fully inside the other return overlap of 1
            bool aInB = boundsB.Contains(boundsA.min) && boundsB.Contains(boundsA.max);
            bool bInA = boundsA.Contains(boundsB.min) && boundsA.Contains(boundsB.max);
            if (aInB || bInA) return 1.0f;


            if (boundsA.Intersects(boundsB))
            {
                Vector3 min = Vector3.Max(boundsA.min, boundsB.min);
                Vector3 max = Vector3.Min(boundsA.max, boundsB.max);

                Vector3 size = max - min;

                float intersectionVolume = size.x * size.y * size.z;
                float volumeA = boundsA.size.x * boundsA.size.y * boundsA.size.z;

                overlapPercent = intersectionVolume / volumeA; // 0–1 range
            }

            return overlapPercent;
        }
        bool ShouldUseRootPathPrefab(List<PathMapNode> growthSegments)
        {
            return growthSegments.Count == 0 && generatedNodes.Count == 1 && rootPathPrefabs.Length > 0;
        }
        void CleanupAttempt(GrowthAttempt attempt)
        {
            if (attempt == null) return;

            Debug.Log($"Cleaning up attempt, generation log follows.\n{attempt.GenerationLog}");
            
            // Reset soruce node connections
            // No need to reset others as they are destroyed.
            if (attempt.SourceNode.self.TryGetComponent<Node>(out Node node)) 
                node.Connections.ForEach(c => c.FullReset());


            foreach (PathMapNode segment in attempt.Segments)
            {
                if (segment?.self != null) Destroy(segment.self.gameObject);
            }

            if (attempt.NewBud?.self != null) Destroy(attempt.NewBud.self.gameObject);

            attempt.Segments.Clear();
        }
        #endregion
        void SetRandomNodeRotation(Node node)
        {
            int x = rng.Next(0, 5);
            for (int i = 0; i < x; i++)
            {
                node.Rotate();
            }
        }
    }
}


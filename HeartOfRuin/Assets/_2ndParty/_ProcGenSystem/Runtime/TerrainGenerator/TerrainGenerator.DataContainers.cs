using System.Collections.Generic;
using UnityEngine;

namespace BMD.ProcGen
{
    public partial class TerrainGenerator : MonoBehaviour
    {
        enum RoomType
        {
            Standard,
            Boss,
            BranchEnd
        }
        class GrowthParameters
        {
            public int sourceNodeID;
            public int branchID = 0;
            public RoomType roomType = RoomType.Standard;
            public int growth = 0;
            public bool success = false;

            public GrowthParameters(int sourceNodeID) : this(sourceNodeID, 0, RoomType.Standard) { }
            public GrowthParameters(int sourceNodeID, RoomType roomType) : this(sourceNodeID, 0, roomType) { }

            public GrowthParameters(int sourceNodeID, int branchID, RoomType roomType)
            {
                this.sourceNodeID = sourceNodeID;
                this.branchID = branchID;
                this.roomType = roomType;
            }
        }

        class GrowthAttempt
        {
            public GrowthParameters Parameters;
            public PathMapNode SourceNode;
            public PathMapNode NewBud;
            public List<PathMapNode> Segments = new();
            public bool BuildSucceeded; // Tracks that nodes have been successfully built, this is used to determine if we need to clean up failed attempts
            public bool OverlapsValid;  // Tracks if the new nodes overlap with existing ones, this is used to determine if we need to clean up failed attempts
            
            public int TargetLength;        // The length of the bridge sections not including the bud
            public int TotalGrowth => Segments.Count + (NewBud == null ? 0 : 1);    // Include the bud if it is not null
            public int BranchGrowth => Segments.Count;
            public bool GrowthComplete => TotalGrowth >= TargetLength;    
            public int RemainingGrowth => TargetLength - TotalGrowth;

            public RoomType RoomType => Parameters.roomType;

            public int BranchID 
            { 
                get { return Parameters.branchID; } 
                set { Parameters.branchID = value; } 
            }
            public int SourceNodeID 
            { 
                get { return Parameters.sourceNodeID; } 
                set { Parameters.sourceNodeID = value; } 
            }
            public int PathDepthIndex;
            public int NextNodeIndex;
            public string GenerationLog;    // This is so we dont need to add a Debug.Log constantly, we can print once
        }
        readonly struct NodeAddress
        {
            public readonly int Branch;
            public readonly int Depth;

            public NodeAddress(int branch, int depth)
            {
                Branch = branch;
                Depth = depth;
            }
        }
    }
}


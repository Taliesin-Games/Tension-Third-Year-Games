using System.Collections.Generic;
using UnityEngine;

namespace BMD.ProcGen
{
    public partial class TerrainGenerator : MonoBehaviour
    {
        public static TerrainGenerator Instance { get; private set; }
        const int LOOP_PROTECTION_LIMIT = 100;
        const int MAX_GROWTH_RETRIES = 5;
        readonly Vector3 TERRAIN_CAM_ROTATION = new(80, 0, 0);

        #region References
        // Key: (x, y) coordinates of the node, x = branch index, y = depth level in the path
        Dictionary<NodeAddress, PathMapNode> generatedNodes = new();
        Dictionary<int, int> branchLengths = new();
        PathMapNode currentPlayerNode;   // Location of the player.
        PathMapNode currentBossNode;     // Location of the boss.
        Camera mainCamera;
        Camera terrainGenCam;
        #endregion

        #region Runtime variables
        AudioClip debugBeep;
        System.Random rng;
        Coroutine generationCoroutine;
        bool isGenerating = false;
        bool generationComplete = false;
        int generationStepsThisFrame; // Counter to track how many nodes have been generated in the current frame
        bool debugStepDoneThisFrame = false;
        string growthLog = "";
        string generationStepUIOutput = "";
        #endregion

        #region Preallocations
        // These are preallocated to save assignment performance but are usually only used locally
        readonly List<ConnectionDirection> allowedDirections = new();
        readonly List<ConnectionDirection> biasDirections = new();
        List<ConnectionDirection> selectedDirectionList = new();
        readonly List<Connection> selectedConnections = new();
        object Throttle = null;
        readonly WaitForFixedUpdate waitForFixedUpdate = new();
        readonly WaitForSeconds slowTextUpdate = new(0.2f);
        readonly List<GameObject> validPathPrefabs = new();
        #endregion
        #region Properties
        public bool TerrainReady => !isGenerating && generationComplete;
        public string GenerationStepUIOutput => generationStepUIOutput;

        #endregion
    }
}


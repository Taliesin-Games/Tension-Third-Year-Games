using UnityEngine;

namespace BMD.ProcGen
{
    public partial class TerrainGenerator : MonoBehaviour
    {
        private void Awake()
        {
            CreateInstance();
            SetRandomSeed();
            SanityChecks();
            debugBeep = CreateDebugBeep();
            CreateTerrainCam();
        }
        private void Start()
        {
            generationCoroutine = StartCoroutine(GenerateLevel());
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
            // If we are in demonstration mode, we override the random seed with one from the demonstration seeds array, this ensures that the same levels are generated each time for demonstration purposes.
            if (demonstrationMode)
            {
                randomSeed = demonstrationSeeds[UnityEngine.Random.Range(0, demonstrationSeeds.Length)];
                Debug.Log($"Demonstration mode enabled. Random seed set to {randomSeed} from demonstration seeds.");
            }

            // If seed is 0 set a seed, also output to console for debugging purposes
            // Also sets if demonstration mode is enabled but the demonstration seeds array is empty, this ensures that we still get a random seed in this case.
            if (randomSeed == 0)
            {
                randomSeed = System.Environment.TickCount; // Use current time as seed if 0 is specified
                Debug.Log($"Random seed set to {randomSeed} based on current time.");
            }
            rng = new System.Random(randomSeed);
        }

        private void CreateTerrainCam()
        {
            if (!slowGeneration) return;

            // Create a camera pointing down at 0,20,0
            terrainGenCam = new GameObject("TerrainGenCam").AddComponent<Camera>();
            terrainGenCam.transform.position = new Vector3(0, terrainCamHeight, 0);
            terrainGenCam.transform.rotation = Quaternion.Euler(TERRAIN_CAM_ROTATION);

            // Set as active camera and disable the main camera
            mainCamera = Camera.main;
            if (mainCamera != null) mainCamera.enabled = false;

        }
    }
}


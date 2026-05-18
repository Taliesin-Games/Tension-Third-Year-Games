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
            if (randomSeed == 0)
            {
                randomSeed = System.Environment.TickCount; // Use current time as seed if 0 is specified
                Debug.Log($"Random seed set to {randomSeed} based on current time.");
            }
            rng = new System.Random(randomSeed);
        }
    }
}


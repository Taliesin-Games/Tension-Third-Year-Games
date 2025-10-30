using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformanceTest : MonoBehaviour
{
    #region Configuration
    [Header("Prefabs")]
    [SerializeField] private GameObject modelPrefab;
    [SerializeField] private GameObject billboardPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private int numberToSpawn = 50;
    [SerializeField] private float loadDelay = 2f;
    #endregion

    #region Runtime Variables
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private bool isSpawning = false;
    #endregion

    private void Update()
    {
        if (isSpawning) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            StartCoroutine(SpawnTestObjects(modelPrefab));
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StartCoroutine(SpawnTestObjects(billboardPrefab));
        }
    }

    private IEnumerator SpawnTestObjects(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("No prefab assigned for this test.");
            yield break;
        }

        isSpawning = true;

        // Clear previous objects
        foreach (var obj in spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedObjects.Clear();

        // Wait for scene to settle
        if (loadDelay > 0)
        {
            Debug.Log($"Waiting {loadDelay} seconds before spawning...");
            yield return new WaitForSeconds(loadDelay);
        }

        // Spawn in a line between startPoint and endPoint
        Vector3 startPos = startPoint.position;
        Vector3 endPos = endPoint.position;

        for (int i = 0; i < numberToSpawn; i++)
        {
            float t = (float)i / (numberToSpawn - 1);
            Vector3 position = Vector3.Lerp(startPos, endPos, t);
            GameObject obj = Instantiate(prefab, position, Quaternion.identity);
            spawnedObjects.Add(obj);
        }

        Debug.Log($"Spawned {numberToSpawn} instances of {prefab.name}.");
        isSpawning = false;
    }
}

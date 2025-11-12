using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.AI.Navigation;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;


[RequireComponent(typeof(NavMeshSurface))]
public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [Serializable]
    struct EnemySpawnStruct
    {
        public GameObject enemy;
        public int quantity;
    }

    [SerializeField] NavMeshSurface navMesh;
    [SerializeField] List<EnemySpawnStruct> enemiesToSpawn; 
    [SerializeField] private int maxSampleTries = 20; //max number of attempts to spawn single enemy
    [SerializeField] private float minSpawnSpacing = 2.5f; //minimum space between spawn points


    #region Variables
    private List<GameObject> spawnedEnemies = new List<GameObject>(); //used to track enemies that are spawned
    private List<Vector3> spawnedPositions = new List<Vector3>(); // used to track used spawn locations to help prevent overlapping spawns

    bool isSpawningComplete = false;
    
    #endregion

    #region Properties
    // Number of not null spawned enemies being tracked
    public static int EnemyCount
    {
        get
        {
            Instance.ValidateTrackedEnemies();
            return Instance.spawnedEnemies.Count;
        }
    }
    public static bool IsSpawningComplete { get => Instance.isSpawningComplete; }

    #endregion

    #region Helper Methods
    public void RemoveEnemy(GameObject enemy) { spawnedEnemies.Remove(enemy); }
    #endregion

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (navMesh == null) navMesh = GetComponent<NavMeshSurface>();
        if (navMesh != null) { SpawnEnemies(); }
    }

    private void SpawnEnemies()
    {

        if (enemiesToSpawn.Count == 0) return;

        //Iterate through all enemy spawn structs
        for (int i = 0; i < enemiesToSpawn.Count; i++)
        {
            //attempt to spawn quantity of enemies specified in the struct
            for (int j = 0; j < enemiesToSpawn[i].quantity; j++)
            {
                //attempt to find random position on navmesh
                UnityEngine.Vector3 randomPosition = GetRandomPointInNavMeshVolume();

                //if valid spawn location found, spawn enemy
                if (randomPosition != UnityEngine.Vector3.zero)
                {
                    //REPLACE WITH SPAWN REQUEST FROM OBJECT POOLER
                    GameObject tempEnemy = Instantiate(enemiesToSpawn[i].enemy, randomPosition, UnityEngine.Quaternion.identity);
                    
                    if (tempEnemy != null)
                    {
                        spawnedPositions.Add(randomPosition);
                        spawnedEnemies.Add(tempEnemy);
                    }
                }
            }
        }

        isSpawningComplete = true;
    }

    void DespawnAll()
    {
        ValidateTrackedEnemies();
        foreach (GameObject enemy in spawnedEnemies)
        {
            //REPLACE WITH DESPAWN LOGIC FROM OBJECT POOLER
            Destroy(enemy);
        }
        spawnedEnemies.Clear();
    }

    private void ValidateTrackedEnemies()
    {
        //removes all null references in the list of enemies
        spawnedEnemies.RemoveAll(item => item == null);
    }

    private Bounds GetNavMeshSurfaceBounds(NavMeshSurface surface)
    {
        // Local to world transform
        Vector3 worldCenter = surface.transform.TransformPoint(surface.center);
        Vector3 worldSize = Vector3.Scale(surface.size, surface.transform.lossyScale);

        return new Bounds(worldCenter, worldSize);
    }

    private Vector3 GetRandomPointInNavMeshVolume()
    {
        Bounds bounds = GetNavMeshSurfaceBounds(navMesh);

        for (int i = 0; i < maxSampleTries; i++)
        {
            // random point inside box
            Vector3 randomPoint = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                Random.Range(bounds.min.z, bounds.max.z)
            );

            // find closest valid navmesh point near that random position
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                //ensure minimum distance between spawn points
                if (IsFarEnoughFromOthers(hit.position))
                {
                    return hit.position;
                }
            }
        }

        Debug.LogWarning("No valid NavMesh point found inside volume.");
        return Vector3.zero;
    }

    private bool IsFarEnoughFromOthers(Vector3 newPos)
    {
        foreach (var pos in spawnedPositions)
        {
            if (Vector3.SqrMagnitude(pos - newPos) < minSpawnSpacing * minSpawnSpacing)
                return false;
        }
        return true;
    }
}

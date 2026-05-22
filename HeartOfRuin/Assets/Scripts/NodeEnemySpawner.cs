using BMD.ProcGen;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NodeEnemySpawner : MonoBehaviour
{
    const float PLAYER_ACTIVATION_RANGE = 30f;
    const float NAV_MESH_SEARCH_RADIUS = 4f;

    [SerializeField] Enemy[] commonEnemiesToSpawn;
    [SerializeField] Enemy[] rareEnemiesToSpawn;
    [SerializeField] int enemySpawnQuantity = 3;
    [SerializeField] float rareChance = 0.20f;
    [SerializeField] int maxEnemiesToSpawn = 10;
    [SerializeField] float spawnRadius = 20f; // TODO replace with finding bounding box of the node and spawning within that, this will allow for better use of space and less clumping of enemies

    NavMeshSurface navMeshSurface;

    int totalEnemiesToSpawn;
    int enemiesSpawned = 0;

    int multipler = 1;
    
    List<Enemy> spawnedEnemies = new();

    float DistanceToPlayer => Player.Instance ? Vector3.Distance(transform.position, Player.Instance.transform.position) : float.MaxValue;

    private void Awake()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
    }

    private void Start()
    {
        StartCoroutine(PlayerDistanceCheck());
        StartCoroutine(DelayedSpawn());
    }
    IEnumerator DelayedSpawn()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();

            if (TerrainGenerator.Instance.SpawnEnemiesNow)
            {
                SpawnEnemiesNow();
                yield break;
            }
        }
    }
    IEnumerator PlayerDistanceCheck()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            if(DistanceToPlayer <= PLAYER_ACTIVATION_RANGE)
            {
                ActivateEnemies();
            }
            else
            {
                DeactivateEenemies();
            }
        }
    }

    void DeactivateEenemies()
    {
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy) enemy.gameObject.SetActive(false);
        }
    }
    void ActivateEnemies()
    {
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy) enemy.gameObject.SetActive(true);
        }
    }

    public void SpawnEnemiesNow(int multipler = 1)
    {
        totalEnemiesToSpawn = enemySpawnQuantity * multipler;
        
        this.multipler = multipler; // TODO get node branch position to increase as path lengthens

        StartCoroutine(SpawnEnemies());
    }
    IEnumerator SpawnEnemies()
    {
        while (enemiesSpawned < totalEnemiesToSpawn)
        {
            yield return new WaitForFixedUpdate();

            if (spawnedEnemies.Count >= maxEnemiesToSpawn)
            {
                yield return new WaitUntil(() => spawnedEnemies.Count < maxEnemiesToSpawn);
            }

            SpawnOneEnemy();


        }
    }
    void SpawnOneEnemy()
    {
        Enemy enemyToSpawn = null;
        if (Random.value < rareChance)
        {
            enemyToSpawn = rareEnemiesToSpawn[Random.Range(0, rareEnemiesToSpawn.Length)];
        }
        else
        {
            enemyToSpawn = commonEnemiesToSpawn[Random.Range(0, commonEnemiesToSpawn.Length)];
        }
        if (enemyToSpawn)
        {
            var spawnedEnemy = Instantiate(enemyToSpawn, GetEnemySpawnPosition(), Quaternion.identity);
            spawnedEnemies.Add(spawnedEnemy);
            enemiesSpawned++;
        }
    }

    Vector3 GetEnemySpawnPosition()
    {
        // If nav mesh not null, get a point on a circle around the spawner,
        // If its null pcik a random point in a circle around the spawner, this allows the spawner to work even if the nav mesh is not baked yet, but will still use the nav mesh if it is available for better spawn points.

        Vector3 position = transform.position;
        position.y = 0f;

        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 randomPoint = position + new Vector3(randomCircle.x, 0f, randomCircle.y);

        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, NAV_MESH_SEARCH_RADIUS, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return randomPoint;
    }

}

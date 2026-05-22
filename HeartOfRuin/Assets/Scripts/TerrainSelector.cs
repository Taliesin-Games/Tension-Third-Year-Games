using BMD.ProcGen;
using UnityEngine;

public class TerrainSelector : MonoBehaviour
{
    [Tooltip("The terrain generators to choose from. Warning, this is not validated ")] // TODO add validation
    [SerializeField] TerrainGenerator[] terrainGenerators;

    private void Awake()
    {
        int randomIndex = Random.Range(0, terrainGenerators.Length);

        // Spawn the prefab at origin with no rotation
        Instantiate(terrainGenerators[randomIndex], Vector3.zero, Quaternion.identity);
    }
}

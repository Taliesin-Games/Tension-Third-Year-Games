using System;
using UnityEngine;
using UnityEditor;



public class LootComponent : MonoBehaviour
{
    [SerializeField] LootTable lootTable;

    [SerializeField] Inventory[] lootInventories;
    
    LootGenerator lootGenerator;

    public LootTable GetLootTable() => lootTable;
    public Inventory[] GetLootInventories() => lootInventories;

    private void Start()
    {
        lootGenerator = UnityEngine.Object.FindFirstObjectByType<LootGenerator>();
        if (lootGenerator == null)
        {
            Debug.LogError("LootComponent: No LootGenerator found in scene. Please add one.");
            return;
        }
        lootGenerator.GenerateLoot(this);
    }

}



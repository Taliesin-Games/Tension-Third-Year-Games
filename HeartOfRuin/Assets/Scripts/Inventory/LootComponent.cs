using System;
using UnityEngine;
using UnityEditor;



public class LootComponent : MonoBehaviour
{
    [SerializeField] LootTable lootTable;

    [SerializeField] Inventory[] lootInventories;

    // One entry per ItemRarity; rarity is hidden and populated automatically.
    [SerializeField] DropRates dropRates;
    
    LootGenerator lootGenerator;

    public LootTable GetLootTable() => lootTable;
    public Inventory[] GetLootInventories() => lootInventories;
    public DropRates GetDropRates() => dropRates;

    private void Reset()
    {
        dropRates.EnsureDropRates();
    }

    private void OnValidate()
    {
        dropRates.EnsureDropRates();
    }

    private void Awake()
    {
        dropRates.EnsureDropRates();
    }

    private void Start()
    {
        dropRates.EnsureDropRates();

        lootGenerator = UnityEngine.Object.FindFirstObjectByType<LootGenerator>();
        if (lootGenerator == null)
        {
            Debug.LogWarning("LootComponent: No LootGenerator found in the scene. Loot generation will not work.");
        }
        else { lootGenerator.GenerateLoot(this); }
    }


}



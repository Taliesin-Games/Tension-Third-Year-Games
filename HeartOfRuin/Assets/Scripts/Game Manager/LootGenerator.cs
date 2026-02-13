using System;
using System.Collections.Generic;
using UnityEngine;

public class LootGenerator : MonoBehaviour
{
    [SerializeField] ItemDatabase itemDatabase;

    // Generate loot from a LootTable and put into the LootComponent's inventories.
    // Uses table.Sample() to obtain (Item, qty) pairs.
    public void GenerateLoot(LootComponent component)
    {
        if (component == null || component.GetLootTable() == null) return;

        // sample the table
        var results = component.GetLootTable().Sample();

        var inventories = component.GetLootInventories();
        if (inventories == null || inventories.Length == 0) return;

        // ensure inventories are initialized
        foreach (var inv in inventories) inv?.Initialise();

        int invIndex = 0;
        int invCount = inventories.Length;

        foreach (var (item, qty) in results)
        {
            int remaining = qty;
            // try to add across inventories
            for (int i = 0; i < invCount && remaining > 0; i++)
            {
                int idx = (invIndex + i) % invCount;
                var inv = inventories[idx];
                if (inv == null) continue;
                var remSlot = inv.AddItem(item, remaining);
                if (remSlot == null)
                {
                    remaining = 0;
                    invIndex = (idx + 1) % invCount;
                    break;
                }
                else
                {
                    remaining = remSlot.GetQuantity();
                }
            }
            if (remaining > 0)
            {
                // inventories full - optional handling: spawn world drops or log
                Debug.Log($"LootGenerator: Could not place {remaining}x {item.GetItemName()} (inventories full).");
            }
        }

        // compact all inventories
        foreach (var inv in inventories) inv?.CompactInventoryNonStacking();
    }
}

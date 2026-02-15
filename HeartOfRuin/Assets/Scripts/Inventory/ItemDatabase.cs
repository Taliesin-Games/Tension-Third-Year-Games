using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Items/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<Item> items = new();

    public LootTable[] lootTables = Array.Empty<LootTable>();

    private Dictionary<string, Item> lookup;

    public void BuildLookup()
    {
        lookup = new Dictionary<string, Item>();

        foreach (Item item in items)
        {
            if (item == null || string.IsNullOrEmpty(item.GetID()))
                continue;

            lookup[item.GetID()] = item;
        }
    }

    public Item GetByID(string id)
    {
        if (lookup == null) 
        {
            BuildLookup();
        }


        lookup.TryGetValue(id, out Item item);
        return item;
    }

    public Item[] GetByTag(tg_ItemTag tag)
    {
        if (tag == null) 
        { 
            Debug.LogWarning("Tag is null. Returning empty array.");
            return new Item[0]; 
        }

        if (lookup == null) 
        { 
            BuildLookup(); 
        }

        List<Item> result = new List<Item>(); 
        foreach (Item item in items) 
        {
            if (item != null && item.HasTag(tag))
            {
                result.Add(item);
            }
        
        } 
        
        return result.ToArray();

    }
}

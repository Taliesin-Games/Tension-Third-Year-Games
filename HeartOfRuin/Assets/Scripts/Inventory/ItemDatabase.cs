using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Items/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<Item> items = new();

    private Dictionary<string, Item> lookup;

    public void BuildLookup()
    {
        lookup = new Dictionary<string, Item>();

        foreach (var item in items)
        {
            if (item == null || string.IsNullOrEmpty(item.GetID()))
                continue;

            lookup[item.GetID()] = item;
        }
    }

    public Item GetByID(string id)
    {
        if (lookup == null)
            BuildLookup();

        lookup.TryGetValue(id, out var item);
        return item;
    }
}

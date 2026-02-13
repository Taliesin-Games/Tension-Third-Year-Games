using System;
using System.Linq;
using UnityEngine;

[System.Serializable]
public enum ItemRarity { common, uncommon, rare, epic, lengendary, cosmic }

[Serializable]
public class Item : ScriptableObject
{
    [Tooltip("Icon representing the item in the inventory UI")]
    [SerializeField] Sprite itemIcon;
    [Tooltip("Name of the item")]
    [SerializeField] string itemName;
    [Tooltip("Description of the item")]
    [SerializeField] string itemDescription;
    [Tooltip("Unique identifier for the item")]
    [SerializeField] private string id;
    [Tooltip("3D model of the item for world representation")]
    [SerializeField] GameObject itemMesh;
    [Tooltip("Max number of items that can be stacked into a single inventory slot")]
    [SerializeField] int maxStackSize = 1;
    [Tooltip("Rarity level of the item, used for visual effects and loot generation")] 
    [SerializeField] 
    ItemRarity rarity = ItemRarity.common;

    [Tooltip("Tags used to categorize this item for loot tables or filtering (reference Tag assets)")]
    [SerializeField] tg_ItemTag[] tags = new tg_ItemTag[0];

    public Sprite GetItemIcon()
    {
        return itemIcon;
    }

    public string GetItemName()
    {
        return itemName;
    }

    public string GetItemDescription()
    {
        return itemDescription;
    }

    public string GetID()
    {
        return id;
    }

    public GameObject GetItemMesh()
    {
        return itemMesh;
    }

    public int GetMaxStackSize()
    {
        return maxStackSize;
    }

    // NEW: expose rarity to runtime code
    public ItemRarity GetRarity()
    {
        return rarity;
    }

    public virtual EquipSlotType GetEquipSlotType()
    {
        return EquipSlotType.None;
    }

    // Tag helpers
    public tg_ItemTag[] GetTagObjects()
    {
        return tags ?? Array.Empty<tg_ItemTag>();
    }

    public string[] GetTagNames()
    {
        return GetTagObjects().Where(t => t != null).Select(t => t.GetName()).ToArray();
    }

    public bool HasTag(tg_ItemTag tag)
    {
        if (tag == null) return false;
        return GetTagObjects().Any(t => t != null && t.GetID() == tag.GetID());
    }

    public bool HasTag(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName)) return false;
        var tn = tagName.Trim();
        return GetTagObjects().Any(t => t != null && string.Equals(t.GetName(), tn, StringComparison.OrdinalIgnoreCase));
    }
}
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
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
    [SerializeField] int id; //TODO: Ensure unique IDs across all items
    [Tooltip("3D model of the item for world representation")]
    [SerializeField] GameObject itemMesh;
    [Tooltip("Max number of items that can be stacked into a single inventory slot")]
    [SerializeField] int maxStackSize = 1;

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

    public int GetID()
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

}

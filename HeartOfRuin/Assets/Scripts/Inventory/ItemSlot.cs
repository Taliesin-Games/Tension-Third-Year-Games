using UnityEngine;

[System.Serializable]
public class ItemSlot
{
    [SerializeField] Item item;
    [SerializeField] int quantity;
    [SerializeField] EquipSlotType slotType; // This is per-slot metadata and is preserved

    // Runtime index/property - this is optional; prefer using Inventory list index as source-of-truth.
    // Kept for backwards compatibility but Inventory will maintain indices when needed.
    int inventoryIndex = -1;

    // --- Read-only accessors (avoid exposing setters publicly) ---
    public Item GetItem() => item;
    public int GetQuantity() => quantity;
    public EquipSlotType GetSlotType() => slotType;
    public int GetIndex() => inventoryIndex;

    // --- Initialization & internal mutation methods ---
    public void Initialize(EquipSlotType initialSlotType = EquipSlotType.None, int index = -1)
    {
        slotType = initialSlotType;
        inventoryIndex = index;
        item = null;
        quantity = 0;
    }

    /// <summary>Set the full contents of the slot (replaces contents, preserves slotType and index)</summary>
    public void Set(Item newItem, int newQuantity)
    {
        item = newItem;
        quantity = newQuantity;
    }

    /// <summary>Set only the item (keeps quantity untouched)</summary>
    public void SetItem(Item newItem)
    {
        item = newItem;
    }

    /// <summary>Set only the quantity</summary>
    public void SetQuantity(int newQuantity)
    {
        quantity = newQuantity;
        if (quantity <= 0)
        {
            // leave item reference as-is or clear? We keep Clear separate to avoid surprising side-effects.
        }
    }

    public void SetIndex(int index) => inventoryIndex = index;
    public void SetSlotType(EquipSlotType type) => slotType = type;

    // --- Helper mutation APIs ---
    public void Clear()
    {
        item = null;
        quantity = 0;
    }

    public bool IsEmpty()
    {
        return item == null || quantity <= 0;
    }

    public bool IsFull()
    {
        if (item == null) return false;
        return quantity >= item.GetMaxStackSize();
    }

    /// <summary>Attempt to add up to 'amount' to the slot. Returns how many were actually added.</summary>
    public int TryAddQuantity(int amount)
    {
        if (item == null || amount <= 0) return 0;
        int free = item.GetMaxStackSize() - quantity;
        int add = Mathf.Min(free, amount);
        quantity += add;
        return add;
    }

    /// <summary>Attempt to add an item+quantity into the slot if it is empty and the slot type is compatible.
    /// Returns how many were placed into the slot (0 if incompatible or not empty).</summary>
    public int TryPlaceIntoEmptySlot(Item candidateItem, int amount)
    {
        if (candidateItem == null || amount <= 0) return 0;
        if (!IsEmpty()) return 0;
        if (slotType != EquipSlotType.None && slotType != candidateItem.GetEquipSlotType()) return 0;

        int add = Mathf.Min(amount, candidateItem.GetMaxStackSize());
        item = candidateItem;
        quantity = add;
        return add;
    }

    /// <summary>Remove a quantity from the slot. Returns the actual removed amount.</summary>
    public int RemoveQuantity(int amount)
    {
        if (IsEmpty() || amount <= 0) return 0;
        int removed = Mathf.Min(quantity, amount);
        quantity -= removed;
        if (quantity <= 0)
            Clear();
        return removed;
    }
}

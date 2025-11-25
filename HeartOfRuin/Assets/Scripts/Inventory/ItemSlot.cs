using UnityEngine;

[System.Serializable]
public class ItemSlot
{

    [SerializeField] Item item;
    [SerializeField] int quantity;
    [SerializeField] EquipSlotType SlotType;
    int inventoryIndex = -1;

    public Item GetItem()
    {
        return item;
    }

    public void SetItem(Item item)
    {
        this.item = item;
    }

    public void SetItem(Item item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }

    public int GetQuantity()
    {
        return quantity;
    }

    public void SetQuantity(int quantity)
    {
        this.quantity = quantity;
    }

    public EquipSlotType GetSlotType()
    {
        return SlotType;
    }

    public void SetSlotType(EquipSlotType slotType)
    {
        this.SlotType = slotType;
    }

    public void SetIndex(int index)
    {
        inventoryIndex = index;
    }

    public int GetIndex()
    {
        return inventoryIndex;
    }


    public bool IsEmpty()
    {
        if (item == null || quantity <= 0)
        {
            ClearSlot();
            return true;
        }
        return false;
    }

    public bool IsFull()
    {
        if (item == null) return false;
        return quantity >= item.GetMaxStackSize();
    }

    public void ClearSlot()
    {
        item = null;
        quantity = 0;
    }
}

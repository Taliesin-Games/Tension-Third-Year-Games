using UnityEngine;

[System.Serializable]
public class ItemSlot
{

    [SerializeField] Item item;
    [SerializeField] int quantity;
    [SerializeField] EquipSlotType SlotType;

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
        if (item is EquippableItem equippable)
        {
            return equippable.GetEquipSlotType();
        }
        return EquipSlotType.None;
    }

    public void SetSlotType(EquipSlotType slotType)
    {
        this.SlotType = slotType;
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

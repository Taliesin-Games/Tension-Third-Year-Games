using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Inventory : MonoBehaviour
{

    [SerializeField] int inventorySize = 20;
    [SerializeField] ItemSlot[] inventorySlots;
    List<ItemSlot> _inventorySlots ;

    Inventory()
    {
        inventorySlots = new ItemSlot[inventorySize];
    }

    private void Start()
    {
        _inventorySlots = inventorySlots.ToList<ItemSlot>();
        EnsureInventorySize(_inventorySlots, inventorySize);
    }

    // Compacts the inventory by removing empty slots and shifting items to the front, maintaining their order.
    public void CompactInventoryNonStacking(List<ItemSlot> items)
    {
        int write = 0;

        // Move non-empty slots up
        for (int read = 0; read < items.Count; read++)
        {
            if (items[read] != null && !items[read].IsEmpty())
            {
                items[write] = items[read];
                write++;
            }
        }

        // Clear remaining slots (ensure slot objects exist)
        for (int i = write; i < items.Count; i++)
        {
            if (items[i] == null)
                items[i] = new ItemSlot();

            items[i].ClearSlot();
        }

        // Ensure list size is always InventorySize
        while (items.Count < inventorySize)
            items.Add(new ItemSlot());
    }

    void EnsureInventorySize(List<ItemSlot> slots, int requiredSize)
    {
        // Replace nulls and ensure existing slots are valid
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == null)
                slots[i] = new ItemSlot();
        }

        // Add missing slots if list is too small
        while (slots.Count < requiredSize)
            slots.Add(new ItemSlot());
    }

    // Returns remaining quantity that could not be added (0 if all added successfully)
    public int AddItem(Item item, int quantity)
    {
        if (item == null || quantity <= 0)
            return 0;

        //ensure the list has exactly InventorySize usable slots
        EnsureInventorySize(_inventorySlots, inventorySize);

        //pass 1: Stack into existing matching stacks
        quantity = FillStacks(item, quantity, true);

        //pass 2: Place new stacks into empty slots
        quantity = PlaceInEmptySlots(item, quantity, true);

        // Inventory full, return remaining quantity
        return quantity;
    }

    public int AddItem(ItemSlot itemSlot)
    {
        if (itemSlot == null || itemSlot.GetItem() == null || itemSlot.GetQuantity() <= 0)
        {
            return 0; 
        }

        return AddItem(itemSlot.GetItem(), itemSlot.GetQuantity());
    }

    int FillStacks(Item item, int quantity, bool prevalidated)
    {
        //prevalidation check, ensure inventory and attempted inputs are valid
        if (!prevalidated)
        {
            if (item == null || quantity <= 0)
                return quantity;

            EnsureInventorySize(_inventorySlots, inventorySize);
        }

        // First pass: Try to fill existing stacks
        for (int i = 0; i < _inventorySlots.Count; i++)
        {
            var slot = _inventorySlots[i];
            if (slot.IsEmpty())
                continue;

            if (slot.GetItem() == item)
            {
                int maxStack = item.GetMaxStackSize();
                int freeSpace = maxStack - slot.GetQuantity();
                if (freeSpace <= 0)
                    continue;

                int add = Mathf.Min(quantity, freeSpace);
                slot.SetQuantity(slot.GetQuantity() + add);
                quantity -= add;

                if (quantity <= 0)
                    return 0;
            }
        }

        return quantity;
    }

    // Returns remaining quantity that could not be added
    int PlaceInEmptySlots(Item item, int quantity, bool prevalidated)
    {
        //prevalidation check, ensure inventory and attempted inputs are valid
        if (!prevalidated)
        {
            if (item == null || quantity <= 0)
                return quantity;

            EnsureInventorySize(_inventorySlots, inventorySize);
        }

        for (int i = 0; i < _inventorySlots.Count; i++)
        {
            var slot = _inventorySlots[i];
            if (!slot.IsEmpty())
                continue;

            int add = Mathf.Min(quantity, item.GetMaxStackSize());
            slot.SetItem(item, add);
            quantity -= add;

            if (quantity <= 0)
                return 0;
        }

        return quantity;
    }

    // Transfers item to another inventory, returns true if successful
    public void TransferItemToAnotherInventory(Inventory outputInv, int indexOfItemToTransfer)
    {
        if (outputInv == null || indexOfItemToTransfer < 0 || indexOfItemToTransfer >= _inventorySlots.Count)
            return;

        if (_inventorySlots[indexOfItemToTransfer].IsEmpty())
            return;

        ItemSlot slotToTransfer = _inventorySlots[indexOfItemToTransfer];
        int remaining = outputInv.AddItem(slotToTransfer.GetItem(), slotToTransfer.GetQuantity());
        if (remaining == 0)
        {
            // All items transferred, clear the slot
            slotToTransfer.ClearSlot();
        }
        else
        {
            // Some items could not be transferred, update the quantity
            slotToTransfer.SetQuantity(remaining);
        }
    }

    public void TransferEntireInventory(Inventory outputInv)
    {
        if (outputInv == null)
            return;

        for (int i = 0; i < _inventorySlots.Count; i++)
        {
            var slot = _inventorySlots[i];
            if (slot.IsEmpty())
                continue;
            int remaining = outputInv.AddItem(slot.GetItem(), slot.GetQuantity());
            if (remaining == 0)
            {
                // All items transferred, clear the slot
                slot.ClearSlot();
            }
            else
            {
                // Some items could not be transferred, update the quantity
                slot.SetQuantity(remaining);
            }
        }
    }

    public void ClearInventory()
    {
        foreach (var slot in _inventorySlots)
        {
            slot.ClearSlot();
        }
    }

    public bool RemoveItem(int index, int quantity)
    {
        if (index < 0 || index >= _inventorySlots.Count)
            return false;

        if (_inventorySlots[index].IsEmpty())
            return false;

        int currentQuantity = _inventorySlots[index].GetQuantity();
        if (quantity <= 0 || quantity > currentQuantity)
            return false;

        _inventorySlots[index].SetQuantity(currentQuantity - quantity);
        return true;
    }

    public bool IsEmpty()
    {
        foreach (var slot in _inventorySlots)
        {
            if (!slot.IsEmpty())
                return false;
        }
        return true;
    }

    bool UseItem()
    {
        return true;
    }

    public void PrintInventory()
    {
        Debug.Log("Inventory Contents:");
        for (int i = 0; i < _inventorySlots.Count; i++)
        {
            var slot = _inventorySlots[i];
            if (!slot.IsEmpty())
            {
                Debug.Log($"Slot {i}: {slot.GetItem().GetItemName()} x{slot.GetQuantity()}");
            }
            else
            {
                Debug.Log($"Slot {i}: Empty");
            }
        }
    }


    public void Swap(int a, int b)
    {
        (inventorySlots[a], inventorySlots[b]) = (inventorySlots[b], inventorySlots[a]);
    }

    public List<ItemSlot> GetInventorySlots()
    {
        return _inventorySlots;
    }

    public int GetInventorySize() { return _inventorySlots.Count; }

}

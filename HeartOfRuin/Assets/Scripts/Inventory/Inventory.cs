using JetBrains.Annotations;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;


public class Inventory : MonoBehaviour
{

    [SerializeField] int inventorySize = 20;
    int validatedInventorySize = 0;
    [SerializeField] ItemSlot[] startingItems;
    List<ItemSlot> _inventorySlots ;
    [SerializeField] GameObject itemDropPrefab;

    Inventory()
    {
        startingItems = new ItemSlot[inventorySize];
    }

    private void Start()
    {
        _inventorySlots = startingItems.ToList<ItemSlot>();
        validatedInventorySize = _inventorySlots.Count;
        ValidateInventorySize();
    }

    private void Update()
    {
        ValidateInventorySize();
    }



    /// <summary>
    /// Compacts the inventory by removing empty slots and shifting items to the front, maintaining their order. 
    /// DO NOT USE FOR INVENTORY SIZE CHANGES, USE <see cref="ValidateInventorySize"/> INSTEAD
    /// </summary>
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

    /// <summary>
    /// Ensures inventory is no larger than the input size. Drops items on floor if too many in inventory after removing empty slots
    /// </summary>
    void EnsureMaximumInventorySize()
    {
        //remove empty slots from the back first
        for (int i = _inventorySlots.Count - 1; i >= 0; i--)
        {
            if (_inventorySlots.Count == inventorySize)
            {
                break;
            }

            if (_inventorySlots[i] == null || _inventorySlots[i].IsEmpty())
            {
                _inventorySlots.RemoveAt(i);
            }
            
        }
        Debug.Log($"size after empties cleared: {_inventorySlots.Count}");
        //If still too large, remove items from the back
        while (_inventorySlots.Count > inventorySize)
        {
            DropItem(_inventorySlots.Count - 1);
            _inventorySlots.RemoveAt(_inventorySlots.Count - 1);
        }
    }

    /// <summary>
    /// Ensures inventory is at least the input size. Does not account for if inventory is larger than required size
    /// </summary>
    void EnsureMinimumInventorySize()
    {
        // Replace nulls and ensure existing slots are valid
        for (int i = 0; i < _inventorySlots.Count; i++)
        {
            if (_inventorySlots[i] == null)
                _inventorySlots[i] = new ItemSlot();
        }

        // Add missing slots if list is too small
        while (_inventorySlots.Count < inventorySize)
            _inventorySlots.Add(new ItemSlot());
    }

    /// <summary>
    /// Ensures inventory matches inventorySize, will drop items from inventory if reduction requires it
    /// </summary>
    public void ValidateInventorySize()
    {
        if (inventorySize == validatedInventorySize)
        {
            Debug.Log("Inventory size ok");
            return;
        }

        if (inventorySize < 0)
        {
            return;
        }

        if (inventorySize > validatedInventorySize)
        {
            Debug.Log("Inventory size to small");
            EnsureMinimumInventorySize();
            validatedInventorySize = inventorySize;
        }

        if (inventorySize < validatedInventorySize)
        {
            Debug.Log("Inventory size too big");
            EnsureMaximumInventorySize();
            validatedInventorySize = inventorySize;
        }
    }

    /// <summary>
    /// Add item to an inventory.
    /// First fills any stacks of existing items first, then fills empty slots.
    /// Ensures inventory size before adding.
    /// </summary>
    /// <returns>
    /// Quantity of items that could not be added
    /// </returns>
    public int AddItem(Item item, int quantity)
    {
        if (item == null || quantity <= 0)
            return 0;

        //ensure the list has exactly InventorySize usable slots
        ValidateInventorySize();

        //pass 1: Stack into existing matching stacks
        quantity = FillStacks(item, quantity, true);

        //pass 2: Place new stacks into empty slots
        quantity = PlaceInEmptySlots(item, quantity, true);

        // Inventory full, return remaining quantity
        return quantity;
    }

    /// <summary>
    /// Add item to an inventory.
    /// First fills any stacks of existing items first, then fills empty slots.
    /// Ensures inventory size before adding.
    /// </summary>
    /// <returns>
    /// Quantity of items that could not be added
    /// </returns>
    public int AddItem(ItemSlot itemSlot)
    {
        if (itemSlot == null || itemSlot.GetItem() == null || itemSlot.GetQuantity() <= 0)
        {
            return 0; 
        }

        return AddItem(itemSlot.GetItem(), itemSlot.GetQuantity());
    }

    /// <summary>
    /// Attempts to fill existing inventory slots with matching item. Does not fill empty slots.
    /// </summary>
    /// <returns>
    /// Quantity of items remaining after attempting to add.
    /// </returns>
    int FillStacks(Item item, int quantity, bool prevalidated)
    {
        //prevalidation check, ensure inventory and attempted inputs are valid
        if (!prevalidated)
        {
            if (item == null || quantity <= 0)
                return quantity;

            ValidateInventorySize();
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

    /// <summary>
    /// Attempts to fill empty inventory slots with item. Does not fill existing stacks of item.
    /// </summary>
    /// <returns>
    /// Quantity of items remaining after attempting to add.
    /// </returns>
    int PlaceInEmptySlots(Item item, int quantity, bool prevalidated)
    {
        //prevalidation check, ensure inventory and attempted inputs are valid
        if (!prevalidated)
        {
            if (item == null || quantity <= 0)
                return quantity;

            ValidateInventorySize();
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

    /// <summary>
    /// Attempts to transfer an item from this inventory to an inventory. Does not swap two items.
    /// </summary>
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

    /// <summary>
    /// Attempts to transfer an one this inventory to another. Does not swap items.
    /// </summary>
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

    /// <summary>
    /// Attempts to remove an a set quantity of an item from inventory.
    /// </summary>
    /// <returns>
    /// returns true if successfully removed.
    /// </returns>
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

    /// <summary>
    /// Checks if the inventory is empty
    /// </summary>
    /// <returns>
    /// Returns true if empty
    /// </returns>
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

    /// <summary>
    /// Drops an item from the inventory as an object into the world placed at the characters location, -1 quantity drops index's full quantity
    /// </summary>
    public void DropItem(int index, int quantity = -1)
    {
        //validate input
        if (index >= _inventorySlots.Count || index < 0) { return; }
        if (quantity == 0 || quantity < -1) { return; }
        if (itemDropPrefab == null) { return; }

        GameObject droppedItem = Instantiate(itemDropPrefab); //drpped item prefab
        droppedItem.transform.position = gameObject.transform.position; //place prefab at players location

        if (droppedItem.GetComponent<ItemPickup>() != null) //Ensure the "dropped item" prefab that is set has an item pickup component
        {
            //copy existing slot to dropped item
            var slot = _inventorySlots[index];
            if (slot.IsEmpty()) { return; }
            droppedItem.GetComponent<ItemPickup>().itemSlot.SetItem(slot.GetItem(), slot.GetQuantity());
            

            //dropped all or attempted to drop more than available, can return
            if (quantity == -1 || quantity > _inventorySlots[index].GetQuantity())
            {
                _inventorySlots[index].ClearSlot();
                return;
            }

            //dropped specific quantity
            droppedItem.GetComponent<ItemPickup>().itemSlot.SetQuantity(quantity); 
            _inventorySlots[index].SetQuantity(_inventorySlots[index].GetQuantity() - quantity);
        }

    }

    /// <summary>
    /// Swaps two item slots in the inventory
    /// </summary>
    public void Swap(int a, int b)
    {
        (startingItems[a], startingItems[b]) = (startingItems[b], startingItems[a]);
    }

    public List<ItemSlot> GetInventorySlots()
    {
        return _inventorySlots;
    }

    public int GetInventorySize() { return _inventorySlots.Count; }

}

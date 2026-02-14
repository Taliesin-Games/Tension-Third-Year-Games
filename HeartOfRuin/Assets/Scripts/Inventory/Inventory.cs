using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] string inventoryName = "";
    [SerializeField] int inventorySize = 20;
    // For initial configuration, you can assign starting slot contents in the inspector.
    // These will be copied into the permanent slot objects on Initialise.
    [SerializeField] ItemSlot[] startingItems = new ItemSlot[0];
    [SerializeField] GameObject itemDropPrefab; // optional
    int validatedInventorySize = 0;

    // Internal permanent slots — never replace these objects (UI holds references safely)
    List<ItemSlot> _inventorySlots;
    bool initialised = false;

    // Events - keep same semantics as before (invoke when items added/removed)
    public Action<Item> OnAddItem;
    public Action<Item> OnRemoveItem;

    public List<ItemSlot> GetInventorySlots() => _inventorySlots;

    public int GetInventorySize() => _inventorySlots.Count;

    public ItemSlot GetSlotAtIndex(int index)
    {
        if (index < 0 || index >= _inventorySlots.Count)
        {
            return null;
        }

        return _inventorySlots[index];
    }

    internal GameObject GetItemDropPrefab() => itemDropPrefab;


    private void Start()
    {
        Initialise();
    }

    private void Update()
    {
        ValidateInventorySize();
    }

    /// <summary>
    /// Call externally to ensure inventory is initialised if access is required before this method is called internally by the inventory. 
    /// </summary>
    public void Initialise()
    {
        if (initialised) return;

        // Create permanent slot objects. Preserve any startingItems content where possible.
        _inventorySlots = new List<ItemSlot>(inventorySize);
        for (int i = 0; i < inventorySize; i++)
        {
            ItemSlot slot = new ItemSlot();
            // If a starting slot exists, copy its data (do not replace slot object)
            if (startingItems != null && i < startingItems.Length && startingItems[i] != null)
            {
                slot.Initialize(startingItems[i].GetSlotType(), i);
                slot.Set(startingItems[i].GetItem(), startingItems[i].GetQuantity());
            }
            else
            {
                slot.Initialize(EquipSlotType.None, i);
            }
            _inventorySlots.Add(slot);
        }

        // clear serialized editor helper
        startingItems = new ItemSlot[0];

        validatedInventorySize = _inventorySlots.Count;
        IndexItemSlots(); 
        initialised = true;
    }

    /// <summary>Re-invoke OnAddItem for UI rebuilds / refreshes</summary>
    public void ForceReAddItemInvoke()
    {
        foreach (ItemSlot slot in _inventorySlots)
        {
            if (!slot.IsEmpty()) 
            {
                OnAddItem?.Invoke(slot.GetItem()); 
            }
        }
    }

    /// <summary>
    /// Compact permanent slots by moving non-empty contents to the front.
    /// This mutates existing slots rather than replacing them, preserving slotType and references.
    /// </summary>
    public void CompactInventoryNonStacking()
    {
        List<(Item item, int quantity)> nonEmpty = new List<(Item, int)>();

        // collect existing item data
        for (int i = 0; i < _inventorySlots.Count; i++)
        {
            ItemSlot s = _inventorySlots[i];
            if (!s.IsEmpty())
            {
                nonEmpty.Add((s.GetItem(), s.GetQuantity()));
            }
                
        }

        // place collected items in front slots, clear rest
        int write = 0;
        for (; write < nonEmpty.Count && write < _inventorySlots.Count; write++)
        {
            _inventorySlots[write].Set(nonEmpty[write].item, nonEmpty[write].quantity);
        }

        // clear remaining slots
        for (int i = write; i < _inventorySlots.Count; i++)
        {
            _inventorySlots[i].Clear();
        }
    }

    // --- Inventory resizing (preserve slot objects) ---

    void EnsureMinimumInventorySize()
    {
        // ensure existing slots are valid (should be by Initialise but keep safe)
        for (int i = 0; i < _inventorySlots.Count; i++)
        {
            if (_inventorySlots[i] == null)
            {
                ItemSlot s = new ItemSlot();
                s.Initialize(EquipSlotType.None, i);
                _inventorySlots[i] = s;
            }
        }

        // add missing slots (create new permanent slot objects)
        while (_inventorySlots.Count < inventorySize)
        {
            ItemSlot s = new ItemSlot();
            s.Initialize(EquipSlotType.None, _inventorySlots.Count);
            _inventorySlots.Add(s);
        }
    }

    /// <summary>
    /// Shrink inventory: remove empty slots from the back first; if still too large drop items
    /// Note: This will remove slot objects from the end — UI references to removed slots will become invalid.
    /// </summary>
    void EnsureMaximumInventorySize()
    {
        // remove empty slots from the back first
        for (int i = _inventorySlots.Count - 1; i >= 0 && _inventorySlots.Count > inventorySize; i--)
        {
            if (_inventorySlots[i] == null || _inventorySlots[i].IsEmpty())
            {
                _inventorySlots.RemoveAt(i);
            }
                
        }

        // If still too large, drop items from the back
        while (_inventorySlots.Count > inventorySize)
        {
            int idx = _inventorySlots.Count - 1;
            DropItem(idx);
            _inventorySlots.RemoveAt(idx);
        }
    }

    /// <summary>
    /// Ensure inventory matches inventorySize. When shrinking, preserves behavior of dropping items.
    /// </summary>
    public void ValidateInventorySize()
    {
        if (inventorySize == validatedInventorySize)
        {
            return;
        }
        if (inventorySize < 0)
        {
            return;
        }

        if (inventorySize > validatedInventorySize)
        {
            EnsureMinimumInventorySize();
            IndexItemSlots();
            validatedInventorySize = inventorySize;
        }
        else if (inventorySize < validatedInventorySize)
        {
            EnsureMaximumInventorySize();
            IndexItemSlots();
            validatedInventorySize = inventorySize;
        }
    }

    /// <summary>
    /// Sets all slot indexes to their position in the inventory list.
    /// </summary>
    void IndexItemSlots()
    {
        for (int i = 0; i < _inventorySlots.Count; i++)
        {
            _inventorySlots[i].SetIndex(i);
        }
    }

    // --- AddItem behaviour (stack-first, then empty slots) ---

    /// <summary>
    /// Adds an Item + quantity to this inventory. Returns a new ItemSlot representing leftovers (or null if none).
    /// </summary>
    public ItemSlot AddItem(Item item, int quantity, EquipSlotType returnSlotType = EquipSlotType.None)
    {
        if (item == null || quantity <= 0)
        {
            return null;
        }

        // Ensure inventory layout
        ValidateInventorySize();

        // 1) Fill existing stacks
        for (int i = 0; i < _inventorySlots.Count && quantity > 0; i++)
        {
            ItemSlot slot = _inventorySlots[i];
            if (slot.IsEmpty())
            {
                continue;
            }
            
            if (slot.GetItem() == item)
            {
                int added = slot.TryAddQuantity(quantity);
                if (added > 0)
                {
                    quantity -= added;
                    OnAddItem?.Invoke(item);
                }
            }
        }

        // 2) Place into empty slots
        for (int i = 0; i < _inventorySlots.Count && quantity > 0; i++)
        {
            ItemSlot slot = _inventorySlots[i];
            if (!slot.IsEmpty())
            {
                continue;
            }
            int placed = slot.TryPlaceIntoEmptySlot(item, quantity);
            if (placed > 0)
            {
                quantity -= placed;
                OnAddItem?.Invoke(item);
            }
        }

        // If quantity left, return an ItemSlot representing the remainder (caller expects a slot-like return)
        if (quantity > 0)
        {
            ItemSlot remainder = new ItemSlot();
            remainder.Initialize(returnSlotType, -1);
            remainder.Set(item, quantity);
            remainder.SetSlotType(returnSlotType);
            return remainder;
        }

        return null;
    }

    public ItemSlot AddItem(ItemSlot itemSlot)
    {
        if (itemSlot == null || itemSlot.GetItem() == null || itemSlot.GetQuantity() <= 0)
        {
            return null;
        }
        return AddItem(itemSlot.GetItem(), itemSlot.GetQuantity(), itemSlot.GetSlotType());
    }

    /// <summary>
    /// Place an ItemSlot at a specific index (used for equipment slots or UI direct drops).
    /// Does NOT replace the permanent slot object — only mutates its contents.
    /// Returns leftover/swapped content as an ItemSlot (like previous behaviour).
    /// </summary>
    public ItemSlot AddItemAtIndex(int index, ItemSlot incoming)
    {
        if (incoming == null || incoming.GetItem() == null || incoming.GetQuantity() <= 0)
        {
            return null;
        }

        if (index < 0 || index >= _inventorySlots.Count)
        {
            return incoming; // invalid index
        }

        ValidateInventorySize();
        ItemSlot target = _inventorySlots[index];

        // Ensure the item can go in this slot
        if (target.GetSlotType() != EquipSlotType.None && target.GetSlotType() != incoming.GetItem().GetEquipSlotType())
        {
            return incoming; // cannot place here
        }
            

        // If target empty > place whole incoming (or as much as fits to max stack)
        if (target.IsEmpty())
        {
            // place as much as allowed by max stack
            int toPlace = Mathf.Min(incoming.GetQuantity(), incoming.GetItem().GetMaxStackSize());
            target.Set(incoming.GetItem(), toPlace);
            // If incoming had more than max stack, return remainder
            int remainderAmount = incoming.GetQuantity() - toPlace;
            OnAddItem?.Invoke(incoming.GetItem());
            if (remainderAmount > 0)
            {
                ItemSlot remainder = new ItemSlot();
                remainder.Initialize(incoming.GetSlotType(), -1);
                remainder.Set(incoming.GetItem(), remainderAmount);
                remainder.SetSlotType(incoming.GetSlotType());
                return remainder;
            }
            return null;
        }

        // If same item type -> try stacking
        if (target.GetItem() == incoming.GetItem())
        {
            int free = incoming.GetItem().GetMaxStackSize() - target.GetQuantity();
            if (free <= 0)
            {
                // cannot stack more -> swap entire slot (return previous contents)
                ItemSlot swapped = new ItemSlot();
                swapped.Initialize(target.GetSlotType(), -1);
                swapped.Set(target.GetItem(), target.GetQuantity());
                swapped.SetSlotType(target.GetSlotType());

                // set incoming into the slot
                target.Set(incoming.GetItem(), incoming.GetQuantity());
                OnAddItem?.Invoke(incoming.GetItem());
                return swapped;
            }
            else
            {
                int add = Mathf.Min(free, incoming.GetQuantity());
                target.SetQuantity(target.GetQuantity() + add);
                int remainder = incoming.GetQuantity() - add;
                OnAddItem?.Invoke(incoming.GetItem());

                if (remainder > 0)
                {
                    ItemSlot remainderSlot = new ItemSlot();
                    remainderSlot.Initialize(incoming.GetSlotType(), -1);
                    remainderSlot.Set(incoming.GetItem(), remainder);
                    remainderSlot.SetSlotType(incoming.GetSlotType());
                    return remainderSlot;
                }
                return null;
            }
        }

        // Different items -> swap contents (return old contents)
        ItemSlot oldContents = new ItemSlot();
        oldContents.Initialize(target.GetSlotType(), -1);
        oldContents.Set(target.GetItem(), target.GetQuantity());
        oldContents.SetSlotType(target.GetSlotType());

        // Put incoming into the slot (preserve slot metadata)
        target.Set(incoming.GetItem(), incoming.GetQuantity());
        OnAddItem?.Invoke(incoming.GetItem());
        OnRemoveItem?.Invoke(oldContents.GetItem());
        return oldContents;
    }

    /// <summary>
    /// Try to fill existing stacks with the provided item & quantity. Returns an ItemSlot containing remainder (or null).
    /// </summary>
    ItemSlot FillStacks(Item item, int quantity)
    {
        if (item == null || quantity <= 0)
        {
            return null;
        }
        ValidateInventorySize();

        for (int i = 0; i < _inventorySlots.Count && quantity > 0; i++)
        {
            ItemSlot slot = _inventorySlots[i];
            if (slot.IsEmpty())
            {
                continue;
            }
            if (slot.GetItem() == item)
            {
                int added = slot.TryAddQuantity(quantity);
                if (added > 0)
                {
                    quantity -= added;
                    OnAddItem?.Invoke(item);
                }
            }
        }

        if (quantity > 0)
        {
            ItemSlot remainder = new ItemSlot();
            remainder.Initialize(EquipSlotType.None, -1);
            remainder.Set(item, quantity);
            return remainder;
        }
        return null;
    }

    /// <summary>
    /// Try to fill empty slots with the provided item & quantity. Returns an ItemSlot containing remainder (or null).
    /// </summary>
    ItemSlot PlaceInEmptySlots(Item item, int quantity)
    {
        if (item == null || quantity <= 0)
        {
            return null;
        }
        ValidateInventorySize();

        for (int i = 0; i < _inventorySlots.Count && quantity > 0; i++)
        {
            ItemSlot slot = _inventorySlots[i];
            if (!slot.IsEmpty()) 
            {
                continue;
            }
            int placed = slot.TryPlaceIntoEmptySlot(item, quantity);
            if (placed > 0)
            {
                quantity -= placed;
                OnAddItem?.Invoke(item);
            }
        }

        if (quantity > 0)
        {
            ItemSlot remainder = new ItemSlot();
            remainder.Initialize(EquipSlotType.None, -1);
            remainder.Set(item, quantity);
            return remainder;
        }
        return null;
    }

    // --- Transfers between inventories ---

    /// <summary>
    /// Transfer the item at index to another inventory. Does not do swapping.
    /// </summary>
    public void TransferItemToAnotherInventory(Inventory outputInv, int indexOfItemToTransfer)
    {
        if (outputInv == null)
        {
            return;
        }

        if (indexOfItemToTransfer < 0 || indexOfItemToTransfer >= _inventorySlots.Count) 
        {
            return;
        }

        ItemSlot sourceSlot = _inventorySlots[indexOfItemToTransfer];
        
        if (sourceSlot.IsEmpty()) 
        {
            return;
        }

        // Try to add to output inventory
        ItemSlot remainder = outputInv.AddItem(sourceSlot.GetItem(), sourceSlot.GetQuantity(), sourceSlot.GetSlotType());

        if (remainder == null || remainder.GetQuantity() <= 0)
        {
            // all transferred
            OnRemoveItem?.Invoke(sourceSlot.GetItem());
            sourceSlot.Clear();
        }
        else
        {
            // partial transfer -> update source with remainder quantity
            sourceSlot.SetQuantity(remainder.GetQuantity());
        }
    }

    /// <summary>
    /// Transfer the entire inventory to another inventory. Does not do swapping.
    /// </summary>
    public void TransferEntireInventory(Inventory outputInv)
    {
        if (outputInv == null)
        {
            return;
        }
        for (int i = 0; i < _inventorySlots.Count; i++)
        {
            ItemSlot slot = _inventorySlots[i];
            if (slot.IsEmpty()) 
            {
                continue;
            }
            ItemSlot remainder = outputInv.AddItem(slot.GetItem(), slot.GetQuantity(), slot.GetSlotType());
            if (remainder == null || remainder.GetQuantity() <= 0)
            {
                OnRemoveItem?.Invoke(slot.GetItem());
                slot.Clear();
            }
            else
            {
                slot.SetQuantity(remainder.GetQuantity());
            }
        }
    }

    // --- Clearing & queries ---

    public void ClearInventory()
    {
        foreach (ItemSlot slot in _inventorySlots)
        {
            if (!slot.IsEmpty())
            {
                OnRemoveItem?.Invoke(slot.GetItem());
            }
                
            slot.Clear();
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
        {
            return false; 
        }

        ItemSlot slot = _inventorySlots[index];

        if (slot.IsEmpty()) 
        {
            return false; 
        }

        if (quantity <= 0 || quantity > slot.GetQuantity()) 
        {
            return false;
        }

        slot.RemoveQuantity(quantity);

        if (slot.IsEmpty()) 
        {
            OnRemoveItem?.Invoke(slot.GetItem());
        }
           
        return true;
    }

    public bool IsEmpty()
    {
        foreach (ItemSlot s in _inventorySlots)
        {
            if (!s.IsEmpty())
            {
                return false; 
            }
        }

        return true;
    }

    public void PrintInventory()
    {
        Debug.Log("Inventory Contents:");
        for (int i = 0; i < _inventorySlots.Count; i++)
        {
            ItemSlot slot = _inventorySlots[i];
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
    /// Instantiate a dropped item in the world. If quantity == -1, drop the whole slot.
    /// </summary>
    public void DropItem(int index, int quantity = -1)
    {
        if (index < 0 || index >= _inventorySlots.Count)
        {
            return;
        }

        if (quantity == 0 || quantity < -1)
        {
            return;
        }

        if (itemDropPrefab == null)
        {
            return;
        }

        ItemSlot slot = _inventorySlots[index];
        if (slot.IsEmpty())
        {
            return;
        }

        GameObject droppedItem = Instantiate(itemDropPrefab);
        droppedItem.transform.position = gameObject.transform.position;

        ItemPickup pickup = droppedItem.GetComponent<ItemPickup>();
        if (pickup != null)
        {
            Debug.Log($"{inventoryName} item dropped successfully");
            int dropAmount = (quantity == -1) ? slot.GetQuantity() : Mathf.Min(quantity, slot.GetQuantity());
            pickup.ItemSlot.Set(slot.GetItem(), dropAmount);

            // remove from slot
            if (quantity == -1 || quantity >= slot.GetQuantity())
            {
                OnRemoveItem?.Invoke(slot.GetItem());
                slot.Clear();
            }
            else
            {
                slot.SetQuantity(slot.GetQuantity() - dropAmount);
                OnRemoveItem?.Invoke(slot.GetItem());
            }
        }
        else
        {
            // if prefab is invalid, destroy it
            Destroy(droppedItem);
        }
    }


    /// <summary>
    /// Removes all items from the inventory and drops them into the game world.
    /// </summary>
    public void DropAllItems()
    {
        for (int i = 0; i < _inventorySlots.Count; i++)
        {
            DropItem(i);
        }
    }
}


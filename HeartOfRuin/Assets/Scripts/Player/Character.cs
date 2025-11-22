using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BMD.CharacterController))] // Ensure that a CharacterController component is attached
[RequireComponent(typeof(Inventory))]
public abstract class Character : MonoBehaviour
{
    [SerializeField] string characterName = "Dan";
    


    List<ItemEffect> activeEffects;
    Dictionary<EquipSlotType, ItemSlot> equipmentSlots;
    [SerializeField] protected Inventory inventory;

    public ItemSlot Head
    {
        get
        {
            return equipmentSlots[EquipSlotType.Head];
        }
        set
        {
            equipmentSlots[EquipSlotType.Head] = value;
        }
    }

    public ItemSlot Chest
    {
        get
        {
            return equipmentSlots[EquipSlotType.Chest];
        }
        set
        {
            equipmentSlots[EquipSlotType.Chest] = value;
        }
    }

    public ItemSlot Legs
    {
        get
        {
            return equipmentSlots[EquipSlotType.Legs];
        }
        set
        {
            equipmentSlots[EquipSlotType.Legs] = value;
        }
    }

    public ItemSlot Feet
    {
        get
        {
            return equipmentSlots[EquipSlotType.Feet];
        }
        set
        {
            equipmentSlots[EquipSlotType.Feet] = value;
        }
    }

    public ItemSlot LeftHand
    {
        get
        {
            return equipmentSlots[EquipSlotType.LeftHand];
        }
        set
        {
            equipmentSlots[EquipSlotType.LeftHand] = value;
        }
    }

    public ItemSlot RightHand
    {
        get
        {
            return equipmentSlots[EquipSlotType.RightHand];
        }
        set
        {
            equipmentSlots[EquipSlotType.RightHand] = value;
        }
    }


    protected virtual void Start()
    {
        inventory = GetComponent<Inventory>();
        Debug.Log("Character Name: " + characterName);
        if (equipmentSlots != null)
        {
            foreach (ItemSlot slot in equipmentSlots.Values)
            {
                EquippableItem tempItem = slot.GetItem() as EquippableItem;
                if (tempItem != null)
                {
                    tempItem.OnEquip(this);

                }
            }
        }
    }

    // Update is called once per frame
    protected virtual void Update()
    {

    }
   

    public void AddItemEffect(ItemEffect effect)
    {
        activeEffects.Add(effect);
        effect.Init();
        effect.OnEquipEffect(this.gameObject);
    }

    public void RemoveItemEffect(ItemEffect effect)
    {
        activeEffects.Remove(effect);
        effect.Cleanup();
    }

    public void AddItemEffects(ItemEffect[] effects)
    {
        foreach (ItemEffect effect in effects)
        {
            activeEffects.Add(effect);
        }
    }

    public void RemoveItemEffects(ItemEffect[] effects)
    {
        foreach (ItemEffect effect in effects)
        {
            activeEffects.Remove(effect);
        }
    }
}

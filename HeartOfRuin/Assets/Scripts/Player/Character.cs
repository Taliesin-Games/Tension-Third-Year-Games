using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BMD.CharacterController))] // Ensure that a CharacterController component is attached
[RequireComponent(typeof(Inventory))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PlayerStats))]
public abstract class Character : MonoBehaviour
{
    [SerializeField] string characterName = "Glorp Gleep";
    


    List<ItemEffect> activeEffects;
    //protected Dictionary<EquipSlotType, ItemSlot> equipmentSlots;
    [SerializeField] protected Inventory equipmentSlots;
    [SerializeField] protected Inventory inventory;
    [SerializeField] protected PlayerStats playerStats;
    PlayerStats baseStats;


    protected virtual void Awake()
    {
        baseStats = playerStats;
        inventory = GetComponent<Inventory>();
        Debug.Log("Character Name: " + characterName);
        if (equipmentSlots != null)
        {
            equipmentSlots.OnAddItem += OnItemEquipped;
            equipmentSlots.OnRemoveItem += OnItemUnequipped;
            equipmentSlots.Initialise();
            inventory.Initialise();
            foreach (ItemSlot slot in equipmentSlots.GetInventorySlots())
            {
                EquippableItem tempItem = slot.GetItem() as EquippableItem;
                if (tempItem != null)
                {
                    tempItem.OnEquip(this);
                }
            }
            inventory.ForceReAddItemInvoke();
            equipmentSlots.ForceReAddItemInvoke();
        }
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

    public void OnItemEquipped(Item item)
    {
        EquippableItem equippedItem = (EquippableItem)item;
        playerStats.setAgility(playerStats.getAgility() + equippedItem.GetBonusAgility());
        playerStats.setIntelligence(playerStats.getIntelligence() + equippedItem.GetBonusIntelligence());
        playerStats.setStrength(playerStats.getStrength() + equippedItem.GetBonusStrength());
        playerStats.setCriticalChance(playerStats.getCriticalChance() + equippedItem.GetBonusCriticalChance());
        playerStats.setCriticalDamage(playerStats.getCriticalDamage() + equippedItem.GetBonusCriticalDamage());
    }

    public void OnItemUnequipped(Item item)
    {
        EquippableItem equippedItem = (EquippableItem)item;
        playerStats.setAgility(playerStats.getAgility() - equippedItem.GetBonusAgility());
        playerStats.setIntelligence(playerStats.getIntelligence() - equippedItem.GetBonusIntelligence());
        playerStats.setStrength(playerStats.getStrength() - equippedItem.GetBonusStrength());
        playerStats.setCriticalChance(playerStats.getCriticalChance() - equippedItem.GetBonusCriticalChance());
        playerStats.setCriticalDamage(playerStats.getCriticalDamage() - equippedItem.GetBonusCriticalDamage());
    }
}

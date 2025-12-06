using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BMD.CharacterController))] // Ensure that a CharacterController component is attached
[RequireComponent(typeof(Inventory))]
[RequireComponent(typeof(CharacterStats))]
public abstract class Character : MonoBehaviour
{
    [SerializeField] string characterName = "Glorp Gleep";
    


    [SerializeField] List<ItemEffect> activeEffects;
    //protected Dictionary<EquipSlotType, ItemSlot> equipmentSlots;
    [SerializeField] protected Inventory equipmentSlots;
    [SerializeField] protected Inventory inventory;
    [SerializeField] protected CharacterStats characterStats;
    CharacterStats baseStats;


    protected virtual void Awake()
    {
        initialiseCharacter();
    }

    void initialiseCharacter()
    {
        baseStats = characterStats;
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
        if(effects == null) { return; }
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
        characterStats.setAgility(characterStats.getAgility() + equippedItem.GetBonusAgility());
        characterStats.setIntelligence(characterStats.getIntelligence() + equippedItem.GetBonusIntelligence());
        characterStats.setStrength(characterStats.getStrength() + equippedItem.GetBonusStrength());
        characterStats.setCriticalChance(characterStats.getCriticalChance() + equippedItem.GetBonusCriticalChance());
        characterStats.setCriticalDamage(characterStats.getCriticalDamage() + equippedItem.GetBonusCriticalDamage());
    }

    public void OnItemUnequipped(Item item)
    {
        EquippableItem equippedItem = (EquippableItem)item;
        characterStats.setAgility(characterStats.getAgility() - equippedItem.GetBonusAgility());
        characterStats.setIntelligence(characterStats.getIntelligence() - equippedItem.GetBonusIntelligence());
        characterStats.setStrength(characterStats.getStrength() - equippedItem.GetBonusStrength());
        characterStats.setCriticalChance(characterStats.getCriticalChance() - equippedItem.GetBonusCriticalChance());
        characterStats.setCriticalDamage(characterStats.getCriticalDamage() - equippedItem.GetBonusCriticalDamage());
    }
}

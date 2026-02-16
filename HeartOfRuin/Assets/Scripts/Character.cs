using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BMD.CharacterController))] // Ensure that a CharacterController component is attached
[RequireComponent(typeof(Inventory))]
[RequireComponent(typeof(CharacterStats))]
public abstract class Character : MonoBehaviour
{
    #region Configuration
    [SerializeField] string characterName = "Glorp Gleep";
    
    [SerializeField] List<ItemEffect> activeEffects;
    //protected Dictionary<EquipSlotType, ItemSlot> equipmentSlots;
    [SerializeField] protected Inventory equipmentSlots;
    [SerializeField] protected Inventory inventory;
    [SerializeField] protected CharacterStats characterStats;
    [SerializeField] SpellCaster castComponent;
    #endregion

    #region Cached References
    BMD.CharacterController controller;
    CharacterStats baseStats;
    DamageComponent damageComponent;
    #endregion

    #region Runtime Variables

    #endregion

    protected virtual void Awake()
    {
        InitialiseCharacter();
    }

    void InitialiseCharacter()
    {
        baseStats = characterStats;

        damageComponent = GetComponent<DamageComponent>();

        if (inventory == null)
        {
            inventory = GetComponent<Inventory>();
        }

        if (equipmentSlots == null)
        {
            foreach(Inventory inv in GetComponents<Inventory>())
            {
                if (inv != inventory)
                {
                    equipmentSlots = inv;
                    break;
                }
            }
        }
        controller = GetComponent<BMD.CharacterController>();

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

    private void OnEnable()
    {
        controller.OnDealDamageFromWeapon += HandleDealDamage;
        controller.OnCastSpell += HandleCastSpell;
    }

    private void OnDisable()
    {
        controller.OnDealDamageFromWeapon -= HandleDealDamage;
        controller.OnCastSpell -= HandleCastSpell;
    }

    private void HandleCastSpell()
    {
        if (!castComponent) return;
        castComponent.TryCastSpell();
    }

    private void HandleDealDamage()
    {

    }
    public void HitWithWeapon(GameObject target)
    {
        DamageStruct damage = damageComponent.CalculatePlayerDamage(baseStats);

        target.GetComponent<Health>()?.TakeDamage(damage);
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

using System;
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
    [SerializeField] DamageStruct characterDamageBonusPercentage;
    [SerializeField] SpellCaster castComponent;
    #endregion

    #region Cached References
    protected BMD.CharacterController controller;
    CharacterStats baseStats;
    DamageStruct baseDamageBonusPercentage;

    #endregion

    #region Runtime Variables
    bool weaponDamageEnabled;
    #endregion

    #region Properties
    public bool WeaponDamageEnabled => weaponDamageEnabled;

    public event Action NotifyStatChange;
    #endregion

    protected virtual void Awake()
    {
        InitialiseCharacter();
    }

    void InitialiseCharacter()
    {
        SetupInventory();
        SetupSignaling();
    }

    void SetupSignaling()
    {
        controller.OnAttackPerformed += OnAttack; 
    }
    private void SetupInventory()
    {
        baseStats = characterStats;
        baseDamageBonusPercentage = characterDamageBonusPercentage;


        if (inventory == null)
        {
            inventory = GetComponent<Inventory>();
        }

        if (equipmentSlots == null)
        {
            foreach (Inventory inv in GetComponents<Inventory>())
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

    protected virtual void Start()
    {
        NotifyStatChange?.Invoke();
    }
    protected virtual void OnEnable()
    {
        controller.OnEnableDamageFromWeapon += HangleEnableWeaponDamage;
        controller.OnDisableDamageFromWeapon += HandleDisableWeaponDamage;
        controller.OnCastSpell += HandleCastSpell;
    }
    protected virtual void OnDisable()
    {
        controller.OnEnableDamageFromWeapon -= HangleEnableWeaponDamage;
        controller.OnDisableDamageFromWeapon -= HandleDisableWeaponDamage;
        controller.OnCastSpell -= HandleCastSpell;
    }
    private void HandleCastSpell()
    {
        if (!castComponent) return;
        castComponent.TryCastSpell();
    }

    private void FixedUpdate()
    {
        foreach (var effect in activeEffects)
        {
            effect.EachFrameEffect(this.gameObject);
        }
    }

    private void HangleEnableWeaponDamage()
    {
        weaponDamageEnabled = true;
    }
    private void HandleDisableWeaponDamage()
    {
        weaponDamageEnabled = false;
    }
    public void HitWithWeapon(GameObject target)
    {

    }
    public void AddItemEffect(ItemEffect effect)
    {
        activeEffects.Add(effect);
        effect.Init();
    }
    public void RemoveItemEffect(ItemEffect effect)
    {
        activeEffects.Remove(effect);
        effect.Cleanup();
    }
    public void AddItemEffects(ItemEffect[] effects)
    {
        if(effects == null) return;
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
    public CharacterStats GetCharacterStats()
    {
        return characterStats;
    }
    public DamageStruct GetCharacterDamageBonusPercentage()
    {
        return characterDamageBonusPercentage;
    }
    public void OnItemEquipped(Item item)
    {
        EquippableItem equippedItem = (EquippableItem)item;
        characterStats.setAgility(characterStats.getAgility() + equippedItem.GetBonusAgility());
        characterStats.setIntelligence(characterStats.getIntelligence() + equippedItem.GetBonusIntelligence());
        characterStats.setStrength(characterStats.getStrength() + equippedItem.GetBonusStrength());
        characterStats.setCriticalChance(characterStats.getCriticalChance() + equippedItem.GetBonusCriticalChance());
        characterStats.setCriticalDamage(characterStats.getCriticalDamage() + equippedItem.GetBonusCriticalDamage());
        characterDamageBonusPercentage += equippedItem.GetDamageBonusPercentages();

        NotifyStatChange?.Invoke();
    }
    public void OnItemUnequipped(Item item)
    {
        EquippableItem equippedItem = (EquippableItem)item;
        characterStats.setAgility(characterStats.getAgility() - equippedItem.GetBonusAgility());
        characterStats.setIntelligence(characterStats.getIntelligence() - equippedItem.GetBonusIntelligence());
        characterStats.setStrength(characterStats.getStrength() - equippedItem.GetBonusStrength());
        characterStats.setCriticalChance(characterStats.getCriticalChance() - equippedItem.GetBonusCriticalChance());
        characterStats.setCriticalDamage(characterStats.getCriticalDamage() - equippedItem.GetBonusCriticalDamage());
        characterDamageBonusPercentage = characterDamageBonusPercentage - equippedItem.GetDamageBonusPercentages();

        NotifyStatChange?.Invoke();
    }
    public void OnAttack()
    {
        foreach (var effect in activeEffects)
        {
            effect.OnAttackEffect(this.gameObject);
        }
    }
    public void OnTakeDamage()
    {
        foreach (var effect in activeEffects)
        {
            effect.OnTakeDamageEffect(this.gameObject);
        }
    }
    public void OnHitTarget(Character target)
    {
        foreach(var effect in activeEffects)
        {
            effect.OnAttackHitEffect(this, target);
        }
    }

    /// <summary>
    /// newState: 0 = no damage, 1 = player weapon damage
    /// </summary>
    /// <param name="newState"></param>
  
}

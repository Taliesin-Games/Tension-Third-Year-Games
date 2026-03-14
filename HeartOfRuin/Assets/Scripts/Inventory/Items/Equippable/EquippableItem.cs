using UnityEngine;



public abstract class EquippableItem : Item
{
    [Tooltip("Type of item slot this can be equipped into, Any can go into None type, None type cant go into any")]
    [SerializeField] EquipSlotType equipSlotType;
    [Tooltip("Percentage damage bonuses provided by the item")]
    [SerializeField] DamageStruct damageBonusPercentages;
    [Tooltip("Bonus strength provided by the item")]
    [SerializeField] int BonusStrength = 0;
    [Tooltip("Bonus agility provided by the item")]
    [SerializeField] int BonusAgility = 0;
    [Tooltip("Bonus intelligence provided by the item")]
    [SerializeField] int BonusIntelligence = 0;
    [Tooltip("Bonus critical hit chance percentage (e.g., 0.2 for +20% critical chance)")]
    [SerializeField] float BonusCriticalChance = 0f;
    [Tooltip("Bonus critical damage percentage (e.g., 0.5 for +50% critical damage)")]
    [SerializeField] float BonusCriticalDamage = 0f;
    [Tooltip("Effects applied by the item")]
    [SerializeField] ItemEffect[] itemEffects;


    public override EquipSlotType GetEquipSlotType()
    {
        return equipSlotType;
    }

    public void OnEquip(Character character)
    {
        character.AddItemEffects(itemEffects);
        foreach(ItemEffect effect in itemEffects)
        {
            effect.OnEquipEffect(character.gameObject);
        }
    }

    public int GetBonusStrength() {return BonusStrength;}
    public int GetBonusAgility() {return BonusAgility;}
    public int GetBonusIntelligence() { return BonusIntelligence;}
    public float GetBonusCriticalChance() { return BonusCriticalChance; }
    public float GetBonusCriticalDamage() { return BonusCriticalDamage;}
    public DamageStruct GetDamageBonusPercentages() { return damageBonusPercentages; }

}
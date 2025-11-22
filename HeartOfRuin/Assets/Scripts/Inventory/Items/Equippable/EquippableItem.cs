using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum EquipSlotType
{
    None,
    Head,
    Chest,
    Legs,
    Feet,
    Hands,
    LeftHand,
    RightHand,
    TwoHanded,
    Passive,
}

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


    public EquipSlotType GetEquipSlotType()
    {
        return equipSlotType;
    }

    public void OnEquip(Character charcter)
    {
        charcter.AddItemEffects(itemEffects);

    }
}
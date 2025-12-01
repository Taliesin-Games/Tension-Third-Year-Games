using UnityEngine;


public enum WeaponType
{
    None,
    Sword,
    Shield,
    Special, // For unique weapons such as the elves twinblade-bow-daggers
}

[CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Weapon")]
public class Weapon : EquippableItem
{
    [SerializeField] DamageStruct weaponDamageScalings;
    [SerializeField] WeaponType weaponType;

    public DamageStruct GetWeaponDamageScalings()
    {
        return weaponDamageScalings;
    }
    public WeaponType GetWeaponType()
    {
        return weaponType;
    }
}

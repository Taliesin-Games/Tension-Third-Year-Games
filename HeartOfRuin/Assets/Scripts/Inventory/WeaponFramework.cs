using UnityEngine;

public class WeaponFramework : MonoBehaviour
{
    bool EquipWeapon()
    {
        return true;
    }

    void UnequipWeapon()
    {
    }

    Weapon CreateWeapon()
    {
        return ScriptableObject.CreateInstance<Weapon>();
    }
}

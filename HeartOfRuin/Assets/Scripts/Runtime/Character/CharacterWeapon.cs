using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(DamageComponent))]
public class CharacterWeapon : MonoBehaviour
{
    #region Cached References
    Character character;
    DamageComponent damageComponent;
    #endregion


    private void Start()
    {
        character = GetComponentInParent<Character>();
        if (!character) Debug.LogError($"Character not found for {name}");

        damageComponent = GetComponent<DamageComponent>();

    }

    private void OnTriggerEnter(Collider other)
    {
        if (character == null) return;
        
        if(!character.WeaponDamageEnabled) return;

        if ((other.gameObject.layer) == 6) return;  // Layer 6 Player, ignore collisions with player layer

        // Check if the collided object has a Health component
        if (other.GetComponent<Health>() == null) return;
        HitWithWeapon(other.gameObject);
        
    }

    private void HitWithWeapon(GameObject target) 
    {
        Debug.Log("Hit object: " + target.name);

        CharacterStats playerStats = GetComponentInParent<Character>()?.GetCharacterStats();
        DamageStruct damageBonusPercentage = GetComponentInParent<Character>()?.GetCharacterDamageBonusPercentage() ?? new DamageStruct();

        DamageStruct damage = damageComponent.CalculatePlayerDamage(playerStats, damageBonusPercentage);

        target.GetComponent<Health>()?.TakeDamage(damage);
    }
}

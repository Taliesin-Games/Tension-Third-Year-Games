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

        // Check if target and self later are the same
        if (other.gameObject.layer == gameObject.layer) return;

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

        if(target.TryGetComponent<Health>(out Health health))
        {
            health.TakeDamage(damage);
            if(target.TryGetComponent<Character>(out Character targetChar))
                character.OnHitTarget(targetChar);

            if(character.TryGetComponent<DpsTracker>(out DpsTracker dpsTracker))
                dpsTracker.RecordDamage(damage);
            
        }

        
    }
}

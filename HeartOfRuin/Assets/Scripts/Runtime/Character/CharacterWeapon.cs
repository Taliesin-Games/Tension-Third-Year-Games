using UnityEngine;

[RequireComponent(typeof(DamageComponent))]
public class CharacterWeapon : MonoBehaviour
{
    #region Cached References
    Character character;
    DamageComponent damageComponent;
    #endregion

    [SerializeField] AudioClip weaponSound;

    public void SetParentCharacter(Character character)
    {
        this.character = character;
    }

    private void Start()
    {
        character = GetComponentInParent<Character>();
        if (!character) Debug.LogError($"Character not found for {name}, at {transform.position} on parent {transform.parent.name}");

        damageComponent = GetComponent<DamageComponent>();

    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Weapon collided with: " + other.gameObject.name);

        if (character == null) return;
        
        if(!character.WeaponDamageEnabled) return;

        // Check if target and self later are the same
        if (other.gameObject.layer == gameObject.layer) return;

        if (weaponSound != null) AudioSource.PlayClipAtPoint(weaponSound, transform.position);

        // Check if the collided object has a Health component
        if (other.GetComponent<Health>() == null) return;
        HitWithWeapon(other.gameObject);
        
    }

    private void HitWithWeapon(GameObject target) 
    {
        if (character == null) return;

        Debug.Log("Hit object: " + target.name);


        CharacterStats playerStats = character?.GetCharacterStats();
        DamageStruct damageBonusPercentage = character?.GetCharacterDamageBonusPercentage() ?? new DamageStruct();

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

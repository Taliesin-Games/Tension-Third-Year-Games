using UnityEngine;

[CreateAssetMenu(fileName = "New Item Effect", menuName = "Inventory/ItemEffect")]
public class ItemEffect : ScriptableObject
{
    //not sure if we need these, may be useful if we want different items to have same effect, might make it simpler to memorise for players
    string effectName; //example: "Balsalt" (definetly not brimstone from the binding of isaac) 
    string effectDescription; //example: "Staff normal attack replaced with powerful red laser beam" (definetly not brimstone from the binding of isaac) 

    /* This class contains various methods that can be overridden to define specific item effects.
     * Some Effects will require implementation in multiple methods depending on their nature.
     * Some Effects may also require additional virables to track state. Example: A counter for number of attacks since effect last triggered, or a Timer for timed effects.
     */

    //TODO: bind things to character class (clone)

    public void Init()
    {

    }

    public void Cleanup()
    {

    }

    static public void EachFrameEffect(GameObject player)
    {
        // Apply effect logic here
        // Example: Passive health regeneration (isaac example: Placenta)
    }

    static public void OnPickupEffect(GameObject player)
    {
        // Apply effect logic here
        // Heal the player by 20 health points upon pickup (isaac example: Breakfast)
    }

    public void OnEquipEffect(GameObject player)
    {
        // Apply effect logic here
        // Example: Increase player speed by 10% while equipped (isaac example: The Belt)
    }

    static public void OnDropEffect(GameObject player)
    {
        // TODO: Bind to 
        // Apply effect logic here, primarily cleanup for persistent effects such as onPickup
        // Could also have effects when dropped on the ground: Spawn harmful area effect (isaac example: Mom's Toenail)
    }

    static public void OnDodgeEffect(GameObject player)
    {
        // TODO: Bind to player dodge event
        // Apply effect logic here
        // Example: Temporary speed boost after dodge
    }

    static public void OnPerfectDodgeEffect(GameObject player)
    {
        // TODO: Bind to player dodge event
        // Apply effect logic here
        // Example: Grant temporary attack boost after perfect dodge
    }

    static public void OnAttackEffect(GameObject player)
    {
        // TODO: Bind to player attack
        // Apply effect logic here
        // Example: Every Third Attack deals double damage
    }

    static public void OnAttackHitEffect(GameObject player)
    {
        // TODO: Bind to player hit feedback
        // Apply effect logic here
        // Example: Chance to spawn damaging ray of light on hit (isaac example: Holy Light)
    }

    static public void OnTakeDamageEffect(GameObject player)
    {
        // TODO: Bind to player take damage check
        // Apply effect logic here
        // Example: Chance to heal a small amount of health (isaac example: Gimpy)
    }

    static public void OnBlockEffect(GameObject player)
    {
        // TODO: bind to player take damage check
        // Apply effect logic here
        // Example: Reflect a portion of damage back to the attacker 
    }

    static public void OnHealEffect(GameObject player)
    {
        // TODO: bind the player heal
        // Apply effect logic here
        // Example: Increase healing received by 15%
    }

    static public void OnDeathEffect(GameObject player)
    {
        // TODO: Bind to player death and ensure death flags on player dont trigger events
        // Apply effect logic here
        // Example: Respawn player with 1 health (isaac example: Dead Cat)
    }

    static public void PerProjectileEffect(GameObject player)
    {
        // TODO: Bind to player spawn projectile
        // Apply effect logic here
        // could be used to apply another ItemEffect to the projectile or modify its properties
        // Example: Add homing effect to fired projectiles (isaac example: Spoon Bender)
    }
}

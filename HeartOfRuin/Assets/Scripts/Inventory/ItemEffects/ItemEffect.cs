using UnityEngine;

//[CreateAssetMenu(fileName = "New Item Effect", menuName = "Inventory/Item Effect")]
public class ItemEffect : ScriptableObject
{
    [SerializeField] string effectName;
    [SerializeField][TextArea] string effectDescription;

    // Expose read-only accessors so other code can show these in the UI if needed.
    public string EffectName => effectName;
    public string EffectDescription => effectDescription;

    // Optional lifecycle hooks — keep them virtual so specific effects can override.
    public virtual void Init() { }

    public virtual void Cleanup() { }

    public virtual void EachFrameEffect(GameObject character)
    {
        // Apply effect logic here (pass-through base implementation)
        Debug.Log($"{character.name} per frame item effect triggered");
    }

    public virtual void OnPickupEffect(GameObject character)
    {
        Debug.Log($"{character.name} on pickup item triggered");
    }

    public virtual void OnEquipEffect(GameObject character)
    {
        Debug.Log($"{character.name} On Equip Item triggered.");
    }

    public virtual void OnDropEffect(GameObject character)
    {
        Debug.Log($"{character.name} on dropepd item triggered");
    }

    public virtual void OnDodgeEffect(GameObject character)
    {
        Debug.Log($"{character.name} on dodge triggered");
    }

    public virtual void OnPerfectDodgeEffect(GameObject character)
    {
        Debug.Log($"{character.name} on perfect dodge triggered");
    }

    public virtual void OnAttackEffect(GameObject character)
    {
        Debug.Log($"{character.name} On Attacking triggered.");
    }

    public virtual void OnAttackHitEffect(Character character, Character target)
    {
        Debug.Log($"{character.name} on attack hit triggered with target {target.name}");
    }

    public virtual void OnTakeDamageEffect(GameObject character)
    {
        Debug.Log($"{character.name} take damage triggered");
    }

    public virtual void OnBlockEffect(GameObject character)
    {
        Debug.Log($"{character.name} On block triggered triggered");
    }

    public virtual void OnHealEffect(GameObject character)
    {
        Debug.Log($"{character.name} heal effect triggered");
    }

    public virtual void OnDeathEffect(GameObject character)
    {
        Debug.Log($"{character.name} death effect triggered");
    }

    public virtual void PerProjectileEffect(GameObject character)
    {
        Debug.Log($"{character.name} projectile effect triggered triggered");
    }
}

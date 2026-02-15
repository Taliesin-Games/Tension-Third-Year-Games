using UnityEngine;

[CreateAssetMenu(fileName = "New Item Effect", menuName = "Inventory/Item Effect")]
public abstract class ItemEffect : ScriptableObject
{
    [SerializeField] string effectName;
    [SerializeField][TextArea] string effectDescription;

    // Expose read-only accessors so other code can show these in the UI if needed.
    public string EffectName => effectName;
    public string EffectDescription => effectDescription;

    // Optional lifecycle hooks — keep them virtual so specific effects can override.
    public virtual void Init() { }

    public virtual void Cleanup() { }

    public virtual void EachFrameEffect(GameObject player)
    {
        // Apply effect logic here (pass-through base implementation)
    }

    public virtual void OnPickupEffect(GameObject player)
    {
    }

    public virtual void OnEquipEffect(GameObject player)
    {
    }

    public virtual void OnDropEffect(GameObject player)
    {
    }

    public virtual void OnDodgeEffect(GameObject player)
    {
    }

    public virtual void OnPerfectDodgeEffect(GameObject player)
    {
    }

    public virtual void OnAttackEffect(GameObject player)
    {
    }

    public virtual void OnAttackHitEffect(GameObject player)
    {
    }

    public virtual void OnTakeDamageEffect(GameObject player)
    {
    }

    public virtual void OnBlockEffect(GameObject player)
    {
    }

    public virtual void OnHealEffect(GameObject player)
    {
    }

    public virtual void OnDeathEffect(GameObject player)
    {
    }

    public virtual void PerProjectileEffect(GameObject player)
    {
    }
}

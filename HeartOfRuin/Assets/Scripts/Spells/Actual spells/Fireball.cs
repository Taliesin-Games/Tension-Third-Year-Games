using UnityEngine;


[CreateAssetMenu(menuName = "Spells/FireBall")]
[System.Serializable]
public class Fireball : SpellBase
{

    [SerializeField] GameObject ProjectilePrefab;
    

    public override void Cast(SpellContext context)
    {
        lastCastContext = context;
        GameObject projectile = Object.Instantiate(
            ProjectilePrefab,
            context.CastOrigin,
            Quaternion.LookRotation(context.Direction));

        projectile.GetComponent<Projectile>()?.SetSpell(this); 
    }

    public override void DealDamage(GameObject target)
    {
        if(lastCastContext.Caster != null && lastCastContext.damageComponent)
        {
            PlayerStats playerStats = lastCastContext.Caster.GetComponent<PlayerStats>();

            DamageStruct damage = lastCastContext.damageComponent.CalculatePlayerDamage(playerStats, damageScalings);

            target.GetComponent<Health>()?.TakeDamage(damage);
        }
    }
}

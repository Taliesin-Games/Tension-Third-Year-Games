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

        // If layer is player set layer to PlayerProjectile, if layer is enemy set layer to EnemyProjectile, otherwise default to Projectile
        int layer;

        if (context.Caster.layer == LayerMask.NameToLayer("Player"))
        {
            layer = LayerMask.NameToLayer("PlayerProjectiles");
        }
        else if (context.Caster.layer == LayerMask.NameToLayer("Enemy"))
        {
            layer = LayerMask.NameToLayer("EnemyProjectiles");
        }
        else
        {
            layer = LayerMask.NameToLayer("Projectiles");
        }

        SetLayerRecursively(projectile, layer);


        projectile.GetComponent<Projectile>()?.SetSpell(this); 
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    public override void DealDamage(GameObject target)
    {
        if(lastCastContext.Caster != null && lastCastContext.damageComponent)
        {
            if(target != lastCastContext.Caster)
            {
                CharacterStats playerStats = lastCastContext.Caster.GetComponent<CharacterStats>();

                DamageStruct damage = lastCastContext.damageComponent.CalculatePlayerDamage(playerStats, damageScalings);

                target.GetComponent<Health>()?.TakeDamage(damage);
            }

        }
    }
}

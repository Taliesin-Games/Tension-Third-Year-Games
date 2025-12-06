using UnityEngine;


[CreateAssetMenu(menuName = "Spells/FireBall")]
[System.Serializable]
public class Fireball : SpellBase
{

    [SerializeField] GameObject ProjectilePrefab;
    SpellContext lastCastContext;

    public override void Cast(SpellContext context)
    {
        lastCastContext = context;
        Object.Instantiate(
            ProjectilePrefab,
            context.CastOrigin,
            Quaternion.LookRotation(context.Direction));
    }
}

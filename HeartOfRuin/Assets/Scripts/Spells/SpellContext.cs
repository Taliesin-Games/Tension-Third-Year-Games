using UnityEngine;

public struct SpellContext
{
    public GameObject Caster;
    public Vector3 CastOrigin;
    public Vector3 Direction;
    public DamageComponent damageComponent;

    public SpellContext(GameObject caster)
    {
        Caster = caster;
        CastOrigin = caster.transform.position;
        Direction = caster.transform.forward;
        damageComponent = caster.GetComponent<DamageComponent>();
    }
}
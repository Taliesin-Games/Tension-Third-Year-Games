using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Cone Of Cold")]
public class ConeOfCold : SpellBase
{
    [Header("Cone Settings")]
    [SerializeField] private float castRange = 12f;
    [SerializeField] float castConeAngle = 35f;

    [SerializeField] bool drawDebug = false;

    public void DrawConeDebug(Vector3 origin, Vector3 forward, float maxRange, float angle)
    {
        // Left boundary
        Quaternion leftRot = Quaternion.AngleAxis(-angle, Vector3.up);
        Vector3 leftDir = leftRot * forward;

        // Right boundary
        Quaternion rightRot = Quaternion.AngleAxis(angle, Vector3.up);
        Vector3 rightDir = rightRot * forward;

        // Draw the rays
        Debug.DrawRay(origin, leftDir * maxRange, Color.cyan, 0.1f);
        Debug.DrawRay(origin, rightDir * maxRange, Color.cyan, 0.1f);
    }


    public override void Cast(SpellContext context)
    {
        lastCastContext = context;


        //cast out a cone
        Collider[] hits = Physics.OverlapSphere(context.Caster.transform.position + context.Direction * (castRange * 0.5f),
                                        castRange*0.5f);

        if (drawDebug)
        {
            DrawConeDebug(context.Caster.transform.position, context.Direction, castRange, castConeAngle);
        }


        if (hits.Length == 0)
        {
            return;
        }

        //hit targets in cone
        foreach (GameObject target in FindTargets(hits))
        {
            DealDamage(target);
        }
    }

    List<GameObject> FindTargets(Collider[] inHits)
    {
        List<GameObject> best = new List<GameObject>();

        foreach (Collider hit in inHits)
        {
            if (hit.gameObject.GetComponent<Health>() == null || hit.gameObject == lastCastContext.Caster)
            {
                continue;
            }

            Vector3 dir = (hit.transform.position - lastCastContext.Caster.transform.position).normalized;
            float angle = Vector3.Angle(lastCastContext.Direction, dir);

            if (angle < castConeAngle)
            {
                best.Add(hit.gameObject);
            }
        }
        return best;
    }



    public override void DealDamage(GameObject target)
    {
        if (lastCastContext.Caster != null && lastCastContext.damageComponent)
        {
            if (target != lastCastContext.Caster)
            {
                CharacterStats playerStats = lastCastContext.Caster.GetComponent<CharacterStats>();

                DamageStruct damage = lastCastContext.damageComponent.CalculatePlayerDamage(playerStats, damageScalings);

                target.GetComponent<Health>()?.TakeDamage(damage);
            }

        }
    }
}

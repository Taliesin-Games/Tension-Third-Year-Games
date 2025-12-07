using System.Collections.Generic;
using UnityEngine;
using static Unity.Cinemachine.IInputAxisOwner.AxisDescriptor;

[CreateAssetMenu(menuName = "Spells/Chain Lightning")]
public class ChainLightningSpell : SpellBase
{
    [Header("Chain Settings")]
    [SerializeField] private float bounceRange = 12f;
    [SerializeField] private int maxBounces = 5;
    [SerializeField] float castConeAngle = 35f;

    [Header("Visuals")]
    [SerializeField] private GameObject lightningVfxPrefab;
    [SerializeField] private float lightningLifetime = 0.15f;

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

        Collider[] hits = Physics.OverlapSphere(context.Caster.transform.position + context.Direction * (bounceRange * 0.5f),
                                        bounceRange * 0.5f);

        if (drawDebug)
        {
            DrawConeDebug(context.Caster.transform.position, context.Direction, bounceRange, castConeAngle);
        }

        if (hits.Length == 0)
        {
            return;
        }

        GameObject firstTarget = FindInitialTarget(hits);

        if (firstTarget == null) 
        {
            return; 
        }

        // Perform the bounce logic
        BounceLightning(context.Caster, firstTarget.transform.position, firstTarget);
    }

    private void BounceLightning(GameObject caster, Vector3 startPoint, GameObject firstHit)
    {
        HashSet<GameObject> hitTargets = new HashSet<GameObject>();

        Vector3 currentPoint = startPoint;
        GameObject currentTarget = firstHit;

        for (int i = 0; i < maxBounces; i++)
        {
            // Damage target
            DealDamage(currentTarget);
            hitTargets.Add(currentTarget);

            // Find next bounce target
            currentTarget = FindNextTarget(currentTarget, hitTargets, out Vector3 nextPoint);

            if (currentTarget == null)
            {
                break;
            }

            // Visual lightning effect
            SpawnLightningEffect(currentPoint, currentTarget.transform.position);

            // Continue chain
            currentPoint = nextPoint;
        }
    }
    
    GameObject FindInitialTarget(Collider[] inHits)
    {
        GameObject best = null;
        float closest = float.MaxValue;

        foreach (Collider hit in inHits)
        {
            if (hit.gameObject.GetComponent<Health>() == null || hit.gameObject == lastCastContext.Caster)
            {
                continue;
            }

            Vector3 dir = (hit.transform.position - lastCastContext.CastOrigin).normalized;
            float angle = Vector3.Angle(lastCastContext.Direction, dir);

            if (angle < castConeAngle)
            {
                float dist = Vector3.Distance(lastCastContext.CastOrigin, hit.transform.position);
                if (dist < closest)
                {
                    closest = dist;
                    best = hit.gameObject;
                }
            }
        }

        return best;
    }

    GameObject FindNextTarget(
        GameObject fromTarget,
        HashSet<GameObject> alreadyHit,
        out Vector3 hitPoint)
    {
        hitPoint = Vector3.zero;

        Collider fromCollider = fromTarget.GetComponent<Collider>();
        Vector3 fromPos = fromCollider.bounds.center;

        Collider[] hits = Physics.OverlapSphere(fromPos, bounceRange);

        float closest = float.MaxValue;
        GameObject bestCandidate = null;

        foreach (Collider hit in hits)
        {
            // Skip already hit targets
            if (alreadyHit.Contains(hit.gameObject))
            {
                continue;
            }

            if (hit.gameObject.GetComponent<Health>() == null || hit.gameObject == lastCastContext.Caster)
            {
                continue;
            }

            float dist = Vector3.Distance(fromPos, hit.bounds.center);

            if (dist < closest)
            {
                closest = dist;
                bestCandidate = hit.gameObject;
                hitPoint = hit.bounds.center;
            }
        }

        return bestCandidate;
    }

    private void SpawnLightningEffect(Vector3 from, Vector3 to)
    {
        if (lightningVfxPrefab != null)
        {
            //ooooooh lightning
        }
        if (drawDebug)
        {
            Debug.DrawLine(from, to, Color.blueViolet, lightningLifetime);
        }

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

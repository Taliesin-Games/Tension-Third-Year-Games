using System;
using UnityEngine;


[CreateAssetMenu(fileName = "New Item Effect", menuName = "Inventory/Item Effects/DOT Effect")]
public class DOTEffect : ItemEffect
{

    [SerializeField] private DamageStruct damagePerSecond;
    float timeSinceLastDamage;

    public override void EachFrameEffect(GameObject character)
    {
        timeSinceLastDamage += Time.deltaTime;
        if (timeSinceLastDamage >= 1f)
        {
            if (character.TryGetComponent<Health>(out Health health))
            {
                health.TakeDamage(damagePerSecond);
                timeSinceLastDamage = 0f;
            }
        }

    }
}

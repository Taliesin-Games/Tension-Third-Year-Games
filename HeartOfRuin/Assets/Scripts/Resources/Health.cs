using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;


public class Health : Resource
{
   

    bool isDead;
    public bool IsDead => isDead;

    [SerializeField] DamageStruct Resistances;

    //Demo purpose only - visualise Damage Numbers !!REMOVE AFTER DEMO!!
    [SerializeField] GameObject DamageNumberPrefab;


    public void TakeDamage(DamageStruct damage)
    {
        if (isDead) return; // Ignore damage if already dead

        // apply resistances
        // incomingDamage * (1 - target.resistance.stat)
        float finalDamage = (float)ApplyResistances(damage);
        decreaseResource(finalDamage);

        Debug.Log($"{transform.root.name} has taken {finalDamage} damage");

        //DEMO PURPOSE ONLY - SHOW DAMAGE NUMBERS !!REMOVE AFTER DEMO!!
        if (DamageNumberPrefab != null)
        {
            GameObject instance = Instantiate(DamageNumberPrefab, transform.position, Quaternion.identity);
            instance.GetComponent<DemoDamageNumbers>().Initialize(finalDamage);
        }
        //END DEMO PURPOSE ONLY

        if (GetCurrentResource() <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return; // Cannot heal if dead
        increaseResource(amount);
        //Debugger.Log($"{transform.root.name} has healed {amount} health");
    }

    DamageStruct ApplyResistances(DamageStruct damage)
    {
        // apply resistances
        // incomingDamage * (1 - target.resistance.stat)
        DamageStruct adjustedDamage = damage * (1 - Resistances);
        return adjustedDamage;
    }

    void Die()
    {
        if (isDead) return; // Prevent multiple death triggers
        isDead = true;
        //Debugger.Log($"{gameObject.name} has died");
        SendMessage("Die", SendMessageOptions.DontRequireReceiver);
    }
}

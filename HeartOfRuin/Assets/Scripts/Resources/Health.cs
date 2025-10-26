using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;


public class Health : Resource
{

    bool isDead;
    public bool IsDead => isDead;

    public void TakeDamage(float damage)
    {
        if (isDead) return; // Ignore damage if already dead

        decreaseResource(damage);
        //Debugger.Log($"{transform.root.name} has taken {damage} damage");
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


    void Die()
    {
        if (isDead) return; // Prevent multiple death triggers
        isDead = true;
        //Debugger.Log($"{gameObject.name} has died");
        SendMessage("Die", SendMessageOptions.DontRequireReceiver);
    }
}

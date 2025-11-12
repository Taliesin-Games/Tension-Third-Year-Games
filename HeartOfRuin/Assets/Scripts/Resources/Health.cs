using UnityEngine;
using Utils;


public class Health : Resource
{
   
    bool isDead;
    public bool IsDead => isDead;

    [Tooltip("Damage resistances applied to incoming damage as a percentage, eg 0.1 = 10%\n" + 
        "True damage resistance will be ignored.")]
    [SerializeField] DamageStruct resistances;

    //Demo purpose only - visualise Damage Numbers !!REMOVE AFTER DEMO!!
    [SerializeField] GameObject damageNumberPrefab;

    #region Cached References
    BMD.CharacterController characterController;
    #endregion

    protected override void Start()
    {
        base.Start();
        
        // Cache references
        characterController = GetComponent<BMD.CharacterController>();
    }

    public void TakeDamage(DamageStruct damage)
    {
        if (isDead) return; // Ignore damage if already dead

        // apply resistances
        // incomingDamage * (1 - target.resistance.stat)
        float finalDamage = (float)ApplyResistances(damage);
        decreaseResource(finalDamage);

        Debugger.Log($"{transform.root.name} has taken {finalDamage} damage");
        Debugger.Log($"Remaining Health: {GetCurrentResource()} / {GetMaxResource()}");

#if DEMO_MODE
        //DEMO PURPOSE ONLY - SHOW DAMAGE NUMBERS !!REMOVE AFTER DEMO!!
        if (damageNumberPrefab != null)
        {
            GameObject instance = Instantiate(damageNumberPrefab, transform.position, Quaternion.identity);
            instance.GetComponent<DamageNumbers>().Initialize(finalDamage);
        }
        //END DEMO PURPOSE ONLY
#endif
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
        DamageStruct adjustedDamage = damage * (1 - resistances);
        return adjustedDamage;
    }
    void Die()
    {
        if (isDead) return; // Prevent multiple death triggers

        isDead = true;

        // Notify Character controller of death, if is player controller then trigger game manager game over
        if (characterController != null && characterController.GetType() == typeof(BMD.PlayerController))
        {
            // If game manager exists, trigger game over, else debug log
            // Outpuit game manager instance as GameManager gm from if statement
            if (GameManager.Instance is GameManager gm) gm.GameOver();
            else Debugger.LogError("Player has died - No Game manager found to trigger game over.");
        }


        //Debugger.Log($"{gameObject.name} has died");
        SendMessage("Die", SendMessageOptions.DontRequireReceiver);
    }
}

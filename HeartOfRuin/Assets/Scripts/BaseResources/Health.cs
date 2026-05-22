using UnityEngine;
using Utils;


public class Health : Resource
{
   
    bool isDead;
    public bool IsDead => isDead;


    [Tooltip("Damage resistances applied to incoming damage as a percentage, eg 0.1 = 10%\n" + 
        "True damage resistance will be ignored.")]
    [SerializeField] DamageStruct resistances;


    #region Cached References
    BMD.CharacterController characterController;
    Character character;
    #endregion

    protected override void Start()
    {
        base.Start();
        
        // Cache references
        characterController = GetComponent<BMD.CharacterController>();
        character = GetComponent<Character>();
    }

    public void TakeDamage(DamageStruct damage)
    {
        
        if (isDead) return; // Ignore damage if already dead

        // apply resistances
        // incomingDamage * (1 - target.resistance.stat)
        float finalDamage = (float)ApplyResistances(damage);
        DecreaseResource(finalDamage);

        Debugger.Log($"{transform.root.name} has taken {finalDamage} damage");
        Debugger.Log($"Remaining Health: {GetCurrentResource()} / {GetMaxResource()}");

        DamageUIVisualisationController.Instance?.VisualiseDamage(finalDamage, gameObject, this);
        InvokeResourceChanged();
        

        if (GetCurrentResource() <= 0)
        {
            Die();
        }
    }
    public void Heal(float amount)
    {
        if (isDead) return; // Cannot heal if dead
        IncreaseResource(amount);
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
        else if (characterController != null && characterController.GetType() == typeof(EnemyController))
        {
            characterController.RequestDie();
        }


        //Debugger.Log($"{gameObject.name} has died");
        SendMessage("Die", SendMessageOptions.DontRequireReceiver);
    }
}

using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Hierarchy;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;


public class DamageComponent : MonoBehaviour
{

    // OVERALL DAMAGE CALCULATION 
    // For each damage type in damageStruct Damage = ((damageScaling.stat * playerStats.relevantStat) * (1 + critMultiplier)) * (1 - target.resistance.stat)

    [Tooltip("Damage scaling percentage per relevant player stat, 0.1 = 10%\n" +
        "Damage of each type is scaled from a specific player stat: (damageScaling * playerStats.relevantStat)\n" +
        "So 1.5 physical scaling and 50 strength is 1.5 * 50 or 75 damage\n\n"
        +"This is calculated for each stat and then added together to get the damage number before we apply resistances.\n" 
        +"Stats can be found in the Player Stats Script/Component.\n"
        +"Resistances can be found in health component, check the health component of attack target.")]
    [SerializeField] DamageStruct damageScaling;
    public DamageStruct GetDamageScaling() { return damageScaling; }

    public DamageStruct CalculatePlayerDamage(PlayerStats playerStats, DamageStruct damageScaling)
    {
        float critMultiplier = criticalHitMultiplier(playerStats.getCriticalChance(), playerStats.getCriticalDamage());

        DamageStruct damageDealt = new DamageStruct();

        //Damage = ((damageScaling.stat * playerStats.relevantStat)
        damageDealt.None = damageScaling.None;
        damageDealt.Physical = (playerStats.getStrength() * damageScaling.Physical);
        damageDealt.Magical = (playerStats.getIntelligence() * damageScaling.Magical);
        damageDealt.True = damageScaling.True;
        damageDealt.Fire = (playerStats.getStrength() * damageScaling.Fire);
        damageDealt.Lightning = (playerStats.getAgility() * damageScaling.Lightning);
        damageDealt.Ice = (playerStats.getIntelligence() * damageScaling.Ice);
        damageDealt.Earth = (playerStats.getStrength() * damageScaling.Earth);
        damageDealt.Wind = (playerStats.getAgility() * damageScaling.Wind);
        damageDealt.Water = (playerStats.getIntelligence() * damageScaling.Water);


        // Apply crit multiplier
        damageDealt = damageDealt * (1f + critMultiplier);

        return damageDealt;
    }

    public DamageStruct CalculatePlayerDamage(PlayerStats playerStats)
    {
        DamageStruct damageDealt = new DamageStruct();
        damageDealt = CalculatePlayerDamage(playerStats, damageScaling);

        return damageDealt;
    }

    float criticalHitMultiplier(float critChance, float critDamage)
    {
        //if crit chance is greater than or equal to 100%, always crit
        if (critChance >= 1f) 
        {
            Debug.Log("Critical Hit!");
            return critDamage;
        }
        //roll for crit
        if (Random.value < critChance)
        {
            Debug.Log("Critical Hit!");
            return critDamage;
        }
        else
        {
            Debug.Log("Normal Hit.");
            return 0f;
        }
    }
}

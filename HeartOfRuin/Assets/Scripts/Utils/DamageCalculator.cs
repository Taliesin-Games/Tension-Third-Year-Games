using UnityEngine;
using UnityEditor;

public class DamageCalculator : EditorWindow
{

    int strength;
    int agility;
    int intelligence;

    float critChance;
    float critMultiplier;


    DamageStruct damageScaling;
    DamageStruct Resistances;
    private bool showDamageNumbers;

    float attacksPerSecond;

    string message;

    [MenuItem("Tools/Damage Calculator")]
    static void ShowWindow()
    {
        GetWindow<DamageCalculator>("Damage Calculator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Damage Calculation Settings", EditorStyles.boldLabel);
        GUILayout.Space(10);
        GUILayout.Label("Player Stats (found on player stats)", EditorStyles.boldLabel);
        strength = EditorGUILayout.IntField("Strength", strength);
        agility = EditorGUILayout.IntField("Agility", agility);
        intelligence = EditorGUILayout.IntField("Intelligence", intelligence);
        GUILayout.Space(10);

        GUILayout.Label("Critical Hit Settings (found on player stats) percentage: 0.1 = 10%", EditorStyles.boldLabel);
        critChance = EditorGUILayout.FloatField("Critical Chance", critChance);
        critMultiplier = EditorGUILayout.FloatField("Critical Multiplier percentage", critMultiplier);

        GUILayout.Space(10);
        damageScaling = DamageStructGUILayout.Draw(damageScaling, "Damage Scaling percentage (found on weapon/skill damage component): 0.1 = 10%");
        Resistances = DamageStructGUILayout.Draw(Resistances, "Target Resistances percentage (found on target health component): 0.1 = 10%");

        GUILayout.Space(10);
        attacksPerSecond = EditorGUILayout.FloatField("Attacks Per Second", attacksPerSecond);
        GUILayout.Space(10);
        GUILayout.Label("OVERALL DAMAGE CALCULATION", EditorStyles.boldLabel);
        GUILayout.Label("For each damage type in damageStruct:");
        GUILayout.Label("Damage = ((damageScaling.stat * playerStats.relevantStat) * (1 + critMultiplier)) * (1 - target.resistance.stat)");
        GUILayout.Label("Damage for each type is then added together to compute the final damage number.");


        if (GUILayout.Button("Compute Damage"))
        {
            showDamageNumbers = true;
            DamageStruct playerDamage = CalculatePlayerDamage(damageScaling);
            DamageStruct adjustedDamage = ApplyResistances(playerDamage, Resistances);
            Vector3 damageNumbers = ComputeDamageVector(adjustedDamage, critChance, critMultiplier);
            message = $"Damage Dealt:\n" +
                $" - Non-Critical: {damageNumbers.x:F2}  DPS: {(damageNumbers.x * attacksPerSecond):F2}\n" +
                $" - Critical: {damageNumbers.y:F2}  DPS: {(damageNumbers.y * attacksPerSecond):F2}\n" +
                $" - Average: {damageNumbers.z:F2}  DPS: {(damageNumbers.z * attacksPerSecond):F2}";

        }

        if (showDamageNumbers)
        {
            GUILayout.Label(message, EditorStyles.boldLabel);
        }
    }

    private void ComputeDamagePreResistance()
    {

    }

    public DamageStruct CalculatePlayerDamage(DamageStruct damageScaling)
    {
        DamageStruct damageDealt = new DamageStruct();

        //Damage = ((damageScaling.stat * playerStats.relevantStat)
        damageDealt.None = damageScaling.None;
        damageDealt.Physical = (strength * damageScaling.Physical);
        damageDealt.Magical = (intelligence * damageScaling.Magical);
        damageDealt.True = damageScaling.True;
        damageDealt.Fire = (strength * damageScaling.Fire);
        damageDealt.Lightning = (agility * damageScaling.Lightning);
        damageDealt.Ice = (intelligence * damageScaling.Ice);
        damageDealt.Earth = (strength * damageScaling.Earth);
        damageDealt.Wind = (agility * damageScaling.Wind);
        damageDealt.Water = (intelligence * damageScaling.Water);


        return damageDealt;
    }

    Vector3 ComputeDamageVector(DamageStruct damage, float critChance, float critMultiplier)
    {
        float nonCrit = (float)damage;
        float crit = (float)damage * (1 + critMultiplier);
        float averageDamage = (float)damage * (1 + critMultiplier * (Mathf.Min(1, critChance)));
        return new Vector3(nonCrit, crit, averageDamage);
    }

    DamageStruct ApplyResistances(DamageStruct damage, DamageStruct resistances)
    {
        // apply resistances
        // incomingDamage * (1 - target.resistance.stat)
        DamageStruct adjustedDamage = damage * (1 - resistances);
        return adjustedDamage;
    }

}

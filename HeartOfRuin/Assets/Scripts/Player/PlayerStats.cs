using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Stats")]
    [Tooltip("Strength stat: Physical, Fire and Earth damage (scalings found on DamageComponent of weapon)")]
    [SerializeField] int strength = 0;
    [Tooltip("Agility stat: Lightning and Wind damage (scalings found on DamageComponent of weapon)")]
    [SerializeField]int agility = 0;
    [Tooltip("Intelligence stat: Magical, Ice and Water damage (scalings found on DamageComponent of weapon)")]
    [SerializeField]int intelligence = 0;

    [Tooltip("Critical hit chance as a percentage, eg 0.1 = 10%")]
    [SerializeField]float criticalChance = 0f; //start at 0%
    [Tooltip("Critical hit damage multiplier as a percentage, eg 0.5 = 50% extra damage.\n"+
        "Bonus damage from critical hits is applied before resistances.")]
    [SerializeField]float criticalDamage = 0.5f; //start at 50% extra damage

    public int getStrength()
    {
        return strength;
    }
    public int getAgility()
    {
        return agility;
    }
    public int getIntelligence()
    {
        return intelligence;
    }

    public float getCriticalChance()
    {
        return criticalChance;
    }
    public float getCriticalDamage()
    {
        return criticalDamage;
    }

    public void setStrength(int value)
    {
        strength = value;
        ApplyModifiers();
    }

    public void setAgility(int value)
    {
        agility = value;
        ApplyModifiers();
    }   

    public void setIntelligence(int value)
    {
        intelligence = value;
        ApplyModifiers();
    }

    public void setCriticalChance(float value)
    {
        criticalChance = value;
        ApplyModifiers();
    }

    public void setCriticalDamage(float value)
    {
        criticalDamage = value;
        ApplyModifiers();
    }

    void ApplyModifiers()
    {

    }
}

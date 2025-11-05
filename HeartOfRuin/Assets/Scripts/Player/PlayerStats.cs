using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField]int strength = 0;
    [SerializeField]int agility = 0;
    [SerializeField]int intelligence = 0;

    [SerializeField]float criticalChance = 0f; //start at 0%
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

    void ApplyModifiers()
    {

    }
}

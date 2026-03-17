using UnityEngine;

[System.Serializable]
public class DpsChannel
{
    [SerializeField] float timeWindow = 5f;
    [SerializeField] DamageStruct value;

    public void UpdateDecay(float dt)
    {
        float decay = Mathf.Exp(-dt / timeWindow);
        value *= decay;
    }

    public void AddDamage(DamageStruct damage)
    {
        value += damage / timeWindow;
    }

    public DamageStruct GetDPS() 
    {
        return value; 
    }
}

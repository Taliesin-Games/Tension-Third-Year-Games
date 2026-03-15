using UnityEngine;

[System.Serializable]
public class DpsChannel
{
    public float window = 5f;
    public DamageStruct value;

    public void UpdateDecay(float dt)
    {
        float decay = Mathf.Exp(-dt / window);
        value *= decay;
    }

    public void AddDamage(DamageStruct damage)
    {
        value += damage / window;
    }
}

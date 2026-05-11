using UnityEngine;

public class DpsTracker : MonoBehaviour
{
    [SerializeField] DpsChannel[] channels;

    void Update()
    {

        for (int i = 0; i < channels.Length; i++)
        {
            channels[i].UpdateDecay(Time.deltaTime);
        }
    }

    public void RecordDamage(DamageStruct damage)
    {
        for (int i = 0; i < channels.Length; i++)
        {
            channels[i].AddDamage(damage);
        }
    }

    public DamageStruct GetDPS(int index)
    {
        if (index < 0 || index >= channels.Length)
            return default;

        return channels[index].GetDPS();
    }
}
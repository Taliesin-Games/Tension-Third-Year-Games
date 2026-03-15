using UnityEngine;

public class DpsTracker : MonoBehaviour
{
    [SerializeField] DpsChannel[] channels;

    float lastUpdateTime;

    void Awake()
    {
        lastUpdateTime = Time.time;
    }

    void Update()
    {
        float now = Time.time;
        float dt = now - lastUpdateTime;
        lastUpdateTime = now;

        for (int i = 0; i < channels.Length; i++)
        {
            channels[i].UpdateDecay(dt);
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

        return channels[index].value;
    }
}
using System.Collections.Generic;
using UnityEngine;

public class DpsTracker : MonoBehaviour
{
    struct DamageEvent
    {
        public float time;
        public DamageStruct damage;
    }

    Queue<DamageEvent> events = new Queue<DamageEvent>();

    float totalDamage = 0f;
    DamageStruct totalDamageByType;

    [SerializeField] float window = 5f;

    public void RecordDamage(DamageStruct damage)
    {
        float now = Time.time;

        events.Enqueue(new DamageEvent
        {
            time = now,
            damage = damage
        });

        totalDamage += (float)damage;
        totalDamageByType += damage;

        Cleanup(now);
    }

    void Update()
    {
        Cleanup(Time.time);
    }

    void Cleanup(float now)
    {
        while (events.Count > 0 && now - events.Peek().time > window)
        {
            var old = events.Dequeue();

            totalDamage -= (float)old.damage;
            totalDamageByType -= old.damage;
        }
    }

    public float GetDPSCombined()
    {
        return totalDamage / window;
    }

    public DamageStruct GetDPSByType()
    {
        return totalDamageByType / window;
    }
}
using UnityEngine;

public class DpsTracker : MonoBehaviour
{
    [SerializeField] float window = 5f;

    DamageStruct currentDPS;
    float lastUpdateTime;

    void Awake()
    {
        lastUpdateTime = Time.time;
    }

    void Update()
    {
        UpdateDecay();
    }

    void UpdateDecay()
    {
        float now = Time.time;
        float dt = now - lastUpdateTime;
        lastUpdateTime = now;

        float decay = Mathf.Exp(-dt / window);

        currentDPS *= decay;
    }

    public void RecordDamage(DamageStruct damage)
    {
        UpdateDecay();

        currentDPS += damage / window;
    }

    public DamageStruct GetDPS()
    {
        UpdateDecay();
        return currentDPS;
    }
}
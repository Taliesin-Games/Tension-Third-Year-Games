using UnityEngine;

public class Boss : Enemy
{
    [SerializeField] private GameObject levelTransitionEffectPrefab;

    SpecialAttackEffect specialAttackEffect;
    protected override void Awake()
    {
        base.Awake();
        specialAttackEffect = GetComponentInChildren<SpecialAttackEffect>();
    }
    protected override void OnDeath()
    { 
        base.OnDeath();
        DropBossPortal();
    }

    void DropBossPortal()
    {
        Debug.Log("Boss is dying. Checking for level transition effect.");
        if (levelTransitionEffectPrefab)
        {
            Debug.Log("Boss defeated! Instantiating level transition effect.");
            Instantiate(levelTransitionEffectPrefab, transform.position, Quaternion.identity);
        }
    }
    
    void AT_Cast_Special()
    {
        if (specialAttackEffect == null) return;

        specialAttackEffect.Play();
    }
}

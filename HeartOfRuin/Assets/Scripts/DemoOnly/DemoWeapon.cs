using UnityEngine;
using System;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(DamageComponent))]
public class DemoWeapon : MonoBehaviour
{
    [SerializeField] Collider weaponCollider;
    [SerializeField] DamageComponent damageComponent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (weaponCollider == null)
        {
            weaponCollider = GetComponent<Collider>();
        }
        if (damageComponent == null)
        {
            damageComponent = GetComponent<DamageComponent>();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Hit object: " + collision.gameObject.name);

        PlayerStats playerStats = GetComponentInParent<DemoPlayer>()?.playerStats;

        DamageStruct damage = damageComponent.CalculatePlayerDamage(playerStats);

        collision.gameObject.GetComponent<Health>()?.TakeDamage(damage);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using System.Runtime.CompilerServices;
using UnityEngine;
[RequireComponent(typeof(Health))]
public class DestructibleObject : MonoBehaviour
{

    [SerializeField] private Health health;
    [SerializeField] private Collider objectCollider;

    private void Awake()
    {
        if (objectCollider == null)
        {
            Debug.LogWarning($"Collider reference for {name} is set in inspector. Attempting to get component.");
            objectCollider = GetComponent<Collider>();
        }

        if (health == null)
        {
            Debug.LogWarning($"Health reference for {name} is set in inspector. Attempting to get component.");
            health = GetComponent<Health>();
        }
    }

    void Die()
    {
        // Play destruction effects here (e.g., particle system, sound)
        Debug.Log($"{gameObject.name} has been destroyed.");
        Destroy(gameObject);
    }
}

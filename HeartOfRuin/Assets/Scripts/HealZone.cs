using UnityEngine;

public class HealZone : MonoBehaviour
{
    [SerializeField] private float healAmount = 10f;
    private bool hasTriggered = false;


    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player") && !hasTriggered)
        {
            if (other.TryGetComponent<Health>(out Health health))
            { 
                health.Heal(healAmount);
                hasTriggered = true;
            }
        }
    }

}

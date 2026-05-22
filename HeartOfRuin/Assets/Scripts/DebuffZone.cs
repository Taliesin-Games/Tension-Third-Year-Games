using UnityEngine;


[RequireComponent(typeof(Collider))]
public class DebuffZone : MonoBehaviour
{

    [SerializeField] private ItemEffect debuffEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (debuffEffect == null) return;

        if (other.CompareTag("Player"))
        {
            Character character = other.GetComponent<Character>();
            if (character != null)
            {
                character.AddItemEffect(debuffEffect);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (debuffEffect == null) return;

        if (other.CompareTag("Player"))
        {
            Character character = other.GetComponent<Character>();
            if (character != null)
            {
                character.RemoveItemEffect(debuffEffect);
            }
        }
    }
}

using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour
{
    public ItemSlot itemSlot;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var inventory = other.GetComponent<Inventory>();
        int quantity = inventory.AddItem(itemSlot);

        if (quantity > 0)
        {
            Destroy(gameObject);
        }
        
    }
}
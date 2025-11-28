using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour
{
    public ItemSlot itemSlot;
    bool itemSet = false;
    GameObject worldRepresentation;
    [SerializeField] float pickupLockoutTimer = 1.2f;

    private void Update()
    {
        if ( !itemSet && itemSlot.GetItem() != null)
        {
            if (itemSlot.GetItem().GetItemMesh() != null)
            {
                worldRepresentation = Instantiate(itemSlot.GetItem().GetItemMesh(), gameObject.transform);
               
            }
            itemSet = true;
        }
        if (itemSet)
        {
            pickupLockoutTimer -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || pickupLockoutTimer > 0) return;

        var inventory = other.GetComponent<Inventory>();
        itemSlot = inventory.AddItem(itemSlot);
        if (itemSlot == null)
        {
            Destroy(gameObject);
        }
        
    }
}
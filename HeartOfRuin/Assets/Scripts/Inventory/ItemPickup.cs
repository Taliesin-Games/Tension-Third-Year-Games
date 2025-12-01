using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour
{
    [SerializeField] ItemSlot itemSlot;
    [SerializeField] float pickupLockoutTimer = 1.2f;
    bool itemSet = false;
    GameObject worldRepresentation;
   

    public ItemSlot ItemSlot => itemSlot;

    private void Update()
    {
        handleItemPickupLogic();
    }

    void handleItemPickupLogic()
    {
        if (!itemSet && itemSlot.GetItem() != null)
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
        if (!other.CompareTag("Player") || pickupLockoutTimer > 0)
        {
            return;
        }

        Inventory inventory = other.GetComponent<Inventory>();
        itemSlot = inventory.AddItem(itemSlot);
        if (itemSlot == null)
        {
            Destroy(gameObject);
        }
        
    }
}
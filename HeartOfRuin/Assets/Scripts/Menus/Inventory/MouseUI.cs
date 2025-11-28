using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NUnit.Framework.Interfaces;

public class Mouse : MonoBehaviour
{
    [SerializeField] GameObject mouseItemUI;
    [SerializeField] Image mouseCursor;
    [SerializeField] Image itemIcon;
    [SerializeField] TextMeshProUGUI qtyText;
    [SerializeField] GameObject itemDropPrefab;

    private Inventory sourceInventory;
    private ItemSlot mouseSlot = new ItemSlot(); // temporary virtual slot

    void Update()
    {
        transform.position = Input.mousePosition;

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            mouseCursor.enabled = false;
            mouseItemUI.SetActive(false);
            return;
        }

        mouseCursor.enabled = true;
        mouseItemUI.SetActive(!IsEmpty());
    }

    //Sets the mouses held item
    public void Set(Inventory inv, ItemSlot slot)
    {
        sourceInventory = inv;

        mouseSlot.Set(slot.GetItem(), slot.GetQuantity());
        RefreshUI();
    }

    public void Clear()
    {
        mouseSlot.Clear();
        RefreshUI();
    }

    public bool IsEmpty() => mouseSlot.IsEmpty();

    public ItemSlot GetItemSlot() => mouseSlot;
    public Inventory GetInventory() => sourceInventory;

    /// <summary>
    /// Refreshes the mouse UI.
    /// </summary>
    public void RefreshUI()
    {
        if (IsEmpty())
        {
            itemIcon.enabled = false;
            qtyText.enabled = false;
            return;
        }

        itemIcon.sprite = mouseSlot.GetItem().GetItemIcon();
        itemIcon.enabled = true;

        qtyText.text = mouseSlot.GetQuantity() > 1 ? mouseSlot.GetQuantity().ToString() : "";
        qtyText.enabled = mouseSlot.GetQuantity() > 1;
    }

    /// <summary>
    /// Drop the item currently held by the mouse object
    /// </summary>
    public void DropHeldItemToWorld()
    {
        if (!IsEmpty() && sourceInventory != null)
        {
            if(itemDropPrefab == null && sourceInventory != null) { itemDropPrefab = sourceInventory.getItemDropPrefab(); }
            if (itemDropPrefab != null)
            {
                GameObject droppedItem = Instantiate(itemDropPrefab);
                droppedItem.transform.position = sourceInventory.gameObject.transform.position;

                var pickup = droppedItem.GetComponent<ItemPickup>();
                if (pickup != null)
                {
                    int dropAmount = mouseSlot.GetQuantity();
                    pickup.itemSlot.Set(mouseSlot.GetItem(), dropAmount);
                }
                else
                {
                    // if prefab is invalid, destroy it
                    Destroy(droppedItem);
                }

                Clear();
            }
        }
    }
}


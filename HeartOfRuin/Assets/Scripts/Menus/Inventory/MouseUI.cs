using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;


public class Mouse : MonoBehaviour
{
    [SerializeField] GameObject mouseItemUI;
    [SerializeField] Image mouseCursor;
    [SerializeField] ItemSlotUI itemSlotUI;

    void Update()
    {
        transform.position = Input.mousePosition;
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            mouseCursor.enabled = false;
            mouseItemUI.SetActive(false);
        }
        else
        {
            mouseCursor.enabled = true;

            if (!itemSlotUI.isEmpty())
            {
                mouseItemUI.SetActive(true);
            }
            else
            {
                mouseItemUI.SetActive(false);
            }
        }
        
    }

    public void Set(Inventory inv, ItemSlot itm)
    {
        itemSlotUI.Set(inv, itm, false);
    }

    public void Clear()
    {
        itemSlotUI.Set(GetInventory(), new ItemSlot(), false);
    }

    public bool HasItem()
    {
        
        if (!itemSlotUI.isEmpty())
        {
            return true;
        }
        
        return false;
    }

    public void DropHeldItemToWorld()
    {
        if (HasItem())
        {
            //return item to inventory
            itemSlotUI.GetInventory().AddItemAtIndex(itemSlotUI.GetItemSlot().GetIndex(), itemSlotUI.GetItemSlot());
            itemSlotUI.GetInventory().DropItem(itemSlotUI.GetItemSlot().GetIndex());
            Clear();
        }
    }

    public ItemSlot GetItemSlot() { return itemSlotUI.GetItemSlot(); }
    public Inventory GetInventory() { return itemSlotUI.GetInventory(); }

}

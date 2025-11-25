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
        if (inv != itemSlotUI.GetInventory())
        {
            Debug.Log("differeint inv");
        }

        if (itm != itemSlotUI.GetItemSlot())
        {
            Debug.Log("different item slot");

        }
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

    public ItemSlot GetItemSlot() { return itemSlotUI.GetItemSlot(); }
    public Inventory GetInventory() { return itemSlotUI.GetInventory(); }

}

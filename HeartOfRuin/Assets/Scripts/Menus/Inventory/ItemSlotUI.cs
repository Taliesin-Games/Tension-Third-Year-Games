using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler, IDropHandler
{
    private Inventory inventory;
    private ItemSlot itemSlot;
    private int IndexInInventory;
    [SerializeField] private Image itemImage;
    [SerializeField] TextMeshProUGUI quantityText;
    [SerializeField] private Mouse mouse;

    private bool click;

    public void Set(Inventory inv , ItemSlot itm, bool writeBackToInventory)
    {
        if(inv == null || itm == null)
        {
            Debug.Log($"inv: {inv == null}, itemSlot: {itm == null}");
            return;
        }

        itemImage.gameObject.SetActive(false);
        quantityText.gameObject.SetActive(false);

        if (writeBackToInventory && inventory != null && itemSlot != null) 
        {
            ItemSlot writeBackSlot = new ItemSlot();
            writeBackSlot.SetIndex(itemSlot.GetIndex());
            writeBackSlot.SetSlotType(itemSlot.GetSlotType());
            inventory.UISlotWriteBack(writeBackSlot); 
        }


        inventory = inv;
        itemSlot = itm;

        if(itm.GetItem() != null)
        {
            itemImage.sprite = itemSlot.GetItem().GetItemIcon();
            itemImage.gameObject.SetActive(true);
        }
        
        if (itemSlot.GetQuantity() > 1)
        {
            quantityText.text = itemSlot.GetQuantity().ToString();
            quantityText.gameObject.SetActive(true);
        }

        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        eventData.pointerPress = this.gameObject;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        click = true;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (click)
        {
            OnClick();
            click = false;
        }
    }
    public void OnDrop(PointerEventData eventData)
    {
        OnClick();
        click = false;
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (click)
        {
            OnClick();
            click = false;
        }
    }


    public void OnClick()
    {
        if (mouse != null) // validate the mouse ref exists
        {
            if (mouse.HasItem()) // check if mouse has item
            {

                //Attempt to add to slot
                ItemSlot tempSlot = inventory.AddItemAtIndex(itemSlot.GetIndex(), mouse.GetItemSlot());
                mouse.Set(mouse.GetInventory(), tempSlot);
                inventory.IndexItemSlots();
            }
            else
            {
                //if mouse empty, set to this item
                mouse.Set(inventory, itemSlot);
                Set(inventory, new ItemSlot(), true);

            }
        }
    }

    public bool isEmpty()
    {
        return (inventory == null || itemSlot == null || itemSlot.IsEmpty());
    }

    public void SetMouse(Mouse inMouse)
    {
        mouse = inMouse;
    }

    public ItemSlot GetItemSlot() { return itemSlot; }
    public Inventory GetInventory() { return inventory; }
}

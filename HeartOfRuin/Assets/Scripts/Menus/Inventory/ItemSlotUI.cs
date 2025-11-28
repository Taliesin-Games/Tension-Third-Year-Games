using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IDragHandler, IDropHandler
{
    private Inventory inventory;
    private Mouse mouse;
    private int slotIndex;

    private bool click;

    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI quantityText;

    // --- Binding ---
    public void Bind(Inventory inv, int index, Mouse mouseRef)
    {
        inventory = inv;
        slotIndex = index;
        mouse = mouseRef;

        Refresh();
    }

    /// <summary>
    /// Refreshes the item slot UI element 
    /// </summary>
    public void Refresh()
    {
        if (inventory == null) return;

        var slot = inventory.GetSlotAtIndex(slotIndex);
        if (slot == null || slot.IsEmpty())
        {
            itemImage.gameObject.SetActive(false);
            quantityText.gameObject.SetActive(false);
            return;
        }

        itemImage.sprite = slot.GetItem().GetItemIcon();
        itemImage.gameObject.SetActive(true);

        if (slot.GetQuantity() > 1)
        {
            quantityText.text = slot.GetQuantity().ToString();
            quantityText.gameObject.SetActive(true);
        }
        else
        {
            quantityText.gameObject.SetActive(false);
        }
    }

    // --- Events ---
    public void OnPointerDown(PointerEventData eventData)
    {
        click = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (click)
        {
            HandleClick();
            click = false;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        HandleClick();
        click = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (click)
        {
            HandleClick();
            click = false;
        }
    }

    private void HandleClick()
    {
        if (inventory == null || mouse == null)
            return;

        var slot = inventory.GetSlotAtIndex(slotIndex);

        if (mouse.IsEmpty())
        {
            // Pick up items
            if (!slot.IsEmpty())
            {
                mouse.Set(inventory, slot);
                slot.Clear();
                inventory.OnRemoveItem?.Invoke(mouse.GetItemSlot().GetItem());
            }
        }
        else
        {
            // Attempt to place mouse-held item
            ItemSlot leftover = inventory.AddItemAtIndex(slotIndex, mouse.GetItemSlot());

            if (leftover != null && leftover.GetQuantity() > 0)
            {
                mouse.Set(mouse.GetInventory(), leftover);
            }
            else
            {
                mouse.Clear();
            }
        }

        Refresh();
        mouse.RefreshUI();
    }

    public bool IsEmpty()
    {
        var slot = inventory?.GetSlotAtIndex(slotIndex);
        return slot == null || slot.IsEmpty();
    }
}

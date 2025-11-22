using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    private Inventory inventory;
    private ItemSlot itemSlot;
    [SerializeField] private Image itemImage;
    [SerializeField] TextMeshProUGUI quantityText;

    public void Set(Inventory inv , ItemSlot itm)
    {
        if(inv == null || itm == null)
        {
            Debug.Log($"inv: {inv == null}, itemSlot: {itm == null}");
            return;
        }
        itemImage.gameObject.SetActive(false);
        quantityText.gameObject.SetActive(false);

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
}

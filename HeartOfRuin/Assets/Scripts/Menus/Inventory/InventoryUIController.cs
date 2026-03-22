using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryUIController : MonoBehaviour
{
    private static InventoryUIController instance;
    public static InventoryUIController Instance 
    {
        get
        {
            if (instance == null)
            {
                var foundObjects = FindObjectsByType<InventoryUIController>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

                foreach (var foundObject in foundObjects)
                {
                    if (foundObject.GetComponentsInChildren<InventoryUIController>(true).Length > 1)
                        instance = foundObject;
                }
            }

            return instance;
        }
    }

    [SerializeField] GameObject gridUI;
    [SerializeField] GameObject itemSlotUIPrefab;
    [SerializeField] Mouse mouse;
    private List<ItemSlotUI> uiSlots = new List<ItemSlotUI>();

    Inventory inventory;


    void Start()
    {
        HideInventory();
    }

    public void ShowInventory()
    {
        RefreshInventoryView();
        gameObject.SetActive(true);
    }

    public void HideInventory()
    {
        gameObject.SetActive(false);
    }

    public void SetInventory(Inventory inv)
    {
        inventory = inv;
        RefreshInventoryView();
    }

    public bool RefreshInventoryView()
    {
        if (inventory == null || gridUI == null)
            return false;

        inventory.ValidateInventorySize();

        uiSlots = gridUI.GetComponentsInChildren<ItemSlotUI>().ToList();

        // Create missing UI slots
        while (uiSlots.Count < inventory.GetInventorySize())
        {
            GameObject slotObj = Instantiate(itemSlotUIPrefab, gridUI.transform);
            uiSlots.Add(slotObj.GetComponent<ItemSlotUI>());
        }

        // Remove extra UI slots
        while (uiSlots.Count > inventory.GetInventorySize())
        {
            Destroy(uiSlots[uiSlots.Count - 1].gameObject);
            uiSlots.RemoveAt(uiSlots.Count - 1);
        }

        // Bind UI slots
        for (int i = 0; i < inventory.GetInventorySize(); i++)
        {
            uiSlots[i].Bind(inventory, i, mouse);
            uiSlots[i].Refresh();
        }

        return true;
    }

    public Mouse GetMouse() => mouse;
}


using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryUIController : MonoBehaviour
{
    [SerializeField] Inventory inventory;
    [SerializeField] GameObject gridUI;
    [SerializeField] GameObject itemSlotUIDisplay;
    private List<ItemSlotUI> existingSlots = new List<ItemSlotUI>();

    bool isVisible = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(inventory == null)
        {
            Debug.Log("Inventory not found, null reference");
        }

        hideInventory();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void displayInventory()
    { 
        gameObject.SetActive(true);
        isVisible = true;
    }

    public void hideInventory() 
    {
        gameObject.SetActive(false); 
        isVisible = false;
    }

    public bool RefreshInventoryView()
    {
        if (inventory == null)
        {
            return false;
        }
        else 
        {
            return RefresherInventoryView(inventory); 
        }

    }

    public bool RefresherInventoryView(Inventory inv) {
        if(inv == null || gridUI == null)
        {
            return false; 
        }

        existingSlots = gridUI.GetComponentsInChildren<ItemSlotUI>().ToList<ItemSlotUI>();

        if (existingSlots.Count < inv.GetInventorySize())
        {
            int amountToCreate = inv.GetInventorySize() - existingSlots.Count;
            for (int i = 0; i < amountToCreate; i++)
            {
                GameObject newSlotUI = Instantiate(itemSlotUIDisplay, gridUI.transform);
                existingSlots.Add(newSlotUI.GetComponent<ItemSlotUI>());
            }
        }

        int index = 0;
        foreach (ItemSlot item in inv.GetInventorySlots())
        {
            existingSlots[index].Set(inv, item);
            index++;
        }
        return true;

    }



}

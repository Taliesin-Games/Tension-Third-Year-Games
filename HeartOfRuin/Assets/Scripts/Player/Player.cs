using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using static UnityEditor.Experimental.GraphView.Port;
using static UnityEditor.Profiling.HierarchyFrameDataView;


[RequireComponent(typeof(Animator))] // Ensure that an Animator component is attached
[RequireComponent(typeof(BMD.PlayerController))] // Ensure that a CharacterController component is attached

public class Player : Character
{
    [SerializeField] GameObject inventoryUI;
    [SerializeField] GameObject equipmentUI;
    InventoryUIController inventoryUiController;
    InventoryUIController equipmentUiController;

    bool invToggle;

    private void Start()
    {
        
        if (inventoryUI != null) 
        {
            inventoryUiController = inventoryUI.GetComponent<InventoryUIController>();
            inventoryUiController.SetInventory(inventory);
        }
        if (equipmentUI != null)
        {
            equipmentUiController = equipmentUI.GetComponent<InventoryUIController>();
            equipmentUiController.SetInventory(equipmentSlots);
        
        }

        RefreshUIView();
    }

    private void Update()
    {
        if (inventoryUI != null && equipmentUI != null  && Input.GetKeyDown(KeyCode.Tab)){
            invToggle = !invToggle;
            if (invToggle) 
            {
                inventoryUiController.displayInventory();
                equipmentUiController.displayInventory();
            }
            else 
            { 
                inventoryUiController.hideInventory();
                equipmentUiController.hideInventory();
            }
        }

        if (invToggle && Input.GetKeyDown(KeyCode.V))
        {
            inventoryUiController.GetMouse().DropHeldItemToWorld();
        }

        RefreshUIView();
            
    }

    private void RefreshUIView()
    {
        RefreshInvenotryView();
    }

    private void RefreshInvenotryView()
    {
        if (!invToggle || inventoryUI == null || inventoryUiController == null) { return; }
        if (equipmentUI == null || equipmentUiController == null) { return ; }
        
        inventoryUiController.RefreshInventoryView();
        equipmentUiController.RefreshInventoryView();
    }
}

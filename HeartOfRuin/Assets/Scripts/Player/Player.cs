using System;
using System.Buffers;
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
            Debug.Log($"Setting main inventory to {inventoryUiController.name}");
        }
        if (equipmentUI != null)
        {
            equipmentUiController = equipmentUI.GetComponent<InventoryUIController>();
            equipmentUiController.SetInventory(equipmentSlots);
            Debug.Log($"Setting main inventory to {equipmentUiController.name}");

        }
    }

    private void Update()
    {
        if (inventoryUI != null && equipmentUI != null && Input.GetKeyDown(KeyCode.Tab))
        {
            invToggle = !invToggle; //flip the toggle
            Debug.Log($"toggling inventory to: {invToggle}");

        }

        if (invToggle)
        {        
            if (Input.GetKeyDown(KeyCode.V))
            {
                inventoryUiController.GetMouse().DropHeldItemToWorld();
                Debug.Log("drop key pressed");
            }
            inventoryUiController.ShowInventory();
            equipmentUiController.ShowInventory();
        }
        else
        {

            inventoryUiController.HideInventory();
            equipmentUiController.HideInventory();
        }
            
    }

}

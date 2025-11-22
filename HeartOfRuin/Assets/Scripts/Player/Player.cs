using System;
using System.Collections;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.Port;
using static UnityEditor.Profiling.HierarchyFrameDataView;


[RequireComponent(typeof(Animator))] // Ensure that an Animator component is attached
[RequireComponent(typeof(BMD.PlayerController))] // Ensure that a CharacterController component is attached
public class Player : Character
{
    [SerializeField] GameObject inventoryUI;
    InventoryUIController inventoryUiController;

    bool invToggle;

    private void Awake()
    {
        if (inventoryUI != null )
        {
            inventoryUiController = inventoryUI.GetComponent<InventoryUIController>();
        }
        else { Debug.Log("null inventory ui ref"); }
    }

    private void Update()
    {
        if (inventoryUI != null && Input.GetKeyDown(KeyCode.Tab)){
            invToggle = !invToggle;
            if (invToggle) { inventoryUiController.displayInventory() ; }
            else {  inventoryUiController.hideInventory() ; }
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

        inventoryUiController.RefreshInventoryView();
    }
}

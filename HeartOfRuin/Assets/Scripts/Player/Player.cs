using UnityEngine;



[RequireComponent(typeof(Animator))] // Ensure that an Animator component is attached
[RequireComponent(typeof(BMD.PlayerController))] // Ensure that a CharacterController component is attached
[RequireComponent(typeof(Health))]
public class Player : Character
{
    [SerializeField] GameObject inventoryUI;
    [SerializeField] GameObject equipmentUI;
    [SerializeField] SpellCaster castComponent;
    InventoryUIController inventoryUiController;
    InventoryUIController equipmentUiController;
    bool invToggle;


    private void Start()
    {
        initialiseUIControllerVariables();
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

        
        if( castComponent != null)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                castComponent.TryCastSpell(0);
                Debug.Log("Cast Spell 1");
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                castComponent.TryCastSpell(1);
                Debug.Log("Cast Spell 2");
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                castComponent.TryCastSpell(2);
                Debug.Log("Cast Spell 3");
            }
        }   
    }

    void initialiseUIControllerVariables()
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

}

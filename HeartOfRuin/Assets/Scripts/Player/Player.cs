using UnityEngine;


[RequireComponent(typeof(Animator))] // Ensure that an Animator component is attached
[RequireComponent(typeof(BMD.PlayerController))] // Ensure that a CharacterController component is attached
[RequireComponent(typeof(Health))]
public class Player : Character
{
    public static Player Instance;

    [SerializeField] PlayerHUD playerHUD;

    InventoryUIController inventoryUiController;
    InventoryUIController equipmentUiController;
    bool invToggle;

    protected override void Awake()
    {
        Instance = this;
        base.Awake();
    }

    private void Start()
    {
        InitialiseUIControllerVariables();
    }

    private void Update()
    {
        if (inventoryUiController != null && equipmentUiController != null && Input.GetKeyDown(KeyCode.Tab))
        {
            invToggle = !invToggle; //flip the toggle
            Debug.Log($"toggling inventory to: {invToggle}");

        }

        if (invToggle)
        {        
            if (Input.GetKeyDown(KeyCode.V))    // TODO convert to new input system
            {
                inventoryUiController.GetMouse().DropHeldItemToWorld();
                Debug.Log("drop key pressed");
            }
            inventoryUiController.ShowInventory();
            equipmentUiController.ShowInventory();
            HUDOffsetController.Instance.SetOffsetEnabled(true);
        }
        else
        {

            inventoryUiController.HideInventory();
            equipmentUiController.HideInventory();
            HUDOffsetController.Instance.SetOffsetEnabled(false);
        }

    }

    void InitialiseUIControllerVariables()
    {

        inventoryUiController = InventoryUIController.Instance;
        inventoryUiController.SetInventory(inventory);

        var controllers = inventoryUiController.GetComponentsInChildren<InventoryUIController>(true);

        foreach (var controller in controllers)
        {
            if (controller != inventoryUiController)
            {
                equipmentUiController = controller;
                break;
            }
        }

        if(equipmentUiController != null) equipmentUiController.SetInventory(equipmentSlots);

    }

}

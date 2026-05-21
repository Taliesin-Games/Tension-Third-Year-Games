using BMD;
using UnityEngine;
using UnityEngine.InputSystem;


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

    PlayerControls playerControls;
    PlayerControls.PlayerActions playerActionMap;
    PlayerControls.UIActions uiActionMap;
    InputAction inventoryTogglePlayer;
    InputAction inventoryToggleUI;
    InputAction dropItemAction;


    protected override void OnEnable()
    {
        base.OnEnable();
        playerControls.Enable();
        inventoryTogglePlayer.performed += ctx => ToggleInventory();
        inventoryToggleUI.performed += ctx => ToggleInventory();
        dropItemAction.performed += ctx => DropItem();


    }

    protected override void OnDisable()
    {
        base.OnDisable();
        playerControls.Disable();
        inventoryTogglePlayer.performed -= ctx => ToggleInventory();
        inventoryToggleUI.performed -= ctx => ToggleInventory();
        dropItemAction.performed -= ctx => DropItem();
    }


    protected override void Awake()
    {
        Instance = this;
        base.Awake();
        playerControls = new PlayerControls();
        playerActionMap = playerControls.Player;
        uiActionMap = playerControls.UI;

        inventoryToggleUI = uiActionMap.InventoryToggle;

        inventoryTogglePlayer = playerActionMap.InventoryToggle;

        dropItemAction = uiActionMap.DropItem;

        uiActionMap.Disable(); // start with UI action map disabled

    }

    private void Start()
    {
        base.Start();
        InitialiseUIControllerVariables();
    }

    private void Update()
    {

        if (invToggle)
        {   
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


    void DropItem()
    {
        if (inventoryUiController != null)
        {
            inventoryUiController.GetMouse().DropHeldItemToWorld();
            Debug.Log("drop key pressed");
        }
    }


    void ToggleInventory()
    {

        if (playerActionMap.enabled)
        {
            playerActionMap.Disable();
            uiActionMap.Enable();
        }
        else       
        {
            playerActionMap.Enable();
            uiActionMap.Disable();
        }


        if (inventoryUiController != null && equipmentUiController != null)
        {
            invToggle = !invToggle; //flip the toggle
            Debug.Log($"toggling inventory to: {invToggle}");

        }

        if (invToggle)
        {
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

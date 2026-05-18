using BMD;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;


//TODO: Setup as actual character module rather than just using Input.GetKeyDown()
public class PlayerInteractor : CharacterModule
{
    [SerializeField] float interactDistance = 3f;
    [SerializeField] LayerMask interactMask;
    PlayerControls playerControls;
    InputAction interact;
    IInteractableObject currentTarget;

    public override void PreInitialize(BMD.CharacterController controller) { }
    public override void Initialize(BMD.CharacterController controller) { }
    public override void  Tick(float deltaTime) { }
    public override void FixedTick(float fixedDeltaTime) { }
    public override void Dispose() { }

    private void Awake()
    {
        SetupControls();
    }

    private void SetupControls()
    {
        playerControls = new PlayerControls();
        interact = playerControls.Player.Interact;
    }

    private void OnEnable()
    {
        playerControls.Player.Enable();
        interact.performed += ctx => Interact();
    }
    private void OnDisable()
    {
        playerControls.Player.Disable();
    }

    void Interact()
    {
        if (currentTarget != null)
        {
            if (currentTarget.CanInteract)
                currentTarget.Interact();
        }
    }

    public void SetCurrentTarget(IInteractableObject target)
    {
        if (target == null)
        {
            return;
        }
        currentTarget = target;

    }

}


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

    void Update()
    {
        HandleRaycast();
    }

    private void HandleRaycast()
    {
        Vector3 pos = gameObject.transform.position;
        Vector3 dir = gameObject.transform.forward;
        Ray ray = new Ray(pos, dir);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask))
        {
            Debug.DrawRay(pos, dir * hit.distance, Color.green);
            if (hit.collider.TryGetComponent(out IInteractableObject interactable))
            {
                if (interactable != currentTarget)
                {
                    currentTarget?.OnLoseFocus();
                    currentTarget = interactable;
                    currentTarget.OnFocus();
                }
                return;
            }
        }
        else
        {
            Debug.DrawRay(pos, dir * interactDistance, Color.red);
        }

        // No hit or no interactable, clear focus
        if (currentTarget != null)
        {
            currentTarget.OnLoseFocus();
            currentTarget = null;
        }
    }
}


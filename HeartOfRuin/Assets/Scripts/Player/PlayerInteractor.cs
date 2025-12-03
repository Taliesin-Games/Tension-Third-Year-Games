using UnityEngine;
using UnityEngine.InputSystem;


//TODO: Setup as actual character module rather than just using Input.GetKeyDown()
public class PlayerInteractor : MonoBehaviour, ICharacterModule
{
    [SerializeField] float interactDistance = 3f;
    [SerializeField] LayerMask interactMask;

    IInteractableObject currentTarget;


    public void Initialize(BMD.CharacterController controller) { }
    public void Tick(float deltaTime) { }
    public void FixedTick(float fixedDeltaTime) { }
    public void Dispose() { }


    void Update()
    {
        HandleRaycast();

        if (currentTarget != null && Input.GetKeyDown(KeyCode.E))
        {
            if (currentTarget.CanInteract)
                currentTarget.Interact();
        }
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


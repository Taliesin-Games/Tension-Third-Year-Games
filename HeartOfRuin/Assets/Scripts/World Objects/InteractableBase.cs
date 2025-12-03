using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class InteractableBase : MonoBehaviour, IInteractableObject
{
    [Header("Interactable Settings")]
    [SerializeField] protected string displayName = "Interact";
    [SerializeField] protected bool canInteract = true;

    public string DisplayName => displayName;
    public bool CanInteract => canInteract;

    public virtual void OnFocus() { }
    public virtual void OnLoseFocus() { }
    public abstract void Interact();
}


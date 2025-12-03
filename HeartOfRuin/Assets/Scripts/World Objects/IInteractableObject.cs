using UnityEngine;

public interface IInteractableObject
{
    string DisplayName { get; }   // Optional
    bool CanInteract { get; }     // Allows enabling/disabling the interaction dynamically

    void OnFocus();               // Called when player looks at it
    void OnLoseFocus();           // Called when player looks away
    void Interact();              // Perform the interaction
}

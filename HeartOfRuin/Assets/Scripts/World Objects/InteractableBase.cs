using System;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class InteractableBase : MonoBehaviour, IInteractableObject
{
    [Header("Interactable Settings")]
    [SerializeField] protected string displayName = "Interact";
    [SerializeField] protected bool canInteract = true;
    [SerializeField] protected GameObject interactionPromptPrefab;
    [SerializeField] protected float interactionPromptHeightOffset = 3.0f;

    InteractPrompt cachedInteractionPrompt;

    public void Awake()
    {
        // Ensure the collider is set to trigger for interaction purposes
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (canInteract && other.CompareTag("Player"))
        {
            other.GetComponent<PlayerInteractor>()?.SetCurrentTarget(this);
            OnFocus();
            if(interactionPromptPrefab != null) {
                GameObject tempInteractionPrompt = Instantiate(interactionPromptPrefab, transform.position + Vector3.up * interactionPromptHeightOffset, transform.rotation);
                cachedInteractionPrompt = tempInteractionPrompt.GetComponent<InteractPrompt>();
                if (cachedInteractionPrompt != null)
                {
                    cachedInteractionPrompt.Initialize();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerInteractor>()?.SetCurrentTarget(null);
            OnLoseFocus();

            if(cachedInteractionPrompt != null) 
            {
                Destroy(cachedInteractionPrompt.gameObject);
            }
        }
    }

    public string DisplayName => displayName;
    public bool CanInteract => canInteract;

    public virtual void OnFocus() { }
    public virtual void OnLoseFocus() { }
    public abstract void Interact();
}


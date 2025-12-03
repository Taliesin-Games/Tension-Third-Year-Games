using UnityEngine;

public class LevelChangeInteractable : InteractableBase
{
    public override void Interact()
    {
        Debug.Log("Portal activated");
        //transition level to next...
        
    }

    public override void OnFocus()
    {
        Debug.Log("Looking at portal");
    }

    public override void OnLoseFocus()
    {
        Debug.Log("Stopped looking at portal");
    }
}

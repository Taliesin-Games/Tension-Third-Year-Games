using UnityEditor;
using UnityEngine;

public class LevelChangeInteractable : InteractableBase
{
    [SerializeField] SceneAsset nextLevel;
    public override void Interact()
    {
        LevelManager.LoadNextLevel();
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

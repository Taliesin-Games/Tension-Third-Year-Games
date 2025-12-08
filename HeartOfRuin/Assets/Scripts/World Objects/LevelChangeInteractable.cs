using UnityEditor;
using UnityEngine;

public class LevelChangeInteractable : InteractableBase
{

    //[SerializeField] SceneAsset nextLevel;  // TODO does not work in builds, need to rework
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

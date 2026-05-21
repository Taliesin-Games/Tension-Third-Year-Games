using UnityEngine;

public class Readme : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField, TextArea(8, 20)]
    string text = "Add object notes here.";

    public string Text => text;

    private void Reset()
    {
        ReadmeEditorUtility.MoveReadmeBelowTransform(this);
    }

    private void OnValidate()
    {
        ReadmeEditorUtility.MoveReadmeBelowTransform(this);
    }
#endif
}
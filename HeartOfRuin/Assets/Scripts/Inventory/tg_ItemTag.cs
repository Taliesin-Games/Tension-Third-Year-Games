using System;
using UnityEngine;

public class tg_ItemTag : ScriptableObject
{
    [Tooltip("Human readable tag name")]
    [SerializeField] string tagName;

    [Tooltip("Stable identifier for the tag (GUID)")]
    [SerializeField] string id;

    [Tooltip("Optional colour for UI display")]
    [SerializeField] Color color = Color.white;

    [Tooltip("Optional icon to show where tags are displayed")]
    [SerializeField] Sprite icon;

    [Tooltip("Optional description for the tag")]
    [TextArea]
    [SerializeField] string description;

    public string GetName() => tagName ?? string.Empty;
    public string GetID() => id ?? string.Empty;
    public Color GetColor() => color;
    public Sprite GetIcon() => icon;
    public string GetDescription() => description ?? string.Empty;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            id = Guid.NewGuid().ToString("N");
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
        if (string.IsNullOrWhiteSpace(tagName))
        {
            // default tag name from asset name if empty
            tagName = name;
        }
    }
}
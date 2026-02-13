using System;
using UnityEditor;
using UnityEngine;


[Serializable]
public struct DropRateEntry
{
    [HideInInspector][SerializeField] private ItemRarity rarity; // hidden so user can't change it directly
    [SerializeField] private float rate;

    // read-only accessors for other code / editor display
    public ItemRarity Rarity => rarity;
    public float Rate { get => rate; set => rate = value; }

    internal void SetRarity(ItemRarity r) => rarity = r;
}

[Serializable]
public class DropRates 
{
    
    [SerializeField] 
    private DropRateEntry[] entries;

    public float GetDropRate(ItemRarity rarity)
    {
        if (entries == null) return 0f;
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].Rarity == rarity) return entries[i].Rate;
        }
        return 0f;
    }

    // Guarantee array length matches enum and each entry's rarity is fixed
    public void EnsureDropRates()
    {
        var values = Enum.GetValues(typeof(ItemRarity));
        int count = values.Length;

        if (entries == null || entries.Length != count)
            Array.Resize(ref entries, count);

        for (int i = 0; i < count; i++)
        {
            var entry = entries[i];
            var expectedRarity = (ItemRarity)values.GetValue(i);
            // set rarity (struct copy) to guarantee the correct enum is stored and cannot be edited via inspector
            entry.SetRarity(expectedRarity);
            entries[i] = entry;
            entries[i].Rate = Mathf.Clamp01(entries[i].Rate); // ensure rates are between 0 and 1
        }
    }

}


#if UNITY_EDITOR

// Custom drawer to display each DropRateEntry's float field labeled with its rarity name.
// The rarity field remains hidden for editing but is read to produce a clear label.
[CustomPropertyDrawer(typeof(DropRateEntry))]
public class DropRateEntryDrawer : PropertyDrawer
{
    private const string kTooltip = "Drop rate between 0 and 1 (1 = 100%).";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Find the hidden rarity and the visible rate properties
        var rarityProp = property.FindPropertyRelative("rarity");
        var rateProp = property.FindPropertyRelative("rate");

        // Build label text from the enum value if possible
        string rarityLabel = label.text;
        if (rarityProp != null)
        {
            try
            {
                var enumIndex = rarityProp.enumValueIndex;
                var enumNames = Enum.GetNames(typeof(ItemRarity));
                if (enumIndex >= 0 && enumIndex < enumNames.Length)
                    rarityLabel = enumNames[enumIndex];
                else
                    rarityLabel = ((ItemRarity)enumIndex).ToString();
            }
            catch
            {
                // fallback to default label
                rarityLabel = label.text;
            }
        }

        EditorGUI.BeginProperty(position, label, property);
        // Draw single-line float field with rarity label and tooltip explaining expected range
        var content = new GUIContent(rarityLabel, kTooltip);
        EditorGUI.PropertyField(position, rateProp, content);
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}
#endif
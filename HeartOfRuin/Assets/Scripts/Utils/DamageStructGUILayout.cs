using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class DamageStructGUILayout
{
    public static DamageStruct Draw(DamageStruct damage, string label = "DamageStruct")
    {
#if UNITY_EDITOR
        // Use EditorGUILayout in editor for labels and layout niceties
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        damage.None = EditorGUILayout.FloatField("None", damage.None);
        damage.Physical = EditorGUILayout.FloatField("Physical", damage.Physical);
        damage.Magical = EditorGUILayout.FloatField("Magical", damage.Magical);
        damage.True = EditorGUILayout.FloatField("True", damage.True);
        damage.Fire = EditorGUILayout.FloatField("Fire", damage.Fire);
        damage.Lightning = EditorGUILayout.FloatField("Lightning", damage.Lightning);
        damage.Ice = EditorGUILayout.FloatField("Ice", damage.Ice);
        damage.Earth = EditorGUILayout.FloatField("Earth", damage.Earth);
        damage.Wind = EditorGUILayout.FloatField("Wind", damage.Wind);
        damage.Water = EditorGUILayout.FloatField("Water", damage.Water);

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
#else
        // Runtime fallback using GUILayout
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(label, GUI.skin.label);

        damage.None       = FloatField("None", damage.None);
        damage.Physical   = FloatField("Physical", damage.Physical);
        damage.Magical    = FloatField("Magical", damage.Magical);
        damage.True       = FloatField("True", damage.True);
        damage.Fire       = FloatField("Fire", damage.Fire);
        damage.Lightning  = FloatField("Lightning", damage.Lightning);
        damage.Ice        = FloatField("Ice", damage.Ice);
        damage.Earth      = FloatField("Earth", damage.Earth);
        damage.Wind       = FloatField("Wind", damage.Wind);
        damage.Water      = FloatField("Water", damage.Water);

        GUILayout.EndVertical();
#endif
        return damage;
    }

#if !UNITY_EDITOR
    private static float FloatField(string label, float value)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(80));
        string input = GUILayout.TextField(value.ToString(), GUILayout.Width(60));
        float.TryParse(input, out value);
        GUILayout.EndHorizontal();
        return value;
    }
#endif
}

// TODO split this into editor/runtime/shared
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Text;
using System.IO;

public partial class ItemDatabaseWindow
{
    /// <summary>
    /// Helper for Drawing a read-only display of a DamageStruct property.
    /// </summary>
    /// <param name="structProp"></param>
    private void DrawDamageStructDisplay(SerializedProperty structProp)
	{
		if (structProp == null) return;

		SerializedProperty pNone = structProp.FindPropertyRelative("None");
		SerializedProperty pPhysical = structProp.FindPropertyRelative("Physical");
		SerializedProperty pMagical = structProp.FindPropertyRelative("Magical");
		SerializedProperty pTrue = structProp.FindPropertyRelative("True");
		SerializedProperty pFire = structProp.FindPropertyRelative("Fire");
		SerializedProperty pLightning = structProp.FindPropertyRelative("Lightning");
		SerializedProperty pIce = structProp.FindPropertyRelative("Ice");
		SerializedProperty pEarth = structProp.FindPropertyRelative("Earth");
		SerializedProperty pWind = structProp.FindPropertyRelative("Wind");
		SerializedProperty pWater = structProp.FindPropertyRelative("Water");

		EditorGUILayout.BeginVertical("box");
		if (pNone != null) EditorGUILayout.LabelField("None", pNone.floatValue.ToString());
		if (pPhysical != null) EditorGUILayout.LabelField("Physical", pPhysical.floatValue.ToString());
		if (pMagical != null) EditorGUILayout.LabelField("Magical", pMagical.floatValue.ToString());
		if (pTrue != null) EditorGUILayout.LabelField("True", pTrue.floatValue.ToString());
		if (pFire != null) EditorGUILayout.LabelField("Fire", pFire.floatValue.ToString());
		if (pLightning != null) EditorGUILayout.LabelField("Lightning", pLightning.floatValue.ToString());
		if (pIce != null) EditorGUILayout.LabelField("Ice", pIce.floatValue.ToString());
		if (pEarth != null) EditorGUILayout.LabelField("Earth", pEarth.floatValue.ToString());
		if (pWind != null) EditorGUILayout.LabelField("Wind", pWind.floatValue.ToString());
		if (pWater != null) EditorGUILayout.LabelField("Water", pWater.floatValue.ToString());
		EditorGUILayout.EndVertical();
	}

    /// <summary>
    /// Draws editable fields for a DamageStruct. This is used when creating or editing an item, allowing the user to input values for each damage type.
    /// </summary>
    private void DrawDamageStructFields(ref DamageStruct structField)
	{
		structField.None = EditorGUILayout.FloatField(new GUIContent("None", kDamageStructTooltip), structField.None);
		structField.Physical = EditorGUILayout.FloatField(new GUIContent("Physical", kDamageStructTooltip), structField.Physical);
		structField.Magical = EditorGUILayout.FloatField(new GUIContent("Magical", kDamageStructTooltip), structField.Magical);
		structField.True = EditorGUILayout.FloatField(new GUIContent("True", kDamageStructTooltip), structField.True);
		structField.Fire = EditorGUILayout.FloatField(new GUIContent("Fire", kDamageStructTooltip), structField.Fire);
		structField.Lightning = EditorGUILayout.FloatField(new GUIContent("Lightning", kDamageStructTooltip), structField.Lightning);
		structField.Ice = EditorGUILayout.FloatField(new GUIContent("Ice", kDamageStructTooltip), structField.Ice);
		structField.Earth = EditorGUILayout.FloatField(new GUIContent("Earth", kDamageStructTooltip), structField.Earth);
		structField.Wind = EditorGUILayout.FloatField(new GUIContent("Wind", kDamageStructTooltip), structField.Wind);
		structField.Water = EditorGUILayout.FloatField(new GUIContent("Water", kDamageStructTooltip), structField.Water);
	}

	/// <summary>
	/// Sets the numeric bonus properties on the specified serialized object for equippable item attributes.
	/// </summary>
	private void SetEquippableNumericProps(SerializedObject so)
	{
		SerializedProperty bonusStrProp = so.FindProperty("BonusStrength");
		if (bonusStrProp != null) bonusStrProp.intValue = bonusStrength;

		SerializedProperty bonusAgiProp = so.FindProperty("BonusAgility");
		if (bonusAgiProp != null) bonusAgiProp.intValue = bonusAgility;

		SerializedProperty bonusIntProp = so.FindProperty("BonusIntelligence");
		if (bonusIntProp != null) bonusIntProp.intValue = bonusIntelligence;

		SerializedProperty critChanceProp = so.FindProperty("BonusCriticalChance");
		if (critChanceProp != null) critChanceProp.floatValue = bonusCriticalChance;

		SerializedProperty critDmgProp = so.FindProperty("BonusCriticalDamage");
		if (critDmgProp != null) critDmgProp.floatValue = bonusCriticalDamage;
	}


	/// <summary>
	/// Sets the values of a serialized damage property to match the fields of the specified DamageStruct.
	/// </summary>
	private void SetDamageStructToProperty(SerializedObject so, string propertyName, DamageStruct source)
	{
		SerializedProperty prop = so.FindProperty(propertyName);
		if (prop == null) return;

		SerializedProperty pNone = prop.FindPropertyRelative("None");          if (pNone != null) pNone.floatValue = source.None;
		SerializedProperty pPhysical = prop.FindPropertyRelative("Physical");  if (pPhysical != null) pPhysical.floatValue = source.Physical;
		SerializedProperty pMagical = prop.FindPropertyRelative("Magical");    if (pMagical != null) pMagical.floatValue = source.Magical;
		SerializedProperty pTrue = prop.FindPropertyRelative("True");          if (pTrue != null) pTrue.floatValue = source.True;
		SerializedProperty pFire = prop.FindPropertyRelative("Fire");          if (pFire != null) pFire.floatValue = source.Fire;
		SerializedProperty pLightning = prop.FindPropertyRelative("Lightning");if (pLightning != null) pLightning.floatValue = source.Lightning;
		SerializedProperty pIce = prop.FindPropertyRelative("Ice");            if (pIce != null) pIce.floatValue = source.Ice;
		SerializedProperty pEarth = prop.FindPropertyRelative("Earth");        if (pEarth != null) pEarth.floatValue = source.Earth;
		SerializedProperty pWind = prop.FindPropertyRelative("Wind");          if (pWind != null) pWind.floatValue = source.Wind;
		SerializedProperty pWater = prop.FindPropertyRelative("Water");        if (pWater != null) pWater.floatValue = source.Water;
	}

	/// <summary>
	/// Ensures that the specified folder exists within the Unity Assets directory and returns a unique asset path for a
	/// file with the given name.
	/// </summary>
	private string EnsureFolderAndGetUniquePath(string folderPath, string fileNameWithoutExtension)
	{
		if (string.IsNullOrWhiteSpace(fileNameWithoutExtension))
		{
			fileNameWithoutExtension = "NewItem";
		}


		folderPath = folderPath.Replace('\\', '/').TrimEnd('/');
		if (!folderPath.StartsWith("Assets"))
		{
			folderPath = Path.Combine("Assets", folderPath).Replace('\\', '/');
		}


		string[] parts = folderPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
		string current = parts.Length > 0 ? parts[0] : "Assets";
		for (int i = 1; i < parts.Length; i++)
		{
			string next = $"{current}/{parts[i]}";
			if (!AssetDatabase.IsValidFolder(next))
			{
				AssetDatabase.CreateFolder(current, parts[i]);
			}
			current = next;
		}

		string candidate = $"{folderPath}/{fileNameWithoutExtension}.asset";
		string unique = AssetDatabase.GenerateUniqueAssetPath(candidate);
		return unique;
	}

	/// <summary>
	/// Returns a sanitized version of the specified file name by removing invalid characters and replacing directory separators.
	/// </summary>
	private string SanitizeFileName(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			return "NewItem";
		}


		char[] invalid = Path.GetInvalidFileNameChars();
		StringBuilder sb = new StringBuilder();
		foreach (char c in name)
		{
			if (Array.IndexOf(invalid, c) >= 0)
			{
				continue;
			}
			sb.Append(c);
		}

		string result = sb.ToString().Trim();
		if (string.IsNullOrEmpty(result))
		{
			return "NewItem";
		}
		result = result.Replace("/", "_").Replace("\\", "_");
		return result;
	}
}
#endif
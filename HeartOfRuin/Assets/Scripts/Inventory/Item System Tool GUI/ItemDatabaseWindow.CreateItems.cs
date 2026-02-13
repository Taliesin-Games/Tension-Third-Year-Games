using UnityEditor;
using UnityEngine;

public partial class ItemDatabaseWindow
{
	private void CreateBaseItem()
	{
		Item item = CreateInstance<Item>();
		SerializedObject so = new SerializedObject(item);

		so.FindProperty("id").stringValue = ItemIDGenerator.GenerateID();
		so.FindProperty("itemName").stringValue = itemName;
		so.FindProperty("itemDescription").stringValue = itemDescription;
		so.FindProperty("itemIcon").objectReferenceValue = itemIcon;
		so.FindProperty("itemMesh").objectReferenceValue = itemMesh;
		so.FindProperty("maxStackSize").intValue = Mathf.Max(1, maxStackSize);

		SerializedProperty pR = so.FindProperty("rarity");
		if (pR != null) 
		{
			pR.enumValueIndex = (int)itemRarity; 
		}

		SerializedProperty tagsProp = so.FindProperty("tags");
		if (tagsProp != null)
		{
			tagsProp.arraySize = tagsWindow.Count;
			for (int i = 0; i < tagsWindow.Count; i++)
			{
				tagsProp.GetArrayElementAtIndex(i).objectReferenceValue = tagsWindow[i];
			}

		}

		so.ApplyModifiedProperties();

		string folder = "Assets/InGameItems";
		string uniquePath = EnsureFolderAndGetUniquePath(folder, SanitizeFileName(itemName));
		if (string.IsNullOrEmpty(uniquePath))
		{
			EditorUtility.DisplayDialog("Create Item", "Could not determine a valid asset path.", "OK");
			return;
		}

		AssetDatabase.CreateAsset(item, uniquePath);
		database.items.Add(item);
		Selection.activeObject = item;
	}

	private void CreateArmour()
	{
		Armour armour = CreateInstance<Armour>();
		SerializedObject so = new SerializedObject(armour);

		so.FindProperty("id").stringValue = ItemIDGenerator.GenerateID();
		so.FindProperty("itemName").stringValue = itemName;
		so.FindProperty("itemDescription").stringValue = itemDescription;
		so.FindProperty("itemIcon").objectReferenceValue = itemIcon;
		so.FindProperty("itemMesh").objectReferenceValue = itemMesh;
		so.FindProperty("maxStackSize").intValue = Mathf.Max(1, maxStackSize);

		SerializedProperty pR = so.FindProperty("rarity");
		if (pR != null) 
		{
			pR.enumValueIndex = (int)itemRarity; 
		}

		SerializedProperty tagsProp = so.FindProperty("tags");
		if (tagsProp != null)
		{
			tagsProp.arraySize = tagsWindow.Count;
			for (int i = 0; i < tagsWindow.Count; i++) {
				tagsProp.GetArrayElementAtIndex(i).objectReferenceValue = tagsWindow[i];
			}

		}

		SerializedProperty equipProp = so.FindProperty("equipSlotType");
		if (equipProp != null)
		{
			equipProp.enumValueIndex = (int)equipSlotType;
		}
		SetEquippableNumericProps(so);

		SerializedProperty effectsProp = so.FindProperty("itemEffects");
		if (effectsProp != null)
		{
			effectsProp.arraySize = itemEffectsWindow.Count;
			for (int i = 0; i < itemEffectsWindow.Count; i++)
			{
				effectsProp.GetArrayElementAtIndex(i).objectReferenceValue = itemEffectsWindow[i];

			}
		}
		SetDamageStructToProperty(so, "damageBonusPercentages", damageBonusPercentagesWindow);

		so.ApplyModifiedProperties();

		string folder = "Assets/InGameItems/Equipment/Armour";
		string uniquePath = EnsureFolderAndGetUniquePath(folder, SanitizeFileName(itemName));
		if (string.IsNullOrEmpty(uniquePath))
		{
			EditorUtility.DisplayDialog("Create Armour", "Could not determine a valid asset path.", "OK");
			return;
		}

		AssetDatabase.CreateAsset(armour, uniquePath);
		database.items.Add(armour);
		Selection.activeObject = armour;
	}

	private void CreateWeapon()
	{
		Weapon weapon = CreateInstance<Weapon>();
		SerializedObject so = new SerializedObject(weapon);

		so.FindProperty("id").stringValue = ItemIDGenerator.GenerateID();
		so.FindProperty("itemName").stringValue = itemName;
		so.FindProperty("itemDescription").stringValue = itemDescription;
		so.FindProperty("itemIcon").objectReferenceValue = itemIcon;
		so.FindProperty("itemMesh").objectReferenceValue = itemMesh;
		so.FindProperty("maxStackSize").intValue = Mathf.Max(1, maxStackSize);

		SerializedProperty pR = so.FindProperty("rarity");
		if (pR != null)
		{
			pR.enumValueIndex = (int)itemRarity;
		}
		SerializedProperty tagsProp = so.FindProperty("tags");
		if (tagsProp != null)
		{
			tagsProp.arraySize = tagsWindow.Count;
			for (int i = 0; i < tagsWindow.Count; i++)
			{
				tagsProp.GetArrayElementAtIndex(i).objectReferenceValue = tagsWindow[i];
			}
		}

		SerializedProperty equipProp = so.FindProperty("equipSlotType");
		if (equipProp != null) 
		{ 
			equipProp.enumValueIndex = (int)equipSlotType; 
		}

		SetEquippableNumericProps(so);

		SerializedProperty effectsProp = so.FindProperty("itemEffects");
		if (effectsProp != null)
		{
			effectsProp.arraySize = itemEffectsWindow.Count;
			for (int i = 0; i < itemEffectsWindow.Count; i++)
			{
				effectsProp.GetArrayElementAtIndex(i).objectReferenceValue = itemEffectsWindow[i];

			}
		}
		SetDamageStructToProperty(so, "damageBonusPercentages", damageBonusPercentagesWindow);

		SerializedProperty wTypeProp = so.FindProperty("weaponType");
		if (wTypeProp != null)
		{
			wTypeProp.enumValueIndex = (int)weaponType;
		}
		SetDamageStructToProperty(so, "weaponDamageScalings", weaponDamageScalingsWindow);

		so.ApplyModifiedProperties();

		string folder = "Assets/InGameItems/Equipment/Weapons";
		string uniquePath = EnsureFolderAndGetUniquePath(folder, SanitizeFileName(itemName));
		if (string.IsNullOrEmpty(uniquePath))
		{
			EditorUtility.DisplayDialog("Create Weapon", "Could not determine a valid asset path.", "OK");
			return;
		}

		AssetDatabase.CreateAsset(weapon, uniquePath);
		database.items.Add(weapon);
		Selection.activeObject = weapon;
	}

	private void CreateArtifact()
	{
		Artifact artifact = CreateInstance<Artifact>();
		SerializedObject so = new SerializedObject(artifact);

		so.FindProperty("id").stringValue = ItemIDGenerator.GenerateID();
		so.FindProperty("itemName").stringValue = itemName;
		so.FindProperty("itemDescription").stringValue = itemDescription;
		so.FindProperty("itemIcon").objectReferenceValue = itemIcon;
		so.FindProperty("itemMesh").objectReferenceValue = itemMesh;
		so.FindProperty("maxStackSize").intValue = Mathf.Max(1, maxStackSize);

		SerializedProperty pR = so.FindProperty("rarity");
		if (pR != null) 
		{
			pR.enumValueIndex = (int)itemRarity; 
		}

		SerializedProperty tagsProp = so.FindProperty("tags");
		if (tagsProp != null)
		{
			tagsProp.arraySize = tagsWindow.Count;
			for (int i = 0; i < tagsWindow.Count; i++)
			{


				tagsProp.GetArrayElementAtIndex(i).objectReferenceValue = tagsWindow[i];
			}
		}
		SetEquippableNumericProps(so);

		SerializedProperty effectsProp = so.FindProperty("itemEffects");
		if (effectsProp != null)
		{
			effectsProp.arraySize = itemEffectsWindow.Count;
			for (int i = 0; i < itemEffectsWindow.Count; i++)
			{


				effectsProp.GetArrayElementAtIndex(i).objectReferenceValue = itemEffectsWindow[i];
			} 
		}

		SetDamageStructToProperty(so, "damageBonusPercentages", damageBonusPercentagesWindow);

		so.ApplyModifiedProperties();

		string folder = "Assets/InGameItems/Equipment/Artifacts";
		string uniquePath = EnsureFolderAndGetUniquePath(folder, SanitizeFileName(itemName));
		if (string.IsNullOrEmpty(uniquePath))
		{
			EditorUtility.DisplayDialog("Create Artifact", "Could not determine a valid asset path.", "OK");
			return;
		}

		AssetDatabase.CreateAsset(artifact, uniquePath);
		database.items.Add(artifact);
		Selection.activeObject = artifact;
	}
}
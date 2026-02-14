using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;

public partial class ItemDatabaseWindow
{
	/// <summary>
	/// Draws the edit section of the custom editor UI, allowing users to view and modify the properties of the currently
	/// selected item.
	/// </summary>
	private void DrawEditSection()
	{
		EditorGUILayout.BeginVertical("box");
		EditorGUILayout.LabelField("Edit Item", EditorStyles.boldLabel);
		EditorGUILayout.Space();

		if (editItem == null)
		{
			EditorGUILayout.HelpBox("No item selected for editing. Select an item from the Database tab and press Edit.", MessageType.Info);
			if (GUILayout.Button("Go to Database"))
			{
				mainTab = MainTab.Database;
			}
			EditorGUILayout.EndVertical();
			return;
		}

		if (editItemSO == null || editItemSO.targetObject != editItem)
		{
			editItemSO = new SerializedObject(editItem);
			editItemSnapshotJson = EditorJsonUtility.ToJson(editItem);
			editItemSO.Update();
		}

		editScrollPos = EditorGUILayout.BeginScrollView(editScrollPos);

		SerializedProperty pName = editItemSO.FindProperty("itemName");
		SerializedProperty pDesc = editItemSO.FindProperty("itemDescription");
		SerializedProperty pIcon = editItemSO.FindProperty("itemIcon");
		SerializedProperty pMesh = editItemSO.FindProperty("itemMesh");
		SerializedProperty pMaxStack = editItemSO.FindProperty("maxStackSize");
		SerializedProperty pRarity = editItemSO.FindProperty("rarity");
		SerializedProperty pTags = editItemSO.FindProperty("tags");

		if (pName != null) EditorGUILayout.PropertyField(pName, kItemName);
		if (pDesc != null) EditorGUILayout.PropertyField(pDesc, kItemDescription);
		if (pIcon != null) EditorGUILayout.PropertyField(pIcon, kItemIcon);
		if (pMesh != null) EditorGUILayout.PropertyField(pMesh, kItemMesh);
		if (pMaxStack != null) EditorGUILayout.PropertyField(pMaxStack, kMaxStackSize);
		if (pRarity != null) EditorGUILayout.PropertyField(pRarity, kItemRarity);
		if (pTags != null) EditorGUILayout.PropertyField(pTags, new GUIContent("Tags"), true);

		if (editItem is EquippableItem)
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Equippable Properties", EditorStyles.boldLabel);

			SerializedProperty pEquip = editItemSO.FindProperty("equipSlotType");
			if (pEquip != null) EditorGUILayout.PropertyField(pEquip, kEquipSlotType);

			SerializedProperty pBonusStr = editItemSO.FindProperty("BonusStrength");
			SerializedProperty pBonusAgi = editItemSO.FindProperty("BonusAgility");
			SerializedProperty pBonusInt = editItemSO.FindProperty("BonusIntelligence");
			SerializedProperty pCritChance = editItemSO.FindProperty("BonusCriticalChance");
			SerializedProperty pCritDmg = editItemSO.FindProperty("BonusCriticalDamage");

			if (pBonusStr != null) EditorGUILayout.PropertyField(pBonusStr, kBonusStrength);
			if (pBonusAgi != null) EditorGUILayout.PropertyField(pBonusAgi, kBonusAgility);
			if (pBonusInt != null) EditorGUILayout.PropertyField(pBonusInt, kBonusIntelligence);
			if (pCritChance != null) EditorGUILayout.PropertyField(pCritChance, kBonusCriticalChance);
			if (pCritDmg != null) EditorGUILayout.PropertyField(pCritDmg, kBonusCriticalDamage);

			SerializedProperty pEffects = editItemSO.FindProperty("itemEffects");
			if (pEffects != null) EditorGUILayout.PropertyField(pEffects, kItemEffects, true);

			SerializedProperty pDmg = editItemSO.FindProperty("damageBonusPercentages");
			if (pDmg != null)
			{
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Damage Bonus Percentages", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(pDmg, true);
			}
		}

		if (editItem is Weapon)
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Weapon Properties", EditorStyles.boldLabel);
			SerializedProperty pWType = editItemSO.FindProperty("weaponType");
			if (pWType != null)
			{
				EditorGUILayout.PropertyField(pWType, new GUIContent("Weapon Type"));
			}
			SerializedProperty pWScal = editItemSO.FindProperty("weaponDamageScalings");
			if (pWScal != null) 
			{ 
				EditorGUILayout.PropertyField(pWScal, true); 
			}

		}

		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Save Changes", GUILayout.Width(120)))
		{
			if (editItemSO.ApplyModifiedProperties())
			{
				EditorUtility.SetDirty(editItem);
				AssetDatabase.SaveAssets();
				editItemSnapshotJson = EditorJsonUtility.ToJson(editItem);
				editItemSO = new SerializedObject(editItem);
				editItemSO.Update();
			}
			else
			{
				EditorUtility.SetDirty(editItem);
				AssetDatabase.SaveAssets();
				editItemSnapshotJson = EditorJsonUtility.ToJson(editItem);
				editItemSO = new SerializedObject(editItem);
				editItemSO.Update();
			}
		}
		if (GUILayout.Button("Revert", GUILayout.Width(120)))
		{
			if (!string.IsNullOrEmpty(editItemSnapshotJson))
			{
				EditorJsonUtility.FromJsonOverwrite(editItemSnapshotJson, editItem);
				EditorUtility.SetDirty(editItem);
				AssetDatabase.SaveAssets();
				editItemSO = new SerializedObject(editItem);
				editItemSO.Update();
			}
			else
			{
				editItemSO = new SerializedObject(editItem);
				editItemSO.Update();
			}
		}
		if (GUILayout.Button("Close Editor", GUILayout.Width(120)))
		{
			editItem = null;
			editItemSO = null;
			editItemSnapshotJson = null;
			mainTab = MainTab.Database;
		}
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Reveal Asset", GUILayout.Width(120)))
		{
			string path = AssetDatabase.GetAssetPath(editItem);
			if (!string.IsNullOrEmpty(path))
			{
				EditorUtility.RevealInFinder(path);
			}
			else
			{
				EditorUtility.DisplayDialog("Reveal", "Asset path not found.", "OK");
			}
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.EndScrollView();
		EditorGUILayout.EndVertical();
	}


    /// <summary>
    /// Draws a read-only display of the selected item's properties, used in the view mode of the editor
    /// </summary>
    private void DrawItemDetails(Item item)
	{
		EditorGUI.BeginDisabledGroup(true);

		EditorGUILayout.BeginHorizontal();
		Texture2D tex = item.GetItemIcon() != null ? item.GetItemIcon().texture : null;
		if (tex != null)
		{
			GUILayout.Label(tex, GUILayout.Width(64), GUILayout.Height(64));
		}
		else 
		{ 
			GUILayout.Label(GUIContent.none, GUILayout.Width(64), GUILayout.Height(64)); 
		}

		EditorGUILayout.BeginVertical();
		EditorGUILayout.LabelField("Name", item.GetItemName() ?? "(Unnamed)");
		EditorGUILayout.LabelField("Type", item.GetType().Name);
		EditorGUILayout.LabelField("ID", item.GetID() ?? string.Empty);
		EditorGUILayout.LabelField("Max Stack", item.GetMaxStackSize().ToString());

		SerializedObject soTop = new SerializedObject(item);
		soTop.Update();
		SerializedProperty pRarity = soTop.FindProperty("rarity");
		if (pRarity != null)
		{
			try
			{
				ItemRarity rarityVal = (ItemRarity)pRarity.enumValueIndex;
				EditorGUILayout.LabelField("Rarity", rarityVal.ToString());
			}
			catch
			{
				EditorGUILayout.LabelField("Rarity", "<Unknown>");
			}
		}
		soTop.ApplyModifiedProperties();

		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);
		EditorGUILayout.SelectableLabel(item.GetItemDescription() ?? string.Empty, GUILayout.Height(40));

		EditorGUILayout.Space();

		GameObject mesh = item.GetItemMesh();
		EditorGUILayout.ObjectField("World Mesh", mesh, typeof(GameObject), false);

		tg_ItemTag[] tagObjs = item.GetTagObjects();
		EditorGUILayout.LabelField("Tags", string.Join(", ", item.GetTagObjects().Where(t => t != null).Select(t => t.GetName())));

		if (item is EquippableItem)
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Equippable Properties", EditorStyles.boldLabel);

			SerializedObject so = new SerializedObject(item);
			so.Update();

			SerializedProperty equipProp = so.FindProperty("equipSlotType");
			if (equipProp != null)
			{
				EditorGUILayout.EnumPopup("Equip Slot Type", (EquipSlotType)equipProp.enumValueIndex);
			}

			SerializedProperty bonusStr = so.FindProperty("BonusStrength");
			SerializedProperty bonusAgi = so.FindProperty("BonusAgility");
			SerializedProperty bonusInt = so.FindProperty("BonusIntelligence");
			SerializedProperty critChance = so.FindProperty("BonusCriticalChance");
			SerializedProperty critDmg = so.FindProperty("BonusCriticalDamage");

			if (bonusStr != null) EditorGUILayout.IntField("Bonus Strength", bonusStr.intValue);
			if (bonusAgi != null) EditorGUILayout.IntField("Bonus Agility", bonusAgi.intValue);
			if (bonusInt != null) EditorGUILayout.IntField("Bonus Intelligence", bonusInt.intValue);
			if (critChance != null) EditorGUILayout.FloatField("Bonus Critical Chance", critChance.floatValue);
			if (critDmg != null) EditorGUILayout.FloatField("Bonus Critical Damage", critDmg.floatValue);

			SerializedProperty effectsProp = so.FindProperty("itemEffects");
			if (effectsProp != null && effectsProp.isArray)
			{
				EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);
				for (int i = 0; i < effectsProp.arraySize; i++)
				{
					SerializedProperty el = effectsProp.GetArrayElementAtIndex(i);
					ItemEffect eff = el != null ? el.objectReferenceValue as ItemEffect : null;
					EditorGUILayout.LabelField($"- {eff?.name ?? "<None>"}");
				}
			}

			SerializedProperty dmgProp = so.FindProperty("damageBonusPercentages");
			if (dmgProp != null)
			{
				EditorGUILayout.LabelField("Damage Bonus Percentages", EditorStyles.boldLabel);
				DrawDamageStructDisplay(dmgProp);
			}

			if (item is Weapon)
			{
				SerializedProperty wTypeProp = so.FindProperty("weaponType");
				if (wTypeProp != null)
				{
					EditorGUILayout.EnumPopup("Weapon Type", (WeaponType)wTypeProp.enumValueIndex);
				}


				SerializedProperty wScalings = so.FindProperty("weaponDamageScalings");
				if (wScalings != null)
				{
					EditorGUILayout.LabelField("Weapon Damage Scalings", EditorStyles.boldLabel);
					DrawDamageStructDisplay(wScalings);
				}
			}

			so.ApplyModifiedProperties();
		}

		EditorGUI.EndDisabledGroup();
	}
}
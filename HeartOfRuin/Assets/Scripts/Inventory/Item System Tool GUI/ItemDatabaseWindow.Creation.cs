using UnityEditor;
using UnityEngine;

public partial class ItemDatabaseWindow
{
	/// <summary>
	/// Draws the item creation section in the custom editor window, allowing users to input item details, assign tags and
	/// effects, and configure properties for different item types.
	/// </summary>
	private void DrawCreationSection()
	{
		creationScrollPos = EditorGUILayout.BeginScrollView(creationScrollPos);
		EditorGUILayout.BeginVertical("box");
		EditorGUILayout.LabelField("Item Creation", EditorStyles.boldLabel);

		string[] createTabs = new string[] { "Item", "Armour", "Weapon", "Artifact" };
		createTab = (CreateTab)GUILayout.Toolbar((int)createTab, createTabs);

		EditorGUILayout.Space();

		itemName = EditorGUILayout.TextField(kItemName, itemName);
		itemIcon = (Sprite)EditorGUILayout.ObjectField(kItemIcon, itemIcon, typeof(Sprite), false);
		itemDescription = EditorGUILayout.TextField(kItemDescription, itemDescription);
		itemMesh = (GameObject)EditorGUILayout.ObjectField(kItemMesh, itemMesh, typeof(GameObject), false);
		maxStackSize = EditorGUILayout.IntField(kMaxStackSize, maxStackSize);
		itemRarity = (ItemRarity)EditorGUILayout.EnumPopup(kItemRarity, itemRarity);

		EditorGUILayout.Space();
		GUILayout.Label(kTagsLabel, EditorStyles.boldLabel);
		for (int i = 0; i < tagsWindow.Count; i++)
		{
			EditorGUILayout.BeginHorizontal();
			tg_ItemTag tag = tagsWindow[i];
			GUILayout.Label($"{i + 1}. {(tag != null ? tag.GetName() : "<Missing Tag>")}", GUILayout.Width(220));
			if (GUILayout.Button("Remove", GUILayout.Width(70)))
			{
				tagsWindow.RemoveAt(i);
				break;
			}
			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.BeginHorizontal();
		newTagSelection = (tg_ItemTag)EditorGUILayout.ObjectField(newTagSelection, typeof(tg_ItemTag), false);
		if (GUILayout.Button("Add Tag", GUILayout.Width(80)))
		{
			if (newTagSelection != null && !tagsWindow.Exists(t => t != null && t.GetID() == newTagSelection.GetID()))
			{
				tagsWindow.Add(newTagSelection);
			}
			newTagSelection = null;
		}
		EditorGUILayout.EndHorizontal();

		if (createTab != CreateTab.Item)
		{
			EditorGUILayout.Space();
			GUILayout.Label("Equippable Properties", EditorStyles.boldLabel);

			if (createTab != CreateTab.Artifact)
				equipSlotType = (EquipSlotType)EditorGUILayout.EnumPopup(kEquipSlotType, equipSlotType);

			bonusStrength = EditorGUILayout.IntField(kBonusStrength, bonusStrength);
			bonusAgility = EditorGUILayout.IntField(kBonusAgility, bonusAgility);
			bonusIntelligence = EditorGUILayout.IntField(kBonusIntelligence, bonusIntelligence);
			bonusCriticalChance = EditorGUILayout.FloatField(kBonusCriticalChance, bonusCriticalChance);
			bonusCriticalDamage = EditorGUILayout.FloatField(kBonusCriticalDamage, bonusCriticalDamage);

			EditorGUILayout.Space();
			GUILayout.Label("Item Effects", EditorStyles.boldLabel);
			int removeIdx = -1;
			for (int i = 0; i < itemEffectsWindow.Count; i++)
			{
				EditorGUILayout.BeginHorizontal();
				GUIContent label = new GUIContent($"Effect {i + 1}", kItemEffects.tooltip);
				itemEffectsWindow[i] = (ItemEffect)EditorGUILayout.ObjectField(label, itemEffectsWindow[i], typeof(ItemEffect), false);
				if (GUILayout.Button("Remove", GUILayout.Width(70))) removeIdx = i;
				EditorGUILayout.EndHorizontal();
			}
			if (removeIdx >= 0) itemEffectsWindow.RemoveAt(removeIdx);
			if (GUILayout.Button("Add Effect")) itemEffectsWindow.Add(null);

			EditorGUILayout.Space();
			GUILayout.Label("Damage Bonus Percentages", EditorStyles.miniBoldLabel);
			DrawDamageStructFields(ref damageBonusPercentagesWindow);

			if (createTab == CreateTab.Weapon)
			{
				EditorGUILayout.Space();
				GUILayout.Label("Weapon Properties", EditorStyles.boldLabel);
				weaponType = (WeaponType)EditorGUILayout.EnumPopup(new GUIContent("Weapon Type"), weaponType);

				EditorGUILayout.Space();
				GUILayout.Label("Weapon Damage Scalings", EditorStyles.miniBoldLabel);
				DrawDamageStructFields(ref weaponDamageScalingsWindow);
			}
		}

		EditorGUILayout.Space();

		GUI.enabled = database != null && !string.IsNullOrWhiteSpace(itemName);
		if (GUILayout.Button("Create"))
		{
			CreateSelectedItem(createTab);
		}
		GUI.enabled = true;

		if (database == null)
			EditorGUILayout.HelpBox("Select a database to add item into.", MessageType.Info);
		if (string.IsNullOrWhiteSpace(itemName))
			EditorGUILayout.HelpBox("Item Name cannot be empty.", MessageType.Warning);

		EditorGUILayout.EndVertical();
		EditorGUILayout.EndScrollView();
	}

	/// <summary>
	/// Creates a new item in the database based on the specified tab selection.
	/// </summary>
	private void CreateSelectedItem(CreateTab tab)
	{
		switch (tab)
		{
			case CreateTab.Item:    CreateBaseItem();   break;
			case CreateTab.Armour:  CreateArmour();     break;
			case CreateTab.Weapon:  CreateWeapon();     break;
			case CreateTab.Artifact:CreateArtifact();   break;
			default:                CreateBaseItem();   break;
		}

		EditorUtility.SetDirty(database);
		AssetDatabase.SaveAssets();
	}
}
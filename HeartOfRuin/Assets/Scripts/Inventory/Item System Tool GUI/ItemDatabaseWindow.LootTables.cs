using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;

public partial class ItemDatabaseWindow
{

	/// <summary>
	/// Draws the Loot Tables management section in the custom editor UI, allowing users to create, add, edit, and remove
	/// loot tables from the associated item database.
	/// </summary>
	private void DrawLootTablesSection()
	{
		EditorGUILayout.BeginVertical("box");
		EditorGUILayout.LabelField("Loot Tables", EditorStyles.boldLabel);
		EditorGUILayout.Space();

		if (database == null)
		{
			EditorGUILayout.HelpBox("Assign an Item Database to manage its Loot Tables.", MessageType.Info);
			EditorGUILayout.EndVertical();
			return;
		}

		if (database.lootTables == null) database.lootTables = Array.Empty<LootTable>();

		EditorGUILayout.BeginHorizontal();
		newLootTableName = EditorGUILayout.TextField("New Table Name", newLootTableName);
		if (GUILayout.Button("Create Table", GUILayout.Width(120)))
		{
			CreateNewLootTableAsset(newLootTableName);
			newLootTableName = "New Loot Table";
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		addExistingLootSelection = (LootTable)EditorGUILayout.ObjectField("Add Existing", addExistingLootSelection, typeof(LootTable), false);
		if (GUILayout.Button("Add to Database", GUILayout.Width(140)))
		{
			if (addExistingLootSelection != null)
			{
				List<LootTable> list = new List<LootTable>(database.lootTables);
				if (!list.Contains(addExistingLootSelection))
				{
					list.Add(addExistingLootSelection);
					database.lootTables = list.ToArray();
					EditorUtility.SetDirty(database);
					AssetDatabase.SaveAssets();
				}
				addExistingLootSelection = null;
			}
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();

		lootScrollPos = EditorGUILayout.BeginScrollView(lootScrollPos, GUILayout.Height(200));
		for (int i = 0; i < database.lootTables.Length; i++)
		{
			LootTable lt = database.lootTables[i];
			if (lt == null) continue;

			EditorGUILayout.BeginHorizontal("box");
			GUILayout.Label(lt.name, GUILayout.Width(240));
			if (GUILayout.Button("Edit", GUILayout.Width(80)))
			{
				selectedLootTable = lt;
				selectedLootTableSO = null;
				selectedLootTableSnapshotJson = EditorJsonUtility.ToJson(selectedLootTable);
				selectedLootTableDirty = false;
				mainTab = MainTab.LootTables;
			}
			if (GUILayout.Button("Reveal", GUILayout.Width(80)))
			{
				string path = AssetDatabase.GetAssetPath(lt);
				if (!string.IsNullOrEmpty(path)) EditorUtility.RevealInFinder(path);
			}
			if (GUILayout.Button("Remove from DB", GUILayout.Width(140)))
			{
				if (EditorUtility.DisplayDialog("Remove Loot Table", $"Remove '{lt.name}' from database (asset will remain)?", "Remove", "Cancel"))
				{
					List<LootTable> list = new List<LootTable>(database.lootTables);
					list.Remove(lt);
					database.lootTables = list.ToArray();
					EditorUtility.SetDirty(database);
					AssetDatabase.SaveAssets();
				}
			}
			if (GUILayout.Button("Delete Asset", GUILayout.Width(120)))
			{
				if (EditorUtility.DisplayDialog("Delete Loot Table", $"Permanently delete '{lt.name}' asset and remove from DB?", "Delete", "Cancel"))
				{
					string path = AssetDatabase.GetAssetPath(lt);
					List<LootTable> list = new List<LootTable>(database.lootTables);
					list.Remove(lt);
					database.lootTables = list.ToArray();
					AssetDatabase.DeleteAsset(path);
					EditorUtility.SetDirty(database);
					AssetDatabase.SaveAssets();
				}
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndScrollView();

		EditorGUILayout.Space();

		if (selectedLootTable != null)
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField($"Editing: {selectedLootTable.name}" + (selectedLootTableDirty ? " *unsaved changes*" : ""), EditorStyles.boldLabel);

			if (selectedLootTableSO == null || selectedLootTableSO.targetObject != selectedLootTable)
			{
				selectedLootTableSO = new SerializedObject(selectedLootTable);
				selectedLootTableSO.Update();
			}

			lootEditScrollPos = EditorGUILayout.BeginScrollView(lootEditScrollPos);

			SerializedProperty pSampling = selectedLootTableSO.FindProperty("samplingMode");
			EditorGUI.BeginChangeCheck();
			if (pSampling != null) EditorGUILayout.PropertyField(pSampling);
			if (EditorGUI.EndChangeCheck()) selectedLootTableDirty = true;

			SerializedProperty pPicks = selectedLootTableSO.FindProperty("picks");
			if (pPicks != null)
			{
				try
				{
					LootSamplingMode mode = (LootSamplingMode)pSampling.enumValueIndex;
					if (mode == LootSamplingMode.WeightedPicks)
					{
						EditorGUI.BeginChangeCheck();
						EditorGUILayout.PropertyField(pPicks);
						if (EditorGUI.EndChangeCheck()) selectedLootTableDirty = true;
					}
				}
				catch
				{
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField(pPicks);
					if (EditorGUI.EndChangeCheck()) selectedLootTableDirty = true;
				}
			}

			EditorGUILayout.Space();
			SerializedProperty entriesProp = selectedLootTableSO.FindProperty("entries");
			if (entriesProp != null)
			{
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(entriesProp, new GUIContent("Entries"), true);
				if (EditorGUI.EndChangeCheck()) selectedLootTableDirty = true;
			}

			EditorGUILayout.Space();
			GUILayout.Label("Quick Add Entry - select an Item from this database", EditorStyles.miniBoldLabel);

			if (database.items == null || database.items.Count == 0)
			{
				EditorGUILayout.HelpBox("No items in database to add. Create items first.", MessageType.Info);
			}
			else
			{
				quickEntrySearch = EditorGUILayout.TextField("Search", quickEntrySearch);
				List<Item> dbItems = database.items.FindAll(it => it != null);
				List<Item> filtered = string.IsNullOrWhiteSpace(quickEntrySearch)
					? dbItems
					: dbItems.FindAll(it =>
						(it.GetItemName()?.IndexOf(quickEntrySearch, StringComparison.OrdinalIgnoreCase) >= 0)
						|| (it.GetID()?.IndexOf(quickEntrySearch, StringComparison.OrdinalIgnoreCase) >= 0)
						|| it.name.IndexOf(quickEntrySearch, StringComparison.OrdinalIgnoreCase) >= 0);

				EditorGUILayout.BeginVertical("box");
				int resultAreaHeight = Mathf.Min(200, 20 + filtered.Count * 22);
				GUILayout.BeginScrollView(Vector2.zero, GUILayout.Height(resultAreaHeight));
				foreach (Item it in filtered)
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.Label(it.GetItemName() ?? it.name, GUILayout.Width(300));
					if (GUILayout.Button("Select", GUILayout.Width(80)))
					{
						quickEntrySelectedItem = it;
					}
					EditorGUILayout.EndHorizontal();
				}
				GUILayout.EndScrollView();
				EditorGUILayout.EndVertical();

				EditorGUILayout.Space();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Selected Item:", GUILayout.Width(100));
				EditorGUILayout.LabelField(quickEntrySelectedItem != null
					? $"{quickEntrySelectedItem.GetItemName()} ({quickEntrySelectedItem.GetID()})"
					: "<none>");
				EditorGUILayout.EndHorizontal();

				quickEntryWeight = EditorGUILayout.FloatField("Weight", quickEntryWeight);
				quickEntryChance = EditorGUILayout.Slider("Chance (PerEntry)", quickEntryChance, 0f, 1f);
				EditorGUILayout.BeginHorizontal();
				quickEntryMin = EditorGUILayout.IntField("Min Count", quickEntryMin);
				quickEntryMax = EditorGUILayout.IntField("Max Count", quickEntryMax);
				quickEntryMin = Mathf.Abs(quickEntryMin);
				quickEntryMax = Mathf.Abs(quickEntryMax);
				if (quickEntryMax < quickEntryMin) quickEntryMax = quickEntryMin;
				EditorGUILayout.EndHorizontal();
				quickEntryUnique = EditorGUILayout.Toggle("Unique (WeightedPicks)", quickEntryUnique);

				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("Add Entry (buffered)", GUILayout.Width(160)))
				{
					if (quickEntrySelectedItem != null && entriesProp != null)
					{
						int newIndex = entriesProp.arraySize;
						entriesProp.InsertArrayElementAtIndex(newIndex);
						SerializedProperty newEl = entriesProp.GetArrayElementAtIndex(newIndex);
						if (newEl != null)
						{
							SerializedProperty pItem = newEl.FindPropertyRelative("item");
							if (pItem != null) pItem.objectReferenceValue = quickEntrySelectedItem;
							SerializedProperty pWeight = newEl.FindPropertyRelative("weight");
							if (pWeight != null) pWeight.floatValue = Mathf.Max(0f, quickEntryWeight);
							SerializedProperty pChance = newEl.FindPropertyRelative("chance");
							if (pChance != null) pChance.floatValue = Mathf.Clamp01(quickEntryChance);
							SerializedProperty pMin = newEl.FindPropertyRelative("minCount");
							if (pMin != null) pMin.intValue = Mathf.Max(0, quickEntryMin);
							SerializedProperty pMax = newEl.FindPropertyRelative("maxCount");
							if (pMax != null) pMax.intValue = Mathf.Max(quickEntryMin, quickEntryMax);
							SerializedProperty pUnique = newEl.FindPropertyRelative("unique");
							if (pUnique != null) pUnique.boolValue = quickEntryUnique;
						}
						selectedLootTableDirty = true;
					}
				}
				if (GUILayout.Button("Clear Selection", GUILayout.Width(140)))
				{
					quickEntrySearch = string.Empty;
					quickEntrySelectedItem = null;
					quickEntryWeight = 1f;
					quickEntryChance = 1f;
					quickEntryMin = 1;
					quickEntryMax = 1;
					quickEntryUnique = true;
				}
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Save Table", GUILayout.Width(120)))
			{
				if (selectedLootTableSO != null)
				{
					selectedLootTableSO.ApplyModifiedProperties();
					EditorUtility.SetDirty(selectedLootTable);
					AssetDatabase.SaveAssets();
					selectedLootTableSnapshotJson = EditorJsonUtility.ToJson(selectedLootTable);
					selectedLootTableDirty = false;
				}
			}
			if (GUILayout.Button("Revert", GUILayout.Width(120)))
			{
				if (!string.IsNullOrEmpty(selectedLootTableSnapshotJson))
				{
					EditorJsonUtility.FromJsonOverwrite(selectedLootTableSnapshotJson, selectedLootTable);
					selectedLootTableSO = new SerializedObject(selectedLootTable);
					selectedLootTableSO.Update();
					selectedLootTableDirty = false;
				}
				else
				{
					selectedLootTable = AssetDatabase.LoadAssetAtPath<LootTable>(AssetDatabase.GetAssetPath(selectedLootTable));
					selectedLootTableSO = new SerializedObject(selectedLootTable);
					selectedLootTableSO.Update();
					selectedLootTableDirty = false;
				}
			}
			if (GUILayout.Button("Close Editor", GUILayout.Width(120)))
			{
				if (selectedLootTableDirty)
				{
					int choice = EditorUtility.DisplayDialogComplex("Unsaved Changes",
						"There are unsaved changes to this Loot Table. Save before closing?",
						"Save", "Discard", "Cancel");
					if (choice == 0)
					{
						selectedLootTableSO?.ApplyModifiedProperties();
						EditorUtility.SetDirty(selectedLootTable);
						AssetDatabase.SaveAssets();
						selectedLootTableDirty = false;
					}
					else if (choice == 1)
					{
						if (!string.IsNullOrEmpty(selectedLootTableSnapshotJson))
							EditorJsonUtility.FromJsonOverwrite(selectedLootTableSnapshotJson, selectedLootTable);
					}
					else
					{
						EditorGUILayout.EndHorizontal();
						EditorGUILayout.EndScrollView();
						EditorGUILayout.EndVertical();
						return;
					}
				}

				selectedLootTable = null;
				selectedLootTableSO = null;
				selectedLootTableSnapshotJson = null;
				selectedLootTableDirty = false;
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndScrollView();
		}

		EditorGUILayout.EndVertical();
	}


    /// <summary>
    /// Method to create a new Loot Table asset, save it to the project, and add it to the current item database. The new asset will be created in a predefined folder, and the user will be prompted if the name is invalid or if there are issues determining the asset path.
    /// </summary>
    private void CreateNewLootTableAsset(string name)
	{
		LootTable lt = CreateInstance<LootTable>();
		lt.name = string.IsNullOrWhiteSpace(name) ? "New Loot Table" : name;

		string folder = "Assets/InGameItems/LootTables";
		string uniquePath = EnsureFolderAndGetUniquePath(folder, SanitizeFileName(lt.name));
		if (string.IsNullOrEmpty(uniquePath))
		{
			EditorUtility.DisplayDialog("Create Loot Table", "Could not determine a valid asset path.", "OK");
			return;
		}

		AssetDatabase.CreateAsset(lt, uniquePath);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		List<LootTable> list = new List<LootTable>(database.lootTables ?? Array.Empty<LootTable>());
		list.Add(lt);
		database.lootTables = list.ToArray();
		EditorUtility.SetDirty(database);
		AssetDatabase.SaveAssets();
		Selection.activeObject = lt;
	}
}
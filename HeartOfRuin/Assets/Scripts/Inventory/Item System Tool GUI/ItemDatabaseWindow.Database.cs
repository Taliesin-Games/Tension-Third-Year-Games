using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;

public partial class ItemDatabaseWindow
{
    private void DrawDatabaseViewer()
	{
		EditorGUILayout.BeginVertical("box");
		EditorGUILayout.LabelField("Database", EditorStyles.boldLabel);
		EditorGUILayout.Space();

		if (database == null)
		{
			EditorGUILayout.HelpBox("Assign an Item Database to view its contents.", MessageType.Info);
			EditorGUILayout.EndVertical();
			return;
		}

		EditorGUILayout.BeginVertical("box");
		EditorGUILayout.LabelField("Scan Folder for Items", EditorStyles.boldLabel);
		EditorGUILayout.BeginHorizontal();
		folderToScan = (DefaultAsset)EditorGUILayout.ObjectField("Folder", folderToScan, typeof(DefaultAsset), false);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		recursiveScan = EditorGUILayout.ToggleLeft("Recursive", recursiveScan, GUILayout.Width(110));
		assignMissingIDs = EditorGUILayout.ToggleLeft("Assign missing IDs", assignMissingIDs, GUILayout.Width(160));
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Scan and Add", GUILayout.Width(120)))
		{
			ScanFolderAndAddItems();
		}
		EditorGUILayout.EndHorizontal();

		if (!string.IsNullOrEmpty(lastScanReport))
		{
			EditorGUILayout.Space();
			EditorGUILayout.HelpBox(lastScanReport, MessageType.Info);
			if (lastScanAdded.Count > 0 || lastScanSkipped.Count > 0)
			{
				EditorGUILayout.LabelField("Scan results (preview):", EditorStyles.boldLabel);
				scanScrollPos = EditorGUILayout.BeginScrollView(scanScrollPos, GUILayout.Height(120));
				if (lastScanAdded.Count > 0)
				{
					EditorGUILayout.LabelField("Added:");
					foreach (Item it in lastScanAdded)
					{
						EditorGUILayout.LabelField($"  + {it.GetItemName()} ({AssetDatabase.GetAssetPath(it)})");
					}

				}
				if (lastScanSkipped.Count > 0)
				{
					EditorGUILayout.LabelField("Skipped:");
					foreach (Item it in lastScanSkipped)
					{
						EditorGUILayout.LabelField($"  - {it.GetItemName()} ({AssetDatabase.GetAssetPath(it)})");
					}

				}
				EditorGUILayout.EndScrollView();
			}
		}

		EditorGUILayout.EndVertical();

		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField($"Items: {database.items?.Count ?? 0}", GUILayout.Width(80));
		if (GUILayout.Button("Rebuild & Prune", GUILayout.Width(140)))
		{
			RebuildAndPruneDatabase();
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.LabelField("Sort:", GUILayout.Width(40));
		dbSort = (DbSort)EditorGUILayout.EnumPopup(dbSort, GUILayout.Width(110));
		dbSearch = EditorGUILayout.TextField(dbSearch, GUILayout.Width(200));
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();

		dbScrollPos = EditorGUILayout.BeginScrollView(dbScrollPos);
		List<Item> items = database.items ?? new List<Item>();
		IEnumerable<Item> display = items.Where(i => i != null);

		if (!string.IsNullOrWhiteSpace(dbSearch))
		{
			string s = dbSearch.ToLowerInvariant();
			display = display.Where(item =>
				(item.GetItemName()?.ToLowerInvariant().Contains(s) == true)
				|| (item.GetID()?.ToLowerInvariant().Contains(s) == true)
				|| (item.GetType().Name.ToLowerInvariant().Contains(s))
				|| (item.GetTagObjects()?.Any(t => t != null && t.GetName().ToLowerInvariant().Contains(s)) == true));
		}

		switch (dbSort)
		{
			case DbSort.Name:     display = display.OrderBy(i => i.GetItemName()); break;
			case DbSort.Type:     display = display.OrderBy(i => i.GetType().Name); break;
			case DbSort.MaxStack: display = display.OrderByDescending(i => i.GetMaxStackSize()); break;
			default: break;
		}

		List<Item> displayList = display.ToList();
		if (displayList.Count == 0)
		{
			EditorGUILayout.LabelField("No items match the current filter/sort.");
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
			return;
		}

		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("", GUILayout.Width(18));
		GUILayout.Label("Icon", EditorStyles.miniBoldLabel, GUILayout.Width(40));
		GUILayout.Label("Name", EditorStyles.miniBoldLabel, GUILayout.Width(200));
		GUILayout.Label("Type", EditorStyles.miniBoldLabel, GUILayout.Width(100));
		GUILayout.Label("Equip Slot", EditorStyles.miniBoldLabel, GUILayout.Width(100));
		GUILayout.Label("Max Stack", EditorStyles.miniBoldLabel, GUILayout.Width(70));
		GUILayout.Label("ID", EditorStyles.miniBoldLabel, GUILayout.Width(200));
		GUILayout.Label("Actions", EditorStyles.miniBoldLabel);
		EditorGUILayout.EndHorizontal();

		for (int idx = 0; idx < displayList.Count; idx++)
		{
			Item item = displayList[idx];
			if (item == null) continue;

			int idKey = item.GetInstanceID();
			bool expanded = dbExpandedIDs.Contains(idKey);

			EditorGUILayout.BeginHorizontal("box");
			Rect foldRect = GUILayoutUtility.GetRect(16, 16, GUILayout.Width(18));
			if (GUI.Button(foldRect, expanded ? "v" : ">", EditorStyles.label))
			{
				if (expanded) dbExpandedIDs.Remove(idKey);
				else dbExpandedIDs.Add(idKey);
			}

			Texture2D tex = item.GetItemIcon() != null ? item.GetItemIcon().texture : null;
			if (tex != null) GUILayout.Label(tex, GUILayout.Width(40), GUILayout.Height(40));
			else GUILayout.Label(GUIContent.none, GUILayout.Width(40), GUILayout.Height(40));

			GUILayout.Label(item.GetItemName() ?? "(Unnamed)", GUILayout.Width(200));
			GUILayout.Label(item.GetType().Name, GUILayout.Width(100));
			string equipSlotStr = item is EquippableItem eq ? eq.GetEquipSlotType().ToString() : "-";
			GUILayout.Label(equipSlotStr, GUILayout.Width(100));
			GUILayout.Label(item.GetMaxStackSize().ToString(), GUILayout.Width(70));
			GUILayout.Label(item.GetID() ?? string.Empty, GUILayout.Width(200));

			if (GUILayout.Button("Edit", GUILayout.Width(60)))
			{
				editItem = item;
				editItemSO = null;
				mainTab = MainTab.EditItem;
				Selection.activeObject = item;
				EditorGUIUtility.PingObject(item);
				dbSelectedIndex = idx;
			}
			if (GUILayout.Button("Reveal", GUILayout.Width(60)))
			{
				string path = AssetDatabase.GetAssetPath(item);
				if (!string.IsNullOrEmpty(path)) EditorUtility.RevealInFinder(path);
				else EditorUtility.DisplayDialog("Reveal", "Asset path not found.", "OK");
			}
			if (GUILayout.Button("Copy ID", GUILayout.Width(60)))
			{
				GUIUtility.systemCopyBuffer = item.GetID() ?? string.Empty;
			}
			if (GUILayout.Button("Delete Asset", GUILayout.Width(80)))
			{
				string path = AssetDatabase.GetAssetPath(item);
				if (string.IsNullOrEmpty(path))
				{
					EditorUtility.DisplayDialog("Delete", "Asset path not found.", "OK");
				}
				else if (EditorUtility.DisplayDialog("Delete Asset", $"Permanently delete asset at '{path}'?\nThis will remove it from the database and delete the file.", "Delete", "Cancel"))
				{
					database.items.Remove(item);
					EditorUtility.SetDirty(database);
					AssetDatabase.DeleteAsset(path);
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
					break;
				}
			}
			EditorGUILayout.EndHorizontal();

			if (dbExpandedIDs.Contains(idKey))
			{
				EditorGUILayout.BeginVertical("box");
				DrawItemDetails(item);
				EditorGUILayout.EndVertical();
			}
		}

		EditorGUILayout.EndScrollView();
		EditorGUILayout.EndVertical();
	}

	private void ScanFolderAndAddItems()
	{
		lastScanReport = string.Empty;
		lastScanAdded.Clear();
		lastScanSkipped.Clear();

		if (database == null)
		{
			EditorUtility.DisplayDialog("Scan Folder", "Assign an Item Database first.", "OK");
			return;
		}

		if (folderToScan == null)
		{
			EditorUtility.DisplayDialog("Scan Folder", "Select a folder to scan.", "OK");
			return;
		}

		string folderPath = AssetDatabase.GetAssetPath(folderToScan);
		if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
		{
			EditorUtility.DisplayDialog("Scan Folder", "Selected asset is not a valid folder.", "OK");
			return;
		}

		string[] guids = AssetDatabase.FindAssets("t:Item", new[] { folderPath });
		int added = 0, skipped = 0, errors = 0;

		database.BuildLookup();

		foreach (string guid in guids)
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);

			if (!recursiveScan)
			{
				string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
				string normalizedFolder = folderPath.Replace('\\', '/').TrimEnd('/');
				if (!string.Equals(parent, normalizedFolder, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

			}

			Item asset = AssetDatabase.LoadAssetAtPath<Item>(path);
			if (asset == null) 
			{
				errors++;
				continue;
			}

			string id = asset.GetID();
			if (string.IsNullOrWhiteSpace(id) || id.Trim() == "0")
			{
				if (assignMissingIDs)
				{
					SerializedObject so = new SerializedObject(asset);
					SerializedProperty prop = so.FindProperty("id");
					if (prop != null)
					{
						prop.stringValue = ItemIDGenerator.GenerateID();
						so.ApplyModifiedProperties();
						AssetDatabase.SaveAssets();
						id = asset.GetID();
					}
				}
				else
				{
					lastScanSkipped.Add(asset);
					skipped++;
					continue;
				}
			}

			bool duplicate = false;
			if (!string.IsNullOrEmpty(id))
			{
				Item existing = database.GetByID(id);
				if (existing != null) duplicate = true;
			}
			if (!duplicate && database.items.Contains(asset))
			{
				duplicate = true;
			}
			if (duplicate)
			{
				lastScanSkipped.Add(asset);
				skipped++;
				continue;
			}

			database.items.Add(asset);
			lastScanAdded.Add(asset);
			added++;
		}

		EditorUtility.SetDirty(database);
		AssetDatabase.SaveAssets();

		lastScanReport = $"Scan finished. Added: {added}, Skipped (duplicates/missing): {skipped}, Errors: {errors}";
		database.BuildLookup();
	}

	private void RebuildAndPruneDatabase()
	{
		if (database == null)
		{
			EditorUtility.DisplayDialog("Rebuild & Prune", "Assign an Item Database first.", "OK");
			return;
		}

		if (!EditorUtility.DisplayDialog("Rebuild & Prune", "This will remove database entries whose assets no longer exist on disk. Continue?", "Prune", "Cancel"))
		{
			return;
		}


		bool assignedAny = false;
		for (int i = 0; i < database.items.Count; i++)
		{
			Item itm = database.items[i];
			if (itm == null)
			{
				continue;
			}
			string id = itm.GetID();
			if (string.IsNullOrWhiteSpace(id) || id.Trim() == "0")
			{
				SerializedObject so = new SerializedObject(itm);
				SerializedProperty prop = so.FindProperty("id");
				if (prop != null)
				{
					prop.stringValue = ItemIDGenerator.GenerateID();
					so.ApplyModifiedProperties();
					EditorUtility.SetDirty(itm);
					assignedAny = true;
				}
			}
		}
		if (assignedAny)
		{
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		int removed = 0;
		for (int i = database.items.Count - 1; i >= 0; i--)
		{
			Item item = database.items[i];
			bool shouldRemove = false;

			if (item == null)
			{
				shouldRemove = true;
			}
			else
			{
				string path = AssetDatabase.GetAssetPath(item);
				if (string.IsNullOrEmpty(path) || !File.Exists(path)) shouldRemove = true;
			}

			if (shouldRemove)
			{
				database.items.RemoveAt(i);
				removed++;
			}
		}

		if (removed > 0)
		{
			EditorUtility.SetDirty(database);
			AssetDatabase.SaveAssets();
		}

		database.BuildLookup();
		EditorUtility.DisplayDialog("Rebuild & Prune", $"Prune complete. Removed {removed} missing items.", "OK");
	}
}
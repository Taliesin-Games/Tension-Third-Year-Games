using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;


/// <summary>
/// base class for the Item Database Editor Window, responsible for overall layout and tab management.
/// </summary>
public partial class ItemDatabaseWindow : EditorWindow
{
	/// <summary>
	/// Opens the Item System Tools window in the Unity Editor.
	/// </summary>
	[MenuItem("Tools/Items/Item System Tools")]
	public static void Open()
	{
		GetWindow<ItemDatabaseWindow>("Item System Tools");
	}


    /// <summary>
	/// handles drawing each page of the Item System Tools window based on the currently selected tab.
	/// </summary>
    private void OnGUI()
	{
		EditorGUILayout.Space();
		EditorGUILayout.BeginVertical("box");
		database = (ItemDatabase)EditorGUILayout.ObjectField(
			new GUIContent("Item Database"), database, typeof(ItemDatabase), false);
		EditorGUILayout.EndVertical();

		EditorGUILayout.Space();

		string[] mainTabs = new[] { "Item Creation", "Database", "Tags", "Loot Tables", "Edit Item" };
		mainTab = (MainTab)GUILayout.Toolbar((int)mainTab, mainTabs);

		EditorGUILayout.Space();

		switch (mainTab)
		{
			case MainTab.ItemCreation:
				DrawCreationSection();
				break;
			case MainTab.Database:
				DrawDatabaseViewer();
				break;
			case MainTab.Tags:
				DrawTagsSection();
				break;
			case MainTab.LootTables:
				DrawLootTablesSection();
				break;
			case MainTab.EditItem:
			default:
				DrawEditSection();
				break;
		}
	}
}
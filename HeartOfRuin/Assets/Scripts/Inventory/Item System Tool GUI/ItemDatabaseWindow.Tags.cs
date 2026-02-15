using UnityEditor;
using UnityEngine;
using System.IO;

public partial class ItemDatabaseWindow
{

    /// <summary>
	/// Draws the tag section of the item database editor, allowing users to create, edit, and manage item tags. Tags can be used to categorize items and add metadata for filtering and organization.
	/// </summary>
    private void DrawTagsSection()
	{
		EnsureTagsLoaded();

		EditorGUILayout.BeginVertical("box");
		EditorGUILayout.LabelField("Tag Management", EditorStyles.boldLabel);
		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Refresh", GUILayout.Width(80))) RefreshTagList();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Open Tag Folder", GUILayout.Width(140)))
		{
			string folder = "Assets/InGameItems/Tags";
			if (!AssetDatabase.IsValidFolder(folder))
				AssetDatabase.CreateFolder("Assets/InGameItems", "Tags");
			EditorUtility.RevealInFinder(Path.GetFullPath(folder));
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Create New Tag", EditorStyles.boldLabel);
		tagCreateScrollPos = EditorGUILayout.BeginScrollView(tagCreateScrollPos, GUILayout.Height(140));
		newTagName = EditorGUILayout.TextField(kTagName, newTagName);
		newTagColor = EditorGUILayout.ColorField(kTagColor, newTagColor);
		newTagIcon = (Sprite)EditorGUILayout.ObjectField(kTagIcon, newTagIcon, typeof(Sprite), false);
		newTagDescription = EditorGUILayout.TextField(kTagDescription, newTagDescription);
		EditorGUILayout.EndScrollView();

		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Create Tag", GUILayout.Width(120)))
		{
			CreateTagAsset(newTagName, newTagColor, newTagIcon, newTagDescription);
			ResetNewTagFields();
			RefreshTagList();
		}
		if (GUILayout.Button("Create + Add to Selection", GUILayout.Width(180)))
		{
			CreateTagAsset(newTagName, newTagColor, newTagIcon, newTagDescription, addToSelection: true);
			ResetNewTagFields();
			RefreshTagList();
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Existing Tags", EditorStyles.boldLabel);

		tagsScrollPos = EditorGUILayout.BeginScrollView(tagsScrollPos, GUILayout.Height(200));
		for (int i = 0; i < allTags.Count; i++)
		{
			tg_ItemTag t = allTags[i];
			if (t == null) continue;

			EditorGUILayout.BeginHorizontal("box");
			Sprite icon = t.GetIcon();
			if (icon != null)
			{
				GUILayout.Label(icon.texture, GUILayout.Width(24), GUILayout.Height(24));
			}

			else
			{
				GUILayout.Label(GUIContent.none, GUILayout.Width(24), GUILayout.Height(24));
			}

			Rect swatchRect = GUILayoutUtility.GetRect(18, 18);
			EditorGUI.DrawRect(swatchRect, t.GetColor());

			GUILayout.Label(t.GetName(), GUILayout.Width(200));
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Select", GUILayout.Width(70)))
			{
				Selection.activeObject = t;
				EditorGUIUtility.PingObject(t);
				selectedTag = t;
				selectedTagSO = new SerializedObject(selectedTag);
			}
			if (GUILayout.Button("Edit", GUILayout.Width(70)))
			{
				selectedTag = t;
				selectedTagSO = new SerializedObject(selectedTag);
			}
			if (GUILayout.Button("Delete", GUILayout.Width(70)))
			{
				if (EditorUtility.DisplayDialog("Delete Tag", $"Permanently delete tag '{t.GetName()}'?", "Delete", "Cancel"))
				{
					string path = AssetDatabase.GetAssetPath(t);
					if (!string.IsNullOrEmpty(path))
					{
						AssetDatabase.DeleteAsset(path);
						AssetDatabase.SaveAssets();
						RefreshTagList();
						selectedTag = null;
						selectedTagSO = null;
					}
				}
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndScrollView();

		EditorGUILayout.Space();
		if (selectedTag != null)
		{
			EditorGUILayout.LabelField("Edit Tag", EditorStyles.boldLabel);
			if (selectedTagSO == null) selectedTagSO = new SerializedObject(selectedTag);

			selectedTagSO.Update();
			SerializedProperty pName = selectedTagSO.FindProperty("tagName");
			SerializedProperty pColor = selectedTagSO.FindProperty("color");
			SerializedProperty pIcon = selectedTagSO.FindProperty("icon");
			SerializedProperty pDesc = selectedTagSO.FindProperty("description");

			if (pName != null) EditorGUILayout.PropertyField(pName, kTagName);
			if (pColor != null) EditorGUILayout.PropertyField(pColor, kTagColor);
			if (pIcon != null) EditorGUILayout.PropertyField(pIcon, kTagIcon);
			if (pDesc != null) EditorGUILayout.PropertyField(pDesc, kTagDescription);

			selectedTagSO.ApplyModifiedProperties();

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Save Changes", GUILayout.Width(120)))
			{
				EditorUtility.SetDirty(selectedTag);
				AssetDatabase.SaveAssets();
				RefreshTagList();
			}
			if (GUILayout.Button("Clear Selection", GUILayout.Width(120)))
			{
				selectedTag = null;
				selectedTagSO = null;
			}
			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.EndVertical();
	}

    /// <summary>
	/// Refreshes the list of item tags by searching the project for all assets of type tg_ItemTag. This ensures that any new, edited, or deleted tags are reflected in the editor window.
	/// </summary>
    private void RefreshTagList()
	{
		allTags.Clear();
		string[] guids = AssetDatabase.FindAssets("t:tg_ItemTag");
		foreach (string g in guids)
		{
			string path = AssetDatabase.GUIDToAssetPath(g);
			tg_ItemTag tag = AssetDatabase.LoadAssetAtPath<tg_ItemTag>(path);
			if (tag != null) allTags.Add(tag);
		}
	}

    /// <summary>
    /// Checks if the tags have been loaded into the editor window, and if not, it calls RefreshTagList to load them. This is used to ensure that the tag list is populated before trying to display or interact with it in the UI.
    /// </summary>
    private void EnsureTagsLoaded()
	{
		if (!tagsLoaded)
		{
			RefreshTagList();
			tagsLoaded = true;
		}
	}

    /// <summary>
    /// Method creates a tg_tag asset with the specified properties and saves it to the project.
	/// It also handles adding the new tag to the selection if requested, and ensures that the asset is created in a valid location within the project folder structure.
    /// </summary>
    private void CreateTagAsset(string tagName, Color color, Sprite icon, string description, bool addToSelection = false)
	{
		tg_ItemTag tag = CreateInstance<tg_ItemTag>();
		SerializedObject so = new SerializedObject(tag);
		SerializedProperty pName = so.FindProperty("tagName");
		SerializedProperty pColor = so.FindProperty("color");
		SerializedProperty pIcon = so.FindProperty("icon");
		SerializedProperty pDesc = so.FindProperty("description");
		if (pName != null) pName.stringValue = tagName ?? "New Tag";
		if (pColor != null) pColor.colorValue = color;
		if (pIcon != null) pIcon.objectReferenceValue = icon;
		if (pDesc != null) pDesc.stringValue = description ?? string.Empty;
		so.ApplyModifiedProperties();

		string folder = "Assets/InGameItems/Tags";
		string uniquePath = EnsureFolderAndGetUniquePath(folder, SanitizeFileName(tagName));
		if (string.IsNullOrEmpty(uniquePath))
		{
			EditorUtility.DisplayDialog("Create Tag", "Could not determine a valid asset path.", "OK");
			return;
		}

		AssetDatabase.CreateAsset(tag, uniquePath);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		if (addToSelection)
		{
			RefreshTagList();
			tg_ItemTag created = AssetDatabase.LoadAssetAtPath<tg_ItemTag>(uniquePath);
			if (created != null) tagsWindow.Add(created);
		}
	}


    /// <summary>
    /// Resets the tag creation fields to their default values after a tag has been created.
    /// </summary>
    private void ResetNewTagFields()
	{
		newTagName = "New Tag";
		newTagColor = Color.white;
		newTagIcon = null;
		newTagDescription = string.Empty;
	}
}
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;

public static class ItemIDGenerator
{
    public static string Generate()
    {
        return Guid.NewGuid().ToString("N"); // stable, compact
    }
}

public class ItemDatabaseWindow : EditorWindow
{
    private ItemDatabase database;

    private enum MainTab { ItemCreation = 0, Database = 1, Tags = 2, EditItem = 3 }
    private MainTab mainTab = MainTab.ItemCreation;

    private enum CreateTab { Item = 0, Armour = 1, Weapon = 2, Artifact = 3 }
    private CreateTab createTab = CreateTab.Item;

    // Common fields
    private string itemName = "New Item";
    private Sprite itemIcon;
    private string itemDescription;
    private GameObject itemMesh;
    private int maxStackSize = 1;

    // Tags mirror for item creation (now tg_ItemTag references)
    private List<tg_ItemTag> tagsWindow = new List<tg_ItemTag>();
    private tg_ItemTag newTagSelection = null;

    // --- Tag management UI state ---
    private List<tg_ItemTag> allTags = new List<tg_ItemTag>();
    private bool tagsLoaded = false;
    private Vector2 tagsScrollPos;
    private tg_ItemTag selectedTag = null;
    private SerializedObject selectedTagSO = null;

    // New tag quick-creation fields
    private string newTagName = "New Tag";
    private Color newTagColor = Color.white;
    private Sprite newTagIcon = null;
    private string newTagDescription = string.Empty;
    private Vector2 tagCreateScrollPos;

    // Equippable fields (used for Armour, Weapon, and Artifact)
    private EquipSlotType equipSlotType = EquipSlotType.None;
    private int bonusStrength = 0;
    private int bonusAgility = 0;
    private int bonusIntelligence = 0;
    private float bonusCriticalChance = 0f;
    private float bonusCriticalDamage = 0f;
    // Item effects mirror for editing in the window
    private List<ItemEffect> itemEffectsWindow = new List<ItemEffect>();

    // DamageStruct mirrors for editing inside the window
    private DamageStruct damageBonusPercentagesWindow;
    private DamageStruct weaponDamageScalingsWindow;

    // Weapon specific (simple fields)
    private WeaponType weaponType = WeaponType.None;

    // Database viewer state
    private Vector2 dbScrollPos;
    private int dbSelectedIndex = -1;
    private string dbSearch = string.Empty;
    private enum DbSort { None = 0, Name = 1, Type = 2, MaxStack = 3 }
    private DbSort dbSort = DbSort.None;

    // Foldout state for database entries (use InstanceID ints for uniqueness)
    private HashSet<int> dbExpandedIDs = new HashSet<int>();

    // Editor / Edit-item state
    private Item editItem = null;
    private SerializedObject editItemSO = null;
    private Vector2 editScrollPos;

    // Snapshot JSON of the item state when editing started (used for Revert)
    private string editItemSnapshotJson = null;

    // Folder scan state
    private DefaultAsset folderToScan;
    private bool recursiveScan = true;
    private bool assignMissingIDs = true;
    private string lastScanReport = string.Empty;
    private List<Item> lastScanAdded = new List<Item>();
    private List<Item> lastScanSkipped = new List<Item>();
    private Vector2 scanScrollPos;

    // Creation scroll position (make creation tab scrollable)
    private Vector2 creationScrollPos;

    // GUIContent with tooltips (pulled from the Tooltip attributes on the ScriptableObjects)
    private static readonly GUIContent kItemName = new GUIContent("Item Name", "Name of the item");
    private static readonly GUIContent kItemIcon = new GUIContent("Icon", "Icon representing the item in the inventory UI");
    private static readonly GUIContent kItemDescription = new GUIContent("Description", "Description of the item");
    private static readonly GUIContent kItemMesh = new GUIContent("World Mesh", "3D model of the item for world representation");
    private static readonly GUIContent kMaxStackSize = new GUIContent("Max Stack Size", "Max number of items that can be stacked into a single inventory slot");

    private static readonly GUIContent kEquipSlotType = new GUIContent("Equip Slot Type", "Type of item slot this can be equipped into, Any can go into None type, None type cant go into any");
    private const string kDamageStructTooltip = "Percentage damage bonuses provided by the item";
    private static readonly GUIContent kBonusStrength = new GUIContent("Bonus Strength", "Bonus strength provided by the item");
    private static readonly GUIContent kBonusAgility = new GUIContent("Bonus Agility", "Bonus agility provided by the item");
    private static readonly GUIContent kBonusIntelligence = new GUIContent("Bonus Intelligence", "Bonus intelligence provided by the item");
    private static readonly GUIContent kBonusCriticalChance = new GUIContent("Bonus Critical Chance", "Bonus critical hit chance percentage (e.g., 0.2 for +20% critical chance)");
    private static readonly GUIContent kBonusCriticalDamage = new GUIContent("Bonus Critical Damage", "Bonus critical damage percentage (e.g., 0.5 for +50% critical damage)");
    private static readonly GUIContent kItemEffects = new GUIContent("Effect", "Effects applied by the item");
    private static readonly GUIContent kTagsLabel = new GUIContent("Tags", "Tags used to categorize this item for loot tables or filtering (reference Tag assets)");
    private static readonly GUIContent kTagName = new GUIContent("Name", "Tag name shown in UI");
    private static readonly GUIContent kTagColor = new GUIContent("Color", "Optional color to display with the tag");
    private static readonly GUIContent kTagIcon = new GUIContent("Icon", "Optional icon for the tag");
    private static readonly GUIContent kTagDescription = new GUIContent("Description", "Optional longer description for the tag");

    [MenuItem("Tools/Items/Item System Tools")]
    public static void Open()
    {
        GetWindow<ItemDatabaseWindow>("Item System Tools");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical("box");
        database = (ItemDatabase)EditorGUILayout.ObjectField(
            new GUIContent("Item Database"), database, typeof(ItemDatabase), false);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // Top-level: Item Creation vs Database vs Tags vs Edit Item
        string[] mainTabs = new[] { "Item Creation", "Database", "Tags", "Edit Item" };
        mainTab = (MainTab)GUILayout.Toolbar((int)mainTab, mainTabs);

        EditorGUILayout.Space();

        if (mainTab == MainTab.ItemCreation)
        {
            DrawCreationSection();
        }
        else if (mainTab == MainTab.Database)
        {
            DrawDatabaseViewer();
        }
        else if (mainTab == MainTab.Tags)
        {
            DrawTagsSection();
        }
        else
        {
            DrawEditSection();
        }
    }

    private void DrawCreationSection()
    {
        // Wrap the creation UI in a scroll view so the tab can shrink to fit smaller windows.
        creationScrollPos = EditorGUILayout.BeginScrollView(creationScrollPos);
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Item Creation", EditorStyles.boldLabel);

        // Creation sub-tabs
        string[] createTabs = new string[] { "Item", "Armour", "Weapon", "Artifact" };
        createTab = (CreateTab)GUILayout.Toolbar((int)createTab, createTabs);

        EditorGUILayout.Space();

        // Common fields displayed for all creation tabs
        itemName = EditorGUILayout.TextField(kItemName, itemName);
        itemIcon = (Sprite)EditorGUILayout.ObjectField(kItemIcon, itemIcon, typeof(Sprite), false);
        itemDescription = EditorGUILayout.TextField(kItemDescription, itemDescription);
        itemMesh = (GameObject)EditorGUILayout.ObjectField(kItemMesh, itemMesh, typeof(GameObject), false);
        maxStackSize = EditorGUILayout.IntField(kMaxStackSize, maxStackSize);

        // Tags editor (mirror) — now uses tg_ItemTag objects
        EditorGUILayout.Space();
        GUILayout.Label(kTagsLabel, EditorStyles.boldLabel);
        // show existing tags
        for (int i = 0; i < tagsWindow.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            var tag = tagsWindow[i];
            if (tag != null)
                GUILayout.Label($"{i + 1}. {tag.GetName()}", GUILayout.Width(220));
            else
                GUILayout.Label($"{i + 1}. <Missing Tag>", GUILayout.Width(220));

            if (GUILayout.Button("Remove", GUILayout.Width(70)))
            {
                tagsWindow.RemoveAt(i);
                break;
            }
            EditorGUILayout.EndHorizontal();
        }

        // add new tag selection
        EditorGUILayout.BeginHorizontal();
        newTagSelection = (tg_ItemTag)EditorGUILayout.ObjectField(newTagSelection, typeof(tg_ItemTag), false);
        if (GUILayout.Button("Add Tag", GUILayout.Width(80)))
        {
            if (newTagSelection != null && !tagsWindow.Any(t => t != null && t.GetID() == newTagSelection.GetID()))
            {
                tagsWindow.Add(newTagSelection);
            }
            newTagSelection = null;
        }
        EditorGUILayout.EndHorizontal();

        // If equippable tab or equippable-related tabs, show equippable options
        if (createTab != CreateTab.Item)
        {
            EditorGUILayout.Space();
            GUILayout.Label("Equippable Properties", EditorStyles.boldLabel);

            if (createTab != CreateTab.Artifact)
            {
                equipSlotType = (EquipSlotType)EditorGUILayout.EnumPopup(kEquipSlotType, equipSlotType);
            }

            bonusStrength = EditorGUILayout.IntField(kBonusStrength, bonusStrength);
            bonusAgility = EditorGUILayout.IntField(kBonusAgility, bonusAgility);
            bonusIntelligence = EditorGUILayout.IntField(kBonusIntelligence, bonusIntelligence);
            bonusCriticalChance = EditorGUILayout.FloatField(kBonusCriticalChance, bonusCriticalChance);
            bonusCriticalDamage = EditorGUILayout.FloatField(kBonusCriticalDamage, bonusCriticalDamage);

            // Item effects editor (mirror array)
            EditorGUILayout.Space();
            GUILayout.Label("Item Effects", EditorStyles.boldLabel);

            int removeIdx = -1;
            for (int i = 0; i < itemEffectsWindow.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                var label = new GUIContent($"Effect {i + 1}", kItemEffects.tooltip);
                itemEffectsWindow[i] = (ItemEffect)EditorGUILayout.ObjectField(label, itemEffectsWindow[i], typeof(ItemEffect), false);
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    removeIdx = i;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (removeIdx >= 0)
                itemEffectsWindow.RemoveAt(removeIdx);

            if (GUILayout.Button("Add Effect"))
            {
                itemEffectsWindow.Add(null);
            }

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
        {
            EditorGUILayout.HelpBox("Select a database to add item into.", MessageType.Info);
        }

        if (string.IsNullOrWhiteSpace(itemName))
        {
            EditorGUILayout.HelpBox("Item Name cannot be empty.", MessageType.Warning);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }

    // New Tags tab UI
    private void DrawTagsSection()
    {
        EnsureTagsLoaded();

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Tag Management", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh", GUILayout.Width(80)))
        {
            RefreshTagList();
        }

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Open Tag Folder", GUILayout.Width(140)))
        {
            var folder = "Assets/InGameItems/Tags";
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
            newTagName = "New Tag";
            newTagColor = Color.white;
            newTagIcon = null;
            newTagDescription = string.Empty;
            RefreshTagList();
        }
        if (GUILayout.Button("Create + Add to Selection", GUILayout.Width(180)))
        {
            CreateTagAsset(newTagName, newTagColor, newTagIcon, newTagDescription, addToSelection: true);
            newTagName = "New Tag";
            newTagColor = Color.white;
            newTagIcon = null;
            newTagDescription = string.Empty;
            RefreshTagList();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Existing Tags", EditorStyles.boldLabel);

        tagsScrollPos = EditorGUILayout.BeginScrollView(tagsScrollPos, GUILayout.Height(200));
        for (int i = 0; i < allTags.Count; i++)
        {
            var t = allTags[i];
            if (t == null) continue;

            EditorGUILayout.BeginHorizontal("box");
            // show icon + name + color swatch
            var icon = t.GetIcon();
            if (icon != null)
                GUILayout.Label(icon.texture, GUILayout.Width(24), GUILayout.Height(24));
            else
                GUILayout.Label(GUIContent.none, GUILayout.Width(24), GUILayout.Height(24));

            var swatchRect = GUILayoutUtility.GetRect(18, 18);
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
                    var path = AssetDatabase.GetAssetPath(t);
                    if (!string.IsNullOrEmpty(path))
                    {
                        database?.items?.ForEach(it =>
                        {
                            // optionally remove references from items — keep minimal: do not auto-edit items here.
                        });
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
            if (selectedTagSO == null)
                selectedTagSO = new SerializedObject(selectedTag);

            selectedTagSO.Update();
            var pName = selectedTagSO.FindProperty("tagName");
            var pColor = selectedTagSO.FindProperty("color");
            var pIcon = selectedTagSO.FindProperty("icon");
            var pDesc = selectedTagSO.FindProperty("description");

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

    private void RefreshTagList()
    {
        allTags.Clear();
        var guids = AssetDatabase.FindAssets("t:tg_ItemTag");
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var tag = AssetDatabase.LoadAssetAtPath<tg_ItemTag>(path);
            if (tag != null)
            {
                allTags.Add(tag);
            }
        }
    }

    private void EnsureTagsLoaded()
    {
        if (!tagsLoaded)
        {
            RefreshTagList();
            tagsLoaded = true;
        }
    }

    private void CreateTagAsset(string tagName, Color color, Sprite icon, string description, bool addToSelection = false)
    {
        var tag = CreateInstance<tg_ItemTag>();
        // set fields via SerializedObject to write private serialized fields
        var so = new SerializedObject(tag);
        var pName = so.FindProperty("tagName");
        var pColor = so.FindProperty("color");
        var pIcon = so.FindProperty("icon");
        var pDesc = so.FindProperty("description");
        if (pName != null) pName.stringValue = tagName ?? "New Tag";
        if (pColor != null) pColor.colorValue = color;
        if (pIcon != null) pIcon.objectReferenceValue = icon;
        if (pDesc != null) pDesc.stringValue = description ?? string.Empty;

        // ensure id is set by OnValidate in tg_ItemTag or set here
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
            // refresh list first to load new asset instance
            RefreshTagList();
            var created = AssetDatabase.LoadAssetAtPath<tg_ItemTag>(uniquePath);
            if (created != null)
            {
                tagsWindow.Add(created);
            }
        }
    }

    // Database viewer: table-like list of items with foldouts that show full read-only details
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

        // Folder scanning UI
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
                    foreach (var it in lastScanAdded)
                    {
                        EditorGUILayout.LabelField($"  + {it.GetItemName()} ({AssetDatabase.GetAssetPath(it)})");
                    }
                }
                if (lastScanSkipped.Count > 0)
                {
                    EditorGUILayout.LabelField("Skipped:");
                    foreach (var it in lastScanSkipped)
                    {
                        EditorGUILayout.LabelField($"  - {it.GetItemName()} ({AssetDatabase.GetAssetPath(it)})");
                    }
                }
                EditorGUILayout.EndScrollView();
            }
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // Controls: search, sort, count + Rebuild & Prune
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
        var items = database.items ?? new List<Item>();

        // Filter and prepare display list (do not mutate database.items here)
        IEnumerable<Item> display = items.Where(i => i != null);

        if (!string.IsNullOrWhiteSpace(dbSearch))
        {
            var s = dbSearch.ToLowerInvariant();
            display = display.Where(item =>
                (item.GetItemName()?.ToLowerInvariant().Contains(s) == true)
                || (item.GetID()?.ToLowerInvariant().Contains(s) == true)
                || (item.GetType().Name.ToLowerInvariant().Contains(s))
                || (item.GetTagObjects()?.Any(t => t != null && t.GetName().ToLowerInvariant().Contains(s)) == true)
            );
        }

        switch (dbSort)
        {
            case DbSort.Name:
                display = display.OrderBy(i => i.GetItemName());
                break;
            case DbSort.Type:
                display = display.OrderBy(i => i.GetType().Name);
                break;
            case DbSort.MaxStack:
                display = display.OrderByDescending(i => i.GetMaxStackSize());
                break;
            default:
                break;
        }

        var displayList = display.ToList();
        if (displayList.Count == 0)
        {
            EditorGUILayout.LabelField("No items match the current filter/sort.");
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            return;
        }

        // Headers (table)
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.Width(18)); // foldout column
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
            var item = displayList[idx];
            if (item == null) continue;

            int idKey = item.GetInstanceID();
            bool expanded = dbExpandedIDs.Contains(idKey);

            // Row (compact table)
            EditorGUILayout.BeginHorizontal("box");

            // small triangle button to toggle expansion (unique per asset id)
            Rect foldRect = GUILayoutUtility.GetRect(16, 16, GUILayout.Width(18));
            if (GUI.Button(foldRect, expanded ? "v" : ">", EditorStyles.label))
            {
                if (expanded) dbExpandedIDs.Remove(idKey);
                else dbExpandedIDs.Add(idKey);
            }

            // Icon
            Texture2D tex = item.GetItemIcon() != null ? item.GetItemIcon().texture : null;
            if (tex != null)
                GUILayout.Label(tex, GUILayout.Width(40), GUILayout.Height(40));
            else
                GUILayout.Label(GUIContent.none, GUILayout.Width(40), GUILayout.Height(40));

            // Name and basic columns
            GUILayout.Label(item.GetItemName() ?? "(Unnamed)", GUILayout.Width(200));
            GUILayout.Label(item.GetType().Name, GUILayout.Width(100));
            string equipSlotStr = item is EquippableItem eq ? eq.GetEquipSlotType().ToString() : "-";
            GUILayout.Label(equipSlotStr, GUILayout.Width(100));
            GUILayout.Label(item.GetMaxStackSize().ToString(), GUILayout.Width(70));
            GUILayout.Label(item.GetID() ?? string.Empty, GUILayout.Width(200));

            // Actions (edit/reveal/copy/delete)
            if (GUILayout.Button("Edit", GUILayout.Width(60)))
            {
                // open edit tab for this item
                editItem = item;
                editItemSO = null;
                // snapshot will be taken when the edit tab initializes (in DrawEditSection)
                mainTab = MainTab.EditItem;

                Selection.activeObject = item;
                EditorGUIUtility.PingObject(item);
                dbSelectedIndex = idx;
            }
            if (GUILayout.Button("Reveal", GUILayout.Width(60)))
            {
                var path = AssetDatabase.GetAssetPath(item);
                if (!string.IsNullOrEmpty(path))
                    EditorUtility.RevealInFinder(path);
                else
                    EditorUtility.DisplayDialog("Reveal", "Asset path not found.", "OK");
            }
            if (GUILayout.Button("Copy ID", GUILayout.Width(60)))
            {
                GUIUtility.systemCopyBuffer = item.GetID() ?? string.Empty;
            }
            if (GUILayout.Button("Delete Asset", GUILayout.Width(80)))
            {
                var path = AssetDatabase.GetAssetPath(item);
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

                    // stop iterating to refresh UI safely
                    break;
                }
            }

            EditorGUILayout.EndHorizontal();

            // Expanded details (read-only) 
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

    // Edit tab UI: full editable inspector for the selected item (uses SerializedObject)
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

        // Create SerializedObject on-demand and capture a snapshot JSON for revert.
        if (editItemSO == null || editItemSO.targetObject != editItem)
        {
            editItemSO = new SerializedObject(editItem);
            // capture snapshot of current serialized state as JSON so Revert can restore it
            editItemSnapshotJson = EditorJsonUtility.ToJson(editItem);
            // initial sync for PropertyFields
            editItemSO.Update();
        }

        editScrollPos = EditorGUILayout.BeginScrollView(editScrollPos);

        // IMPORTANT: do NOT apply editItemSO.ApplyModifiedProperties() each frame.
        // Buffer changes in the SerializedObject and only write them to the asset when Save is pressed.
        // Basic serialized fields
        var pName = editItemSO.FindProperty("itemName");
        var pDesc = editItemSO.FindProperty("itemDescription");
        var pIcon = editItemSO.FindProperty("itemIcon");
        var pMesh = editItemSO.FindProperty("itemMesh");
        var pMaxStack = editItemSO.FindProperty("maxStackSize");
        var pTags = editItemSO.FindProperty("tags");

        if (pName != null) EditorGUILayout.PropertyField(pName, kItemName);
        if (pDesc != null) EditorGUILayout.PropertyField(pDesc, kItemDescription);
        if (pIcon != null) EditorGUILayout.PropertyField(pIcon, kItemIcon);
        if (pMesh != null) EditorGUILayout.PropertyField(pMesh, kItemMesh);
        if (pMaxStack != null) EditorGUILayout.PropertyField(pMaxStack, kMaxStackSize);
        if (pTags != null) EditorGUILayout.PropertyField(pTags, new GUIContent("Tags"), true);

        // If equippable show equippable props
        if (editItem is EquippableItem)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Equippable Properties", EditorStyles.boldLabel);

            var pEquip = editItemSO.FindProperty("equipSlotType");
            if (pEquip != null) EditorGUILayout.PropertyField(pEquip, kEquipSlotType);

            var pBonusStr = editItemSO.FindProperty("BonusStrength");
            var pBonusAgi = editItemSO.FindProperty("BonusAgility");
            var pBonusInt = editItemSO.FindProperty("BonusIntelligence");
            var pCritChance = editItemSO.FindProperty("BonusCriticalChance");
            var pCritDmg = editItemSO.FindProperty("BonusCriticalDamage");

            if (pBonusStr != null) EditorGUILayout.PropertyField(pBonusStr, kBonusStrength);
            if (pBonusAgi != null) EditorGUILayout.PropertyField(pBonusAgi, kBonusAgility);
            if (pBonusInt != null) EditorGUILayout.PropertyField(pBonusInt, kBonusIntelligence);
            if (pCritChance != null) EditorGUILayout.PropertyField(pCritChance, kBonusCriticalChance);
            if (pCritDmg != null) EditorGUILayout.PropertyField(pCritDmg, kBonusCriticalDamage);

            var pEffects = editItemSO.FindProperty("itemEffects");
            if (pEffects != null) EditorGUILayout.PropertyField(pEffects, kItemEffects, true);

            var pDmg = editItemSO.FindProperty("damageBonusPercentages");
            if (pDmg != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Damage Bonus Percentages", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(pDmg, true);
            }
        }

        // Weapon specific
        if (editItem is Weapon)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Weapon Properties", EditorStyles.boldLabel);
            var pWType = editItemSO.FindProperty("weaponType");
            if (pWType != null) EditorGUILayout.PropertyField(pWType, new GUIContent("Weapon Type"));
            var pWScal = editItemSO.FindProperty("weaponDamageScalings");
            if (pWScal != null) EditorGUILayout.PropertyField(pWScal, true);
        }

        EditorGUILayout.Space();

        // Save / Revert / Close
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save Changes", GUILayout.Width(120)))
        {
            // Apply buffered edits to the actual asset and save.
            if (editItemSO.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(editItem);
                AssetDatabase.SaveAssets();
                // refresh snapshot so Revert will now restore the new saved state
                editItemSnapshotJson = EditorJsonUtility.ToJson(editItem);
                // recreate SerializedObject to ensure sync with saved asset
                editItemSO = new SerializedObject(editItem);
                editItemSO.Update();
            }
            else
            {
                // even if nothing reported as changed, still mark dirty+save to persist object references changes
                EditorUtility.SetDirty(editItem);
                AssetDatabase.SaveAssets();
                editItemSnapshotJson = EditorJsonUtility.ToJson(editItem);
                editItemSO = new SerializedObject(editItem);
                editItemSO.Update();
            }
        }
        if (GUILayout.Button("Revert", GUILayout.Width(120)))
        {
            // Restore the item from the snapshot JSON captured when editing began (or last saved).
            if (!string.IsNullOrEmpty(editItemSnapshotJson))
            {
                EditorJsonUtility.FromJsonOverwrite(editItemSnapshotJson, editItem);
                EditorUtility.SetDirty(editItem);
                AssetDatabase.SaveAssets();
                // recreate SerializedObject to reflect reverted values
                editItemSO = new SerializedObject(editItem);
                editItemSO.Update();
            }
            else
            {
                // Fallback: reload from the asset on disk
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
            var path = AssetDatabase.GetAssetPath(editItem);
            if (!string.IsNullOrEmpty(path)) EditorUtility.RevealInFinder(path);
            else EditorUtility.DisplayDialog("Reveal", "Asset path not found.", "OK");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    // Render read-only item details shown when a database entry foldout is expanded.
    private void DrawItemDetails(Item item)
    {
        EditorGUI.BeginDisabledGroup(true);

        // Top row: icon, basic fields
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
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Description
        EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);
        EditorGUILayout.SelectableLabel(item.GetItemDescription() ?? string.Empty, GUILayout.Height(40));

        EditorGUILayout.Space();

        // Mesh (show asset path)
        var mesh = item.GetItemMesh();
        EditorGUILayout.ObjectField("World Mesh", mesh, typeof(GameObject), false);

        // Tags
        var tagObjs = item.GetTagObjects();
        EditorGUILayout.LabelField("Tags", string.Join(", ", tagObjs.Where(t => t != null).Select(t => t.GetName())));

        // If equippable, show equippable info (read-only retrieved via SerializedObject)
        if (item is EquippableItem)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Equippable Properties", EditorStyles.boldLabel);

            var so = new SerializedObject(item);
            so.Update();

            var equipProp = so.FindProperty("equipSlotType");
            if (equipProp != null)
            {
                EditorGUILayout.EnumPopup("Equip Slot Type", (EquipSlotType)equipProp.enumValueIndex);
            }

            var bonusStr = so.FindProperty("BonusStrength");
            var bonusAgi = so.FindProperty("BonusAgility");
            var bonusInt = so.FindProperty("BonusIntelligence");
            var critChance = so.FindProperty("BonusCriticalChance");
            var critDmg = so.FindProperty("BonusCriticalDamage");

            if (bonusStr != null) EditorGUILayout.IntField("Bonus Strength", bonusStr.intValue);
            if (bonusAgi != null) EditorGUILayout.IntField("Bonus Agility", bonusAgi.intValue);
            if (bonusInt != null) EditorGUILayout.IntField("Bonus Intelligence", bonusInt.intValue);
            if (critChance != null) EditorGUILayout.FloatField("Bonus Critical Chance", critChance.floatValue);
            if (critDmg != null) EditorGUILayout.FloatField("Bonus Critical Damage", critDmg.floatValue);

            // ItemEffects: list names
            var effectsProp = so.FindProperty("itemEffects");
            if (effectsProp != null && effectsProp.isArray)
            {
                EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);
                for (int i = 0; i < effectsProp.arraySize; i++)
                {
                    var el = effectsProp.GetArrayElementAtIndex(i);
                    var eff = el != null ? el.objectReferenceValue as ItemEffect : null;
                    EditorGUILayout.LabelField($"- {eff?.name ?? "<None>"}");
                }
            }

            // damageBonusPercentages struct
            var dmgProp = so.FindProperty("damageBonusPercentages");
            if (dmgProp != null)
            {
                EditorGUILayout.LabelField("Damage Bonus Percentages", EditorStyles.boldLabel);
                DrawDamageStructDisplay(dmgProp);
            }

            // If weapon, show weapon-specific fields
            if (item is Weapon)
            {
                var wTypeProp = so.FindProperty("weaponType");
                if (wTypeProp != null)
                {
                    EditorGUILayout.EnumPopup("Weapon Type", (WeaponType)wTypeProp.enumValueIndex);
                }

                var wScalings = so.FindProperty("weaponDamageScalings");
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
        if (pPhysical != null) { EditorGUILayout.LabelField("Physical", pPhysical.floatValue.ToString()); }
        if (pMagical != null) { EditorGUILayout.LabelField("Magical", pMagical.floatValue.ToString()); }
        if (pTrue != null) { EditorGUILayout.LabelField("True", pTrue.floatValue.ToString()); }
        if (pFire != null) EditorGUILayout.LabelField("Fire", pFire.floatValue.ToString());
        if (pLightning != null) EditorGUILayout.LabelField("Lightning", pLightning.floatValue.ToString());
        if (pIce != null) EditorGUILayout.LabelField("Ice", pIce.floatValue.ToString());
        if (pEarth != null) EditorGUILayout.LabelField("Earth", pEarth.floatValue.ToString());
        if (pWind != null) EditorGUILayout.LabelField("Wind", pWind.floatValue.ToString());
        if (pWater != null) EditorGUILayout.LabelField("Water", pWater.floatValue.ToString());
        EditorGUILayout.EndVertical();
    }

    // Small helper to render the DamageStruct editable fields inside the window (creation UI)
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

    private void CreateSelectedItem(CreateTab tab)
    {
        switch (tab)
        {
            case CreateTab.Item:
                CreateBaseItem();
                break;
            case CreateTab.Armour:
                CreateArmour();
                break;
            case CreateTab.Weapon:
                CreateWeapon();
                break;
            case CreateTab.Artifact:
                CreateArtifact();
                break;
            default:
                CreateBaseItem();
                break;
        }

        // Save database and assets
        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
    }

    private void CreateBaseItem()
    {
        Item item = CreateInstance<Item>();

        SerializedObject so = new SerializedObject(item);

        // Set serialized fields (private fields accessible via SerializedObject)
        so.FindProperty("id").stringValue = ItemIDGenerator.Generate();
        so.FindProperty("itemName").stringValue = itemName;
        so.FindProperty("itemDescription").stringValue = itemDescription;
        so.FindProperty("itemIcon").objectReferenceValue = itemIcon;
        so.FindProperty("itemMesh").objectReferenceValue = itemMesh;
        so.FindProperty("maxStackSize").intValue = Mathf.Max(1, maxStackSize);

        // set tags (tg_ItemTag references)
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

        // Base item fields
        so.FindProperty("id").stringValue = ItemIDGenerator.Generate();
        so.FindProperty("itemName").stringValue = itemName;
        so.FindProperty("itemDescription").stringValue = itemDescription;
        so.FindProperty("itemIcon").objectReferenceValue = itemIcon;
        so.FindProperty("itemMesh").objectReferenceValue = itemMesh;
        so.FindProperty("maxStackSize").intValue = Mathf.Max(1, maxStackSize);

        // set tags
        SerializedProperty tagsProp = so.FindProperty("tags");
        if (tagsProp != null)
        {
            tagsProp.arraySize = tagsWindow.Count;
            for (int i = 0; i < tagsWindow.Count; i++)
            {
                tagsProp.GetArrayElementAtIndex(i).objectReferenceValue = tagsWindow[i];
            }
        }

        // Equippable fields 
        SerializedProperty equipProp = so.FindProperty("equipSlotType");
        if (equipProp != null)
        {
            equipProp.enumValueIndex = (int)equipSlotType;
        }

        SetEquippableNumericProps(so);

        // itemEffects (array)
        SerializedProperty effectsProp = so.FindProperty("itemEffects");
        if (effectsProp != null)
        {
            effectsProp.arraySize = itemEffectsWindow.Count;
            for (int i = 0; i < itemEffectsWindow.Count; i++)
            {
                effectsProp.GetArrayElementAtIndex(i).objectReferenceValue = itemEffectsWindow[i];
            }
        }

        // damageBonusPercentages
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

        // Base item fields
        so.FindProperty("id").stringValue = ItemIDGenerator.Generate();
        so.FindProperty("itemName").stringValue = itemName;
        so.FindProperty("itemDescription").stringValue = itemDescription;
        so.FindProperty("itemIcon").objectReferenceValue = itemIcon;
        so.FindProperty("itemMesh").objectReferenceValue = itemMesh;
        so.FindProperty("maxStackSize").intValue = Mathf.Max(1, maxStackSize);

        // set tags
        SerializedProperty tagsProp = so.FindProperty("tags");
        if (tagsProp != null)
        {
            tagsProp.arraySize = tagsWindow.Count;
            for (int i = 0; i < tagsWindow.Count; i++)
            {
                tagsProp.GetArrayElementAtIndex(i).objectReferenceValue = tagsWindow[i];
            }
        }

        // Equippable fields
        SerializedProperty equipProp = so.FindProperty("equipSlotType");
        if (equipProp != null)
        {
            equipProp.enumValueIndex = (int)equipSlotType;
        }

        SetEquippableNumericProps(so);

        // itemEffects (array)
        SerializedProperty effectsProp = so.FindProperty("itemEffects");
        if (effectsProp != null)
        {
            effectsProp.arraySize = itemEffectsWindow.Count;
            for (int i = 0; i < itemEffectsWindow.Count; i++)
            {
                effectsProp.GetArrayElementAtIndex(i).objectReferenceValue = itemEffectsWindow[i];
            }
        }

        // damageBonusPercentages (struct)
        SetDamageStructToProperty(so, "damageBonusPercentages", damageBonusPercentagesWindow);

        // Weapon specific field
        SerializedProperty wTypeProp = so.FindProperty("weaponType");
        if (wTypeProp != null)
        {
            wTypeProp.enumValueIndex = (int)weaponType;
        }

        // WeaponDamageScalings (struct)
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

        // Base item fields
        so.FindProperty("id").stringValue = ItemIDGenerator.Generate();
        so.FindProperty("itemName").stringValue = itemName;
        so.FindProperty("itemDescription").stringValue = itemDescription;
        so.FindProperty("itemIcon").objectReferenceValue = itemIcon;
        so.FindProperty("itemMesh").objectReferenceValue = itemMesh;
        so.FindProperty("maxStackSize").intValue = Mathf.Max(1, maxStackSize);

        // set tags
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

        // itemEffects (array)
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

    // Helper to set the integer/float bonus fields that live on EquippableItem
    private void SetEquippableNumericProps(SerializedObject so)
    {
        SerializedProperty bonusStrProp = so.FindProperty("BonusStrength");
        if (bonusStrProp != null)
        {
            bonusStrProp.intValue = bonusStrength;
        }

        SerializedProperty bonusAgiProp = so.FindProperty("BonusAgility");
        if (bonusAgiProp != null)
        {
            bonusAgiProp.intValue = bonusAgility;
        }

        SerializedProperty bonusIntProp = so.FindProperty("BonusIntelligence");
        if (bonusIntProp != null)
        {
            bonusIntProp.intValue = bonusIntelligence;
        }

        SerializedProperty critChanceProp = so.FindProperty("BonusCriticalChance");
        if (critChanceProp != null)
        {
            critChanceProp.floatValue = bonusCriticalChance;
        }

        SerializedProperty critDmgProp = so.FindProperty("BonusCriticalDamage");
        if (critDmgProp != null)
        {
            critDmgProp.floatValue = bonusCriticalDamage;
        }
    }

    // Copies the DamageStruct values from the window mirror into the target SerializedObjects struct property
    private void SetDamageStructToProperty(SerializedObject so, string propertyName, DamageStruct source)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop == null)
        {
            return;
        }

        SerializedProperty pNone = prop.FindPropertyRelative("None");
        if (pNone != null)
        {
            pNone.floatValue = source.None;
        }
        SerializedProperty pPhysical = prop.FindPropertyRelative("Physical");
        if (pPhysical != null)
        {
            pPhysical.floatValue = source.Physical;
        }

        SerializedProperty pMagical = prop.FindPropertyRelative("Magical");
        if (pMagical != null)
        {
            pMagical.floatValue = source.Magical;
        }
        SerializedProperty pTrue = prop.FindPropertyRelative("True");
        if (pTrue != null)
        {
            pTrue.floatValue = source.True;
        }
        SerializedProperty pFire = prop.FindPropertyRelative("Fire");
        if (pFire != null)
        {
            pFire.floatValue = source.Fire;
        }
        SerializedProperty pLightning = prop.FindPropertyRelative("Lightning");
        if (pLightning != null)
        {
            pLightning.floatValue = source.Lightning;
        }
        SerializedProperty pIce = prop.FindPropertyRelative("Ice");
        if (pIce != null)
        {
            pIce.floatValue = source.Ice;
        }
        SerializedProperty pEarth = prop.FindPropertyRelative("Earth");
        if (pEarth != null)
        {
            pEarth.floatValue = source.Earth;
        }
        SerializedProperty pWind = prop.FindPropertyRelative("Wind");
        if (pWind != null)
        {
            pWind.floatValue = source.Wind;
        }
        SerializedProperty pWater = prop.FindPropertyRelative("Water");
        if (pWater != null)
        {
            pWater.floatValue = source.Water;
        }
    }

    // Scan folder and add valid Item assets to the database (avoiding duplicates)
    private void ScanFolderAndAddItems()
    {
        lastScanReport = string.Empty;
        lastScanAdded.Clear();
        lastScanSkipped = new List<Item>();

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

        // Find all asset GUIDs of Items (and subclasses) under the folder
        string[] guids = AssetDatabase.FindAssets("t:Item", new[] { folderPath });
        int added = 0;
        int skipped = 0;
        int errors = 0;

        // Ensure database lookup is built (so GetByID will work)
        database.BuildLookup();

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // If non-recursive, only accept items whose immediate parent folder matches
            if (!recursiveScan)
            {
                var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
                var normalizedFolder = folderPath.Replace('\\', '/').TrimEnd('/');
                if (!string.Equals(parent, normalizedFolder, StringComparison.OrdinalIgnoreCase))
                    continue;
            }

            Item asset = AssetDatabase.LoadAssetAtPath<Item>(path);
            if (asset == null)
            {
                errors++;
                continue;
            }

            string id = asset.GetID();

            // Treat empty or "0" id as missing
            if (string.IsNullOrWhiteSpace(id) || id.Trim() == "0")
            {
                if (assignMissingIDs)
                {
                    SerializedObject so = new SerializedObject(asset);
                    var prop = so.FindProperty("id");
                    if (prop != null)
                    {
                        prop.stringValue = ItemIDGenerator.Generate();
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

            // Check duplicate by ID or by reference
            bool duplicate = false;
            if (!string.IsNullOrEmpty(id))
            {
                var existing = database.GetByID(id);
                if (existing != null)
                {
                    duplicate = true;
                }
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

            // Add to database
            database.items.Add(asset);
            lastScanAdded.Add(asset);
            added++;
        }

        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();

        lastScanReport = $"Scan finished. Added: {added}, Skipped (duplicates/missing): {skipped}, Errors: {errors}";
        // refresh DB lookup
        database.BuildLookup();
    }

    // Ensure folder exists (creates missing nested folders) and return a unique asset path for the filename provided.
    private string EnsureFolderAndGetUniquePath(string folderPath, string fileNameWithoutExtension)
    {
        if (string.IsNullOrWhiteSpace(fileNameWithoutExtension))
        {
            fileNameWithoutExtension = "NewItem";
        }

        // normalize folder path
        folderPath = folderPath.Replace('\\', '/').TrimEnd('/');

        if (!folderPath.StartsWith("Assets"))
        {
            // enforce assets
            folderPath = Path.Combine("Assets", folderPath).Replace('\\', '/');
        }

        // create each folder segment if missing
        var parts = folderPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
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

    // Sanitize filename by removing invalid chars and trimming.
    private string SanitizeFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "NewItem";

        var invalid = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder();
        foreach (var c in name)
        {
            if (Array.IndexOf(invalid, c) >= 0)
            {
                continue;
            }

            sb.Append(c);
        }

        var result = sb.ToString().Trim();
        if (string.IsNullOrEmpty(result))
            return "NewItem";
        // additionally replace forward/back slashes if present
        result = result.Replace("/", "_").Replace("\\", "_");
        return result;
    }

    // Rebuild database lookup and remove entries that no longer exist on disk.
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

        // First: assign IDs to items that have empty/"0" ids
        bool assignedAny = false;
        for (int i = 0; i < database.items.Count; i++)
        {
            var itm = database.items[i];
            if (itm == null) continue;
            var id = itm.GetID();
            if (string.IsNullOrWhiteSpace(id) || id.Trim() == "0")
            {
                var so = new SerializedObject(itm);
                var prop = so.FindProperty("id");
                if (prop != null)
                {
                    prop.stringValue = ItemIDGenerator.Generate();
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
        // iterate backwards to safely remove entries
        for (int i = database.items.Count - 1; i >= 0; i--)
        {
            var item = database.items[i];
            bool shouldRemove = false;

            if (item == null)
            {
                shouldRemove = true;
            }
            else
            {
                var path = AssetDatabase.GetAssetPath(item);
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    shouldRemove = true;
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
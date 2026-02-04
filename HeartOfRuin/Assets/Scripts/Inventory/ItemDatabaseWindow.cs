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

    private enum MainTab { ItemCreation = 0, Database = 1 }
    private MainTab mainTab = MainTab.ItemCreation;

    private enum CreateTab { Item = 0, Armour = 1, Weapon = 2, Artifact = 3 }
    private CreateTab createTab = CreateTab.Item;

    // Common fields
    private string itemName = "New Item";
    private Sprite itemIcon;
    private string itemDescription;
    private GameObject itemMesh;
    private int maxStackSize = 1;

    // Equippable fields (used for Armour, Weapon, and Artifact)
    private EquipSlotType equipSlotType = EquipSlotType.None;
    private int bonusStrength = 0;
    private int bonusAgility = 0;
    private int bonusIntelligence = 0;
    private float bonusCriticalChance = 0f;
    private float bonusCriticalDamage = 0f;

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

    // Folder scan state
    private DefaultAsset folderToScan;
    private bool recursiveScan = true;
    private bool assignMissingIDs = true;
    private string lastScanReport = string.Empty;
    private List<Item> lastScanAdded = new List<Item>();
    private List<Item> lastScanSkipped = new List<Item>();
    private Vector2 scanScrollPos;

    [MenuItem("Tools/Items/Item Creator")]
    public static void Open()
    {
        GetWindow<ItemDatabaseWindow>("Item Creator");
    }

    private void OnGUI()
    {
        database = (ItemDatabase)EditorGUILayout.ObjectField(
            "Item Database", database, typeof(ItemDatabase), false);

        EditorGUILayout.Space();

        // Top-level: Item Creation vs Database
        string[] mainTabs = new[] { "Item Creation", "Database" };
        mainTab = (MainTab)GUILayout.Toolbar((int)mainTab, mainTabs);

        EditorGUILayout.Space();

        if (mainTab == MainTab.ItemCreation)
        {
            DrawCreationSection();
        }
        else
        {
            DrawDatabaseViewer();
        }
    }

    private void DrawCreationSection()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Item Creation", EditorStyles.boldLabel);

        // Creation sub-tabs
        string[] createTabs = new string[] { "Item", "Armour", "Weapon", "Artifact" };
        createTab = (CreateTab)GUILayout.Toolbar((int)createTab, createTabs);

        EditorGUILayout.Space();

        // Common fields displayed for all creation tabs
        itemName = EditorGUILayout.TextField("Item Name", itemName);
        itemIcon = (Sprite)EditorGUILayout.ObjectField("Icon", itemIcon, typeof(Sprite), false);
        itemDescription = EditorGUILayout.TextField("Description", itemDescription);
        itemMesh = (GameObject)EditorGUILayout.ObjectField("World Mesh", itemMesh, typeof(GameObject), false);
        maxStackSize = EditorGUILayout.IntField("Max Stack Size", maxStackSize);

        // If equippable tab or equippable-related tabs, show equippable options
        if (createTab != CreateTab.Item)
        {
            EditorGUILayout.Space();
            GUILayout.Label("Equippable Properties", EditorStyles.boldLabel);

            if (createTab != CreateTab.Artifact)
            {
                equipSlotType = (EquipSlotType)EditorGUILayout.EnumPopup("Equip Slot Type", equipSlotType);
            }

            bonusStrength = EditorGUILayout.IntField("Bonus Strength", bonusStrength);
            bonusAgility = EditorGUILayout.IntField("Bonus Agility", bonusAgility);
            bonusIntelligence = EditorGUILayout.IntField("Bonus Intelligence", bonusIntelligence);
            bonusCriticalChance = EditorGUILayout.FloatField("Bonus Critical Chance", bonusCriticalChance);
            bonusCriticalDamage = EditorGUILayout.FloatField("Bonus Critical Damage", bonusCriticalDamage);

            EditorGUILayout.Space();
            GUILayout.Label("Damage Bonus Percentages", EditorStyles.miniBoldLabel);
            DrawDamageStructFields(ref damageBonusPercentagesWindow);

            if (createTab == CreateTab.Weapon)
            {
                EditorGUILayout.Space();
                GUILayout.Label("Weapon Properties", EditorStyles.boldLabel);
                weaponType = (WeaponType)EditorGUILayout.EnumPopup("Weapon Type", weaponType);

                EditorGUILayout.Space();
                GUILayout.Label("Weapon Damage Scalings", EditorStyles.miniBoldLabel);
                DrawDamageStructFields(ref weaponDamageScalingsWindow);

                EditorGUILayout.HelpBox("Complex fields also editable after creation in the Inspector.", MessageType.Info);
            }
        }

        EditorGUILayout.Space();

        GUI.enabled = database != null && !string.IsNullOrWhiteSpace(itemName);

        if (GUILayout.Button("Create"))
        {
            CreateSelectedItem(createTab);
        }

        GUI.enabled = true;

        EditorGUILayout.EndVertical();
    }

    // Database viewer: table-like list of items with basic actions and folder-scan functionality
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
                        EditorGUILayout.LabelField($"  + {it.GetItemName()} ({AssetDatabase.GetAssetPath(it)})");
                }
                if (lastScanSkipped.Count > 0)
                {
                    EditorGUILayout.LabelField("Skipped:");
                    foreach (var it in lastScanSkipped)
                        EditorGUILayout.LabelField($"  - {it.GetItemName()} ({AssetDatabase.GetAssetPath(it)})");
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

        // Headers
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Icon", EditorStyles.miniBoldLabel, GUILayout.Width(40));
        GUILayout.Label("Name", EditorStyles.miniBoldLabel, GUILayout.Width(200));
        GUILayout.Label("Type", EditorStyles.miniBoldLabel, GUILayout.Width(100));
        GUILayout.Label("Equip Slot", EditorStyles.miniBoldLabel, GUILayout.Width(100));
        GUILayout.Label("Max Stack", EditorStyles.miniBoldLabel, GUILayout.Width(70));
        GUILayout.Label("ID", EditorStyles.miniBoldLabel, GUILayout.Width(200));
        GUILayout.Label("Actions", EditorStyles.miniBoldLabel);
        EditorGUILayout.EndHorizontal();

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

        for (int idx = 0; idx < displayList.Count; idx++)
        {
            var item = displayList[idx];
            if (item == null) continue;

            EditorGUILayout.BeginHorizontal("box");

            // Icon
            Texture2D tex = item.GetItemIcon() != null ? item.GetItemIcon().texture : null;
            if (tex != null)
                GUILayout.Label(tex, GUILayout.Width(40), GUILayout.Height(40));
            else
                GUILayout.Label(GUIContent.none, GUILayout.Width(40), GUILayout.Height(40));

            // Name
            GUILayout.Label(item.GetItemName() ?? "(Unnamed)", GUILayout.Width(200));

            // Type
            GUILayout.Label(item.GetType().Name, GUILayout.Width(100));

            // Equip slot (if EquippableItem)
            string equipSlotStr = item is EquippableItem eq ? eq.GetEquipSlotType().ToString() : "-";
            GUILayout.Label(equipSlotStr, GUILayout.Width(100));

            // Max stack
            GUILayout.Label(item.GetMaxStackSize().ToString(), GUILayout.Width(70));

            // ID (allow copying)
            GUILayout.Label(item.GetID() ?? string.Empty, GUILayout.Width(200));

            // Actions
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
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
                    // Remove from database list first (safe), then delete asset
                    database.items.Remove(item);
                    EditorUtility.SetDirty(database);

                    AssetDatabase.DeleteAsset(path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    // ensure we stop iterating safely by breaking to refresh UI
                    break;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    // Small helper to render the DamageStruct editable fields inside the window
    private void DrawDamageStructFields(ref DamageStruct structField)
    {
        structField.None = EditorGUILayout.FloatField("None", structField.None);
        structField.Physical = EditorGUILayout.FloatField("Physical", structField.Physical);
        structField.Magical = EditorGUILayout.FloatField("Magical", structField.Magical);
        structField.True = EditorGUILayout.FloatField("True", structField.True);
        structField.Fire = EditorGUILayout.FloatField("Fire", structField.Fire);
        structField.Lightning = EditorGUILayout.FloatField("Lightning", structField.Lightning);
        structField.Ice = EditorGUILayout.FloatField("Ice", structField.Ice);
        structField.Earth = EditorGUILayout.FloatField("Earth", structField.Earth);
        structField.Wind = EditorGUILayout.FloatField("Wind", structField.Wind);
        structField.Water = EditorGUILayout.FloatField("Water", structField.Water);
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

        // Equippable fields (field names live in EquippableItem.cs)
        SerializedProperty equipProp = so.FindProperty("equipSlotType");
        if (equipProp != null)
        {
            equipProp.enumValueIndex = (int)equipSlotType;
        }

        SetEquippableNumericProps(so);

        // damageBonusPercentages (struct)
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

        // Equippable fields
        SerializedProperty equipProp = so.FindProperty("equipSlotType");
        if (equipProp != null)
        {
            equipProp.enumValueIndex = (int)equipSlotType;
        }

        SetEquippableNumericProps(so);

        // damageBonusPercentages (struct)
        SetDamageStructToProperty(so, "damageBonusPercentages", damageBonusPercentagesWindow);

        // Weapon specific field
        SerializedProperty wTypeProp = so.FindProperty("weaponType");
        if (wTypeProp != null)
        {
            wTypeProp.enumValueIndex = (int)weaponType;
        }

        // WeaponDamageScalings (struct) - copy from window mirror
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

        SetEquippableNumericProps(so);
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

    // Copies the DamageStruct values from the window mirror into the target SerializedObject's struct property
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

            // If missing ID and option to assign is enabled, assign one
            if (string.IsNullOrEmpty(id))
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
                    duplicate = true;
            }
            if (!duplicate && database.items.Contains(asset))
                duplicate = true;

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
            fileNameWithoutExtension = "NewItem";

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
                continue;
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
            return;

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

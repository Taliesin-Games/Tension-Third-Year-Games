using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class ItemDatabaseWindow
{
	private ItemDatabase database;

	private enum MainTab { ItemCreation = 0, Database = 1, Tags = 2, LootTables = 3, EditItem = 4 }
	private MainTab mainTab = MainTab.ItemCreation;

	private enum CreateTab { Item = 0, Armour = 1, Weapon = 2, Artifact = 3 }
	private CreateTab createTab = CreateTab.Item;

	// Common fields
	private string itemName = "New Item";
	private Sprite itemIcon;
	private string itemDescription;
	private GameObject itemMesh;
	private int maxStackSize = 1;
	private ItemRarity itemRarity = ItemRarity.common;

	// Tags mirror for item creation
	private List<tg_ItemTag> tagsWindow = new List<tg_ItemTag>();
	private tg_ItemTag newTagSelection = null;

	// Tag management UI state
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

	// Equippable fields
	private EquipSlotType equipSlotType = EquipSlotType.None;
	private int bonusStrength = 0;
	private int bonusAgility = 0;
	private int bonusIntelligence = 0;
	private float bonusCriticalChance = 0f;
	private float bonusCriticalDamage = 0f;
	private List<ItemEffect> itemEffectsWindow = new List<ItemEffect>();

	// DamageStruct mirrors
	private DamageStruct damageBonusPercentagesWindow;
	private DamageStruct weaponDamageScalingsWindow;

	// Weapon specific
	private WeaponType weaponType = WeaponType.None;

	// Database viewer state
	private Vector2 dbScrollPos;
	private int dbSelectedIndex = -1;
	private string dbSearch = string.Empty;
	private enum DbSort { None = 0, Name = 1, Type = 2, MaxStack = 3 }
	private DbSort dbSort = DbSort.None;
	private HashSet<int> dbExpandedIDs = new HashSet<int>();

	// Editor / Edit-item state
	private Item editItem = null;
	private SerializedObject editItemSO = null;
	private Vector2 editScrollPos;
	private string editItemSnapshotJson = null;

	// Folder scan state
	private DefaultAsset folderToScan;
	private bool recursiveScan = true;
	private bool assignMissingIDs = true;
	private string lastScanReport = string.Empty;
	private List<Item> lastScanAdded = new List<Item>();
	private List<Item> lastScanSkipped = new List<Item>();
	private Vector2 scanScrollPos;

	// Creation scroll position
	private Vector2 creationScrollPos;

	// Loot Tables UI state
	private Vector2 lootScrollPos;
	private LootTable selectedLootTable = null;
	private SerializedObject selectedLootTableSO = null;
	private string selectedLootTableSnapshotJson = null;
	private bool selectedLootTableDirty = false;
	private Vector2 lootEditScrollPos;
	private string newLootTableName = "New Loot Table";
	private LootTable addExistingLootSelection = null;

	// Quick-entry fields
	private string quickEntrySearch = string.Empty;
	private Item quickEntrySelectedItem = null;
	private float quickEntryWeight = 1f;
	private float quickEntryChance = 1f;
	private int quickEntryMin = 1;
	private int quickEntryMax = 1;
	private bool quickEntryUnique = true;

	// GUIContent tooltips
	private static readonly GUIContent kItemName = new GUIContent("Item Name", "Name of the item");
	private static readonly GUIContent kItemIcon = new GUIContent("Icon", "Icon representing the item in the inventory UI");
	private static readonly GUIContent kItemDescription = new GUIContent("Description", "Description of the item");
	private static readonly GUIContent kItemMesh = new GUIContent("World Mesh", "3D model of the item for world representation");
	private static readonly GUIContent kMaxStackSize = new GUIContent("Max Stack Size", "Max number of items that can be stacked into a single inventory slot");
	private static readonly GUIContent kItemRarity = new GUIContent("Rarity", "Rarity level of the item (common > cosmic)");

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
}
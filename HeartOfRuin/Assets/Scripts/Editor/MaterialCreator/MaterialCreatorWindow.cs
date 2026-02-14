using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

public class MaterialCreatorWindow : EditorWindow
{
    #region Constants and read only
    const float MIN_WIDTH = 300f;
    const float COLUMN_MIN_WIDTH = 200f;
    const int COLUMN_COUNT = 3;
    const float MIN_HEIGHT = 400f;

    readonly List<string> ignoredFolders = new List<string>() {
        "Assets/_3rdParty",
        "Assets/_2ndParty"
    };

    readonly string[] mapTypes =
    {
        "base",
        "normal",
        "metal",
        "smoothness",
        "ao",
        "emission"
    };
    #endregion

    #region Styling Settings
    GUILayoutOption[] ColumnLayoutOptions =>
       new GUILayoutOption[]
       {
            GUILayout.MinWidth(COLUMN_MIN_WIDTH),
            GUILayout.Width(position.width / COLUMN_COUNT),
            GUILayout.ExpandWidth(false)
       };

    // Font styling for material warning button
    GUIStyle materialWarningStyle;
    GUIStyle MaterialWarningStyle
    {
        get
        {
            var defaultNew = new GUIStyle(EditorStyles.miniButton)
            {
                wordWrap = true,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(6, 6, 4, 4),
                stretchHeight = true,
                
                
            };
            defaultNew.normal.textColor = Color.yellow;
            materialWarningStyle ??= defaultNew;
            return materialWarningStyle;
        }
    }
       
    // Button layout options for material warning
    GUILayoutOption[] materialWarningLayoutOptions;
    GUILayoutOption[] MaterialWarningLayoutOptions
    { 
        get 
        {
            var defaultNew = new GUILayoutOption[]
                {
                    GUILayout.MinWidth(COLUMN_MIN_WIDTH),
                    GUILayout.Width(position.width / COLUMN_COUNT),
                    GUILayout.ExpandWidth(false),
                    GUILayout.ExpandHeight(false),
                };

            materialWarningLayoutOptions ??= defaultNew;

            return materialWarningLayoutOptions;
        } 
    } 

    #endregion

    #region Runtime Variables
    Vector2 _horizontalScroll;
    Vector2 _fbxScroll;
    Vector2 _channelScroll;
    Vector2 _materialScroll;

    List<FbxScanResult> fbxFiles = new();
    List<string> ignoredFiles = new();
    List<string> materialChannels = new();
    List<string> _mockMaterials = new();

    private int selectedIndex = -1;
    #endregion

    #region Properties
    static Vector2 MinWindowSize => new Vector2(MIN_WIDTH, MIN_HEIGHT);
    bool EnableHorizontalScroll => position.width < COLUMN_MIN_WIDTH * COLUMN_COUNT;
    #endregion
    [MenuItem("Tools/Material Creator")]
    public static void OpenWindow()
    {
        MaterialCreatorWindow window = GetWindow<MaterialCreatorWindow>("Material Creator");
        window.minSize = MinWindowSize;
    }

    void OnEnable()
    {
        minSize = MinWindowSize;

        if (fbxFiles.Count == 0) FindModelData();
    }

    void FindModelData()
    {
        fbxFiles.Clear();
        materialChannels.Clear();
        _mockMaterials.Clear();

        // Get fbx files first
        GetFBXFiles();

        // Then find material channels
        GetChannels(selectedIndex);

        // Then find material files
        _mockMaterials = new List<string>
        {
            "Tree_Trunk.mat",
            "Tree_Branches.mat"
        };
    }

    void GetFBXFiles()
    {
        string[] guids = AssetDatabase.FindAssets("t:Model");

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (!path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase)) continue;
            // Check if path contains any strings from ignoredFolders
            if (IsInIgnoreList(path)) continue;

            if (ignoredFiles.Contains(path)) continue;
            // Store path
            FbxScanResult newFBX = new();
            // Store full path including filename
            newFBX.FbxPath = path;
            newFBX.FbxName = Path.GetFileName(path);
            // Store folder path without file name
            newFBX.FolderPath = Path.GetDirectoryName(path);

            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            newFBX.importMode = importer.materialImportMode;

            fbxFiles.Add(newFBX);
        }

        // Clear selection if list gets shorter
        if (selectedIndex >= fbxFiles.Count) selectedIndex = -1;


    }

    bool IsInIgnoreList(string path)
    {
        foreach(string f in ignoredFolders)
        {
            if (path.Contains(f)) return true;
        }

        return false;
    }
    void GetChannels(int selected)
    {
        if (selectedIndex < 0) selected = 0;
        if (fbxFiles.Count == 0) return;

        FbxScanResult result = fbxFiles[selected];
        result.MaterialSlots.Clear();   // Make sure we clear results incase this was prepopulated
        materialChannels.Clear();       // These get connected by reference but clear both incased they are not connected yet.

        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(result.FbxPath);

        var renderers = model.GetComponentsInChildren<Renderer>(true);

        foreach (var r in renderers)
        {
            foreach (var mat in r.sharedMaterials)
            {
                if (mat == null) continue;

                result.MaterialSlots.Add(mat.name);
            }
        }

        materialChannels = result.MaterialSlots;
    }
    void OnGUI()
    {
        DrawToolbar();

        if (EnableHorizontalScroll) _horizontalScroll = EditorGUILayout.BeginScrollView(_horizontalScroll, false, true);

        EditorGUILayout.BeginHorizontal(GUILayout.MinWidth(MIN_WIDTH));

        DrawFbxColumn();
        DrawChannelColumn();
        DrawMaterialColumn();

        EditorGUILayout.EndHorizontal();
        if (EnableHorizontalScroll) EditorGUILayout.EndScrollView();

        DrawInfoPanel();
    }
    void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton)) FindModelData();
        if (GUILayout.Button("Clear Ignored", EditorStyles.toolbarButton))
        {
            ignoredFiles.Clear();
            FindModelData();
        }

        GUILayout.FlexibleSpace();

        EditorGUILayout.EndHorizontal();
    }
    void DrawFbxColumn()
    {
        EditorGUILayout.BeginVertical(ColumnLayoutOptions);
        EditorGUILayout.LabelField("FBX Files", EditorStyles.boldLabel);

        _fbxScroll = EditorGUILayout.BeginScrollView(_fbxScroll);
        
        for (int i = 0; i < fbxFiles.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            int newIndex = selectedIndex;

            // Add a button to select the file
            if (GUILayout.Toggle(selectedIndex == i, fbxFiles[i].FbxName, "Button")) newIndex = i;

            if (newIndex != selectedIndex)
            {
                selectedIndex = newIndex;
                RefreshSelection(); // calls GetChannels + later texture/material detection
                Repaint();
            }

            // Create a button to hide the file
            if (GUILayout.Button("👁️", EditorStyles.toolbarButton, GUILayout.MaxWidth(50)))
            {
                ignoredFiles.Add(fbxFiles[i].FbxPath);
                fbxFiles.Remove(fbxFiles[i]);
                RefreshSelection();
            }

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }
    void RefreshSelection()
    {
        materialChannels.Clear();
        _mockMaterials.Clear();

        if (selectedIndex < 0 || selectedIndex >= fbxFiles.Count) return;

        GetChannels(selectedIndex);
        // later: GetTextures(selectedIndex), GetExistingMaterials(selectedIndex)
    }
    void DrawChannelColumn()
    {
        EditorGUILayout.BeginVertical(ColumnLayoutOptions);
        EditorGUILayout.LabelField("Material Channels / Textures", EditorStyles.boldLabel);

        if (materialChannels.Count <= 0 || selectedIndex < 0)
        {
            EditorGUILayout.EndVertical();
            return;
        }

        // Draw a warning button if import mode is incorrect.
        if (fbxFiles[selectedIndex].importMode != ModelImporterMaterialImportMode.ImportViaMaterialDescription)
        {
            
            bool pressed = GUILayout.Button(
                "!-Warning-! fbx file materials disabled.\nUnable to read material meta data.\nClick here to fix this.", 
                MaterialWarningStyle, 
                MaterialWarningLayoutOptions
                );
            if (pressed) SetMaterialImportMode(selectedIndex); 
            
        }

        _channelScroll = EditorGUILayout.BeginScrollView(_channelScroll);

        // Draw each channel
        foreach (var c in materialChannels)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(c);
            EditorGUILayout.EndHorizontal();
        }
            

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void SetMaterialImportMode(int selectedIndex)
    {

        FbxScanResult selectedFile = fbxFiles[selectedIndex];

        ModelImporter importer = AssetImporter.GetAtPath(selectedFile.FbxPath) as ModelImporter;

        importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
        selectedFile.importMode = importer.materialImportMode;
        importer.SaveAndReimport();
        GetChannels(selectedIndex);
    }

    void DrawMaterialColumn()
    {
        EditorGUILayout.BeginVertical(ColumnLayoutOptions);
        EditorGUILayout.LabelField("Existing Materials", EditorStyles.boldLabel);

        _materialScroll = EditorGUILayout.BeginScrollView(_materialScroll);

        foreach (var m in _mockMaterials)
            EditorGUILayout.LabelField(m);

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    void DrawInfoPanel()
    {
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Naming convention: ObjectName_MapType.ext\n" +
            "Example: Tree_base.png\n" +
            "Materials stored in FBX folder under 'Materials'.",
            MessageType.Info);
    }
}

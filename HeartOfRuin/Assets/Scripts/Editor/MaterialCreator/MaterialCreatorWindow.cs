using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

public class MaterialCreatorWindow : EditorWindow
{
    
    const float MIN_WIDTH = 300f;
    const float COLUMN_MIN_WIDTH = 200f;
    const int COLUMN_COUNT = 3;
    const float MIN_HEIGHT = 400f;

    List<string> ignoredFolders = new List<string>() {
        "Assets/_3rdParty",
        "Assets/_2ndParty"
    };

    string[] mapTypes =
    {
        "base",
        "normal",
        "metal",
        "smoothness",
        "ao",
        "emission"
    };


    Vector2 _horizontalScroll;
    Vector2 _fbxScroll;
    Vector2 _channelScroll;
    Vector2 _materialScroll;

    List<FbxScanResult> fbxFiles = new();
    List<string> ignoredFiles = new();
    List<string> _mockChannels = new();
    List<string> _mockMaterials = new();

    private int selectedIndex = -1;

    GUILayoutOption[] ColumnLayoutOptions =>
    new GUILayoutOption[]
    {
        GUILayout.MinWidth(COLUMN_MIN_WIDTH),
        GUILayout.Width(position.width / COLUMN_COUNT),
        GUILayout.ExpandWidth(false)
    };

    static Vector2 MinWindowSize => new Vector2(MIN_WIDTH, MIN_HEIGHT);
    bool EnableHorizontalScroll => position.width < COLUMN_MIN_WIDTH * COLUMN_COUNT;

    [MenuItem("Tools/Material Creator")]
    public static void OpenWindow()
    {
        MaterialCreatorWindow window = GetWindow<MaterialCreatorWindow>("Material Creator");
        window.minSize = MinWindowSize;
    }

    private void OnEnable()
    {
        minSize = MinWindowSize;

        if (fbxFiles.Count == 0) FindModelData();
    }

    private void FindModelData()
    {
        fbxFiles.Clear();
        _mockChannels.Clear();
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
            fbxFiles.Add(newFBX);
        }

        // Clear selection if list gets shorter
        if (selectedIndex < fbxFiles.Count) selectedIndex = -1;


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

        _mockChannels.Clear();
        _mockChannels = result.MaterialSlots;
    }
    private void OnGUI()
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

    private void DrawToolbar()
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

    private void DrawFbxColumn()
    {
        EditorGUILayout.BeginVertical(ColumnLayoutOptions);
        EditorGUILayout.LabelField("FBX Files", EditorStyles.boldLabel);

        _fbxScroll = EditorGUILayout.BeginScrollView(_fbxScroll);
        
        for (int i = 0; i < fbxFiles.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Toggle(selectedIndex == i, fbxFiles[i].FbxName, "Button"))
            {
                selectedIndex = i;
                //GetChannels(selectedIndex);
            }
            if (GUILayout.Button("👁️", EditorStyles.toolbarButton, GUILayout.MaxWidth(50)))
            {
                ignoredFiles.Add(fbxFiles[i].FbxPath);
                fbxFiles.Remove(fbxFiles[i]);
            }

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawChannelColumn()
    {
        EditorGUILayout.BeginVertical(ColumnLayoutOptions);
        EditorGUILayout.LabelField("Material Channels / Textures", EditorStyles.boldLabel);

        _channelScroll = EditorGUILayout.BeginScrollView(_channelScroll);

        //GetChannels(selectedIndex);

        foreach (var c in _mockChannels)
            EditorGUILayout.LabelField(c);

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawMaterialColumn()
    {
        EditorGUILayout.BeginVertical(ColumnLayoutOptions);
        EditorGUILayout.LabelField("Existing Materials", EditorStyles.boldLabel);

        _materialScroll = EditorGUILayout.BeginScrollView(_materialScroll);

        foreach (var m in _mockMaterials)
            EditorGUILayout.LabelField(m);

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawInfoPanel()
    {
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Naming convention: ObjectName_MapType.ext\n" +
            "Example: Tree_base.png\n" +
            "Materials stored in FBX folder under 'Materials'.",
            MessageType.Info);
    }
}

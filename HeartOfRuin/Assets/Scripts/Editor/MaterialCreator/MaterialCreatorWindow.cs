using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

public class MaterialCreatorWindow : EditorWindow
{
    const float MIN_WIDTH = 300f;
    const float COLUMN_MIN_WIDTH = 200f;
    const int COLUMN_COUNT = 3;
    const float MIN_HEIGHT = 400f;

    Vector2 _horizontalScroll;
    Vector2 _fbxScroll;
    Vector2 _channelScroll;
    Vector2 _materialScroll;

    List<string> _mockFbx = new();
    List<string> _mockChannels = new();
    List<string> _mockMaterials = new();

    private int _selectedIndex = -1;

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

        if (_mockFbx.Count == 0) GenerateMockData();
    }

    private void GenerateMockData()
    {
        _mockFbx = new List<string>
        {
            "Tree.fbx",
            "Rock.fbx",
            "Building.fbx"
        };

        _mockChannels = new List<string>
        {
            "Trunk",
            "Branches"
        };

        _mockMaterials = new List<string>
        {
            "Tree_Trunk.mat",
            "Tree_Branches.mat"
        };
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

        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton)) GenerateMockData();

        GUILayout.FlexibleSpace();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawFbxColumn()
    {
        EditorGUILayout.BeginVertical(ColumnLayoutOptions);
        EditorGUILayout.LabelField("FBX Files", EditorStyles.boldLabel);

        _fbxScroll = EditorGUILayout.BeginScrollView(_fbxScroll);

        for (int i = 0; i < _mockFbx.Count; i++)
        {
            if (GUILayout.Toggle(_selectedIndex == i, _mockFbx[i], "Button")) _selectedIndex = i;
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawChannelColumn()
    {
        EditorGUILayout.BeginVertical(ColumnLayoutOptions);
        EditorGUILayout.LabelField("Material Channels / Textures", EditorStyles.boldLabel);

        _channelScroll = EditorGUILayout.BeginScrollView(_channelScroll);

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

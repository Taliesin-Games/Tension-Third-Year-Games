#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class DebuggerSettingsInitializer
{
    private const string MarkerPath = "Assets/Settings/.debugger_settings_initialized";

    public static string SETTINGS_FOLDER_NAME = "Settings";
    public static string RESOURCES_FOLDER_NAME = "Resources";
    public static string SETTINGS_FILE_NAME = "EditorDebuggerSettings";
    public static string RESOURCES_FILE_NAME = "BuiltDebuggerSettings";

    static DebuggerSettingsInitializer()
    {
        // Already initialized? Do nothing
        if (File.Exists(MarkerPath)) return;

        // Ensure folders exist
        CreateAsset(SETTINGS_FOLDER_NAME, SETTINGS_FILE_NAME);
        CreateAsset(RESOURCES_FOLDER_NAME, RESOURCES_FILE_NAME);

        // Drop marker file so this only runs once
        File.WriteAllText(MarkerPath, "Debugger settings initialized");
        AssetDatabase.ImportAsset(MarkerPath);
    }

    static void CreateAsset(string subfolderName, string fileName)
    {
        // Ensure folders exist
        if (!AssetDatabase.IsValidFolder("Assets/" + subfolderName))
            AssetDatabase.CreateFolder("Assets", subfolderName);


        // if asset of fileName doenst exist create it.
        string[] guids = AssetDatabase.FindAssets(fileName + " t:Utils.DebuggerSettings");

        if (guids.Length == 0)
        {
            var settings = ScriptableObject.CreateInstance<Utils.DebuggerSettings>();
            AssetDatabase.CreateAsset(settings, $"Assets/{subfolderName}/{fileName}.asset");
            AssetDatabase.SaveAssets();

            Debug.Log($"Created default DebuggerSettings in Assets/{subfolderName}/{fileName}.asset");
        }
        else if (guids.Length == 1)
        {
            string[] paths = guids.Select(g => AssetDatabase.GUIDToAssetPath(g)).ToArray();
            Debug.LogWarning($"DebuggerSettings assets match prefix '{fileName}'. Matches:\n - " + string.Join("\n - ", paths) + "\n\n Please rename the current file");
        }
        else
        {
            string[] paths = guids.Select(g => AssetDatabase.GUIDToAssetPath(g)).ToArray();
            Debug.LogWarning($"Multiple DebuggerSettings assets match prefix '{fileName}'. Matches:\n - " + string.Join("\n - ", paths) + "\n\n Please rename the current files");
        }
    }
}
#endif

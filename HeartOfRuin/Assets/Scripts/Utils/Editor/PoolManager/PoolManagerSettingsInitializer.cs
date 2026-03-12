using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

using PMSIC = PoolManagerSettingsInitializerConstants;
namespace Utils
{
    [InitializeOnLoad]
    public static class PoolManagerSettingsInitializer
    {
        private const string MarkerPath = "Assets/Settings/.pool_settings_initialized";

        public static string SETTINGS_FOLDER_NAME   => PMSIC.SETTINGS_FOLDER_NAME;
        public static string RESOURCES_FOLDER_NAME  => PMSIC.RESOURCES_FOLDER_NAME;
        public static string SETTINGS_FILE_NAME     => PMSIC.SETTINGS_FILE_NAME;
        public static string RESOURCES_FILE_NAME    => PMSIC.RESOURCES_FILE_NAME;

        static PoolManagerSettingsInitializer()
        {
            // Already initialized? Do nothing
            if (File.Exists(MarkerPath)) return;

            // Ensure folders exist
            CreateAsset(SETTINGS_FOLDER_NAME, SETTINGS_FILE_NAME);
            CreateAsset(RESOURCES_FOLDER_NAME, RESOURCES_FILE_NAME);

            // Drop marker file so this only runs once
            File.WriteAllText(MarkerPath, "PoolManager settings initialized");
            AssetDatabase.ImportAsset(MarkerPath);
        }

        static void CreateAsset(string subfolderName, string fileName)
        {
            // Ensure folders exist
            if (!AssetDatabase.IsValidFolder("Assets/" + subfolderName))
                AssetDatabase.CreateFolder("Assets", subfolderName);


            // if asset of fileName doenst exist create it.
            string[] guids = AssetDatabase.FindAssets(fileName + " t:Utils.PoolManagerSettings");

            if (guids.Length == 0)
            {
                var settings = ScriptableObject.CreateInstance<Utils.PoolManagerSettings>();
                AssetDatabase.CreateAsset(settings, $"Assets/{subfolderName}/{fileName}.asset");
                AssetDatabase.SaveAssets();

                Debug.Log($"Created default PoolManagerSettings in Assets/{subfolderName}/{fileName}.asset");
            }
            else if (guids.Length == 1)
            {
                string[] paths = guids.Select(g => AssetDatabase.GUIDToAssetPath(g)).ToArray();
                Debug.LogWarning($"PoolManagerSettings assets match prefix '{fileName}'. Matches:\n - " + string.Join("\n - ", paths) + "\n\n Please rename the current file");
            }
            else
            {
                string[] paths = guids.Select(g => AssetDatabase.GUIDToAssetPath(g)).ToArray();
                Debug.LogWarning($"Multiple PoolManagerSettings assets match prefix '{fileName}'. Matches:\n - " + string.Join("\n - ", paths) + "\n\n Please rename the current files");
            }
        }
    }
}
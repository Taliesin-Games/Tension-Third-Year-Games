using UnityEngine;

namespace Utils
{
    /// <summary>
    /// Interface for accessing Pool Manager settings.
    /// Includes default settings and runtime overrides.
    ///  </summary>
    public static class PoolConfig
    {
        // Add custom getter and setter so if its accessed and doesnt exist it is created
        private static PoolManagerSettings settings;

        #region Properties
        
        #endregion
        static PoolConfig()
        {
#if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets(PoolManagerSettingsInitializerConstants.SETTINGS_FILE_NAME + " t:Utils.PoolManagerSettings");
            if (guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                settings = UnityEditor.AssetDatabase.LoadAssetAtPath<PoolManagerSettings>(path);
            }
#else
            // Build: load from Resources
            settings = Resources.Load<PoolManagerSettings>(PoolManagerSettingsInitializerConstants.RESOURCES_FILE_NAME);
#endif
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Preinitialise()
        {
            //if (GlobalLogLevel > 0) return; // Acessing a static on runtime initialize forces running the constructor and prevents thread safety issues, doing this is important.
        }
    }
}


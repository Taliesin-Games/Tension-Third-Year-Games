using UnityEngine;


namespace Utils
{
    public class PoolManager : MonoBehaviour
    {
        #region Statics
        public static PoolManager Instance;
        #endregion

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void RuntimeInit()
        {
            if (Instance != null) return;

            // Create singleton pool manager object
            GameObject poolGameObject = new GameObject("Utilities: Pool Manager");
            Instance = poolGameObject.AddComponent<PoolManager>();
            DontDestroyOnLoad(poolGameObject);

        }
    }
}
using UnityEditor;
using UnityEngine;

namespace NKStudio
{
    [CreateAssetMenu(fileName = "STFSettings", menuName = "NKStudio/STFSettings")]
    public class STFSettings : ScriptableObject
    {
        private static STFSettings _instance;
        public static STFSettings Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;
                
                _instance = Resources.Load<STFSettings>("STFSettings");

#if UNITY_EDITOR
                if (_instance == null)
                {
                    if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                    {
                        AssetDatabase.CreateFolder("Assets", "Resources");
                    }

                    _instance = AssetDatabase.LoadAssetAtPath<STFSettings>("Assets/Resources/STFSettings.asset");

                    if (_instance == null)
                    {
                        _instance = CreateInstance<STFSettings>();
                        AssetDatabase.CreateAsset(_instance, "Assets/Resources/STFSettings.asset");
                    }
                }

#endif
                
                return _instance;
            }
        }
        
        public ESnapToFloorType SnapToFloorType = ESnapToFloorType.Collider;
    }
    
    public enum ESnapToFloorType
    {
        Collider,
        NavMesh
    }
}
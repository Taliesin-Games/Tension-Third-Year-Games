using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NKStudio
{
    public class STFSettingWindow : EditorWindow
    {
        private VisualTreeAsset _uxml;
        private StyleSheet _uss;
        private TextAsset _packageJson;
        
        private void Awake()
        {
            _uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/_3rdParty/Plugins/SnapToFloor/Scripts/Editor/Settings/STFSettingUXML.uxml");
            _uss = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/_3rdParty/Plugins/SnapToFloor/Scripts/Editor/Settings/STFSettingStyleSheet.uss");
            _packageJson = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_3rdParty/Plugins/SnapToFloor/package.json");
        }

        [MenuItem("Tools/SnapToFloor/Settings")]
        public static void Init()
        {
            STFSettingWindow wnd = GetWindow<STFSettingWindow>();
            wnd.titleContent = new GUIContent("STF Settings");
            wnd.minSize = new Vector2(360, 130);
            wnd.maxSize = new Vector2(360, 130);
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;
        
            // Import UXML
            VisualTreeAsset visualTree = _uxml;
            root.styleSheets.Add(_uss);
            VisualElement container = visualTree.Instantiate();
            root.Add(container);
            
            string version = GetVersion();
            root.Q<Label>("version-label").text = $"Version : {version}";

            // Init
            EnumField snapTargetField = root.Q<EnumField>("snapTarget-field");
            snapTargetField.Init(STFSettings.Instance.SnapToFloorType);
            snapTargetField.SetValueWithoutNotify(STFSettings.Instance.SnapToFloorType);
            
            snapTargetField.RegisterValueChangedCallback(evt =>
            {
                STFSettings.Instance.SnapToFloorType = (ESnapToFloorType)evt.newValue;
            });
        }
        
        private string GetVersion()
        {
            PackageInfo packageInfo = JsonUtility.FromJson<PackageInfo>(_packageJson.text);
            return packageInfo.version;
        }
    }
    
        
    [Serializable]
    public class PackageInfo
    {
        public string name;
        public string displayName;
        public string version;
        public string unity;
        public string description;
        public List<string> keywords;
    }
}
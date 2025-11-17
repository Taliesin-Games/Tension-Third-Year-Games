#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Linq;
using UnityEditor.Build;

[CreateAssetMenu(fileName = "HoRBuildSettings", menuName = "Settings/HoR Build Settings", order = 0)]
public class HoRBuildSettings : ScriptableObject
{
    [Header("Feature Toggles")]
    [SerializeField] bool demoMode;
    [SerializeField] bool cheatsEnabled;
    [SerializeField] bool debugTools;

#if UNITY_EDITOR
    private void OnValidate()
    {
        ApplyDefines();
    }

    public void ApplyDefines()
    {
        // Use the new NamedBuildTarget API
        var namedTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

        // Get current symbols
        PlayerSettings.GetScriptingDefineSymbols(namedTarget, out string[] defines);

        // Helper lambdas
        void AddDefine(string symbol)
        {
            if (!defines.Contains(symbol))
                defines = defines.Append(symbol).ToArray();
        }

        void RemoveDefine(string symbol)
        {
            defines = defines.Where(d => d != symbol).ToArray();
        }

        // Apply toggles
        if (demoMode)       AddDefine("DEMO_MODE");     else RemoveDefine("DEMO_MODE");
        if (cheatsEnabled)  AddDefine("ENABLE_CHEATS"); else RemoveDefine("ENABLE_CHEATS");
        if (debugTools)     AddDefine("DEBUG_TOOLS");   else RemoveDefine("DEBUG_TOOLS");

        // Set updated symbols
        PlayerSettings.SetScriptingDefineSymbols(namedTarget, defines);

        Debug.Log($"[HoRBuildSettings] Updated defines for {namedTarget.TargetName}: {string.Join(";", defines)}");
    }
#endif
}
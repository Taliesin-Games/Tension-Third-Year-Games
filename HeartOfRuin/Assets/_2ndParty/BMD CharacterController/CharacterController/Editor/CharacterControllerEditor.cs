using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BMD 
{
    [CustomEditor(typeof(BMD.CharacterController), true)]
    public class CharacterControllerEditor : Editor
    {
        [InitializeOnLoad]
        public static class CharacterControllerEditorUtility
        {
            static CharacterControllerEditorUtility()
            {
                // Called whenever the hierarchy changes (components added/removed, etc.)
                EditorApplication.hierarchyChanged += CleanupModulesIfControllerRemoved;
            }

            private static void CleanupModulesIfControllerRemoved()
            {
                // Find all game objects that used to have a BMDCharacterController
                // but no longer do, yet still have orphaned modules
                foreach (var obj in UnityEngine.Object.FindObjectsOfType<GameObject>())
                {
                    // If there's no controller but there are modules...
                    if (obj.GetComponent<BMD.CharacterController>() == null)
                    {
                        var orphanModules = obj.GetComponents<ICharacterModule>();
                        if (orphanModules.Length > 0)
                        {
                            foreach (var mod in orphanModules)
                            {
                                if (mod is MonoBehaviour mb)
                                    Undo.DestroyObjectImmediate(mb);
                            }
                            Debug.Log($"[Cleanup] Removed {orphanModules.Length} orphaned module(s) from {obj.name}");
                        }
                    }
                }
            }
        }

        private static List<Type> _cachedModules;
        private bool showAddModules = false;
        private int selectedModuleIndex = 0;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var controller = (BMD.CharacterController)target;

            // Show attached modules
            var modules = controller.GetComponents<ICharacterModule>();
            if (modules.Length == 0)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Character Modules", EditorStyles.boldLabel);
                EditorGUILayout.Space(5);

                EditorGUILayout.HelpBox("No modules attached.", MessageType.Warning);
            }
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Add New Module", EditorStyles.boldLabel);
            DrawAddModuleButtons(controller);

            if (GUI.changed)
                EditorUtility.SetDirty(controller);
        }

        private void DrawAddModuleButtons(BMD.CharacterController controller)
        {
            if (_cachedModules == null)
            {
                _cachedModules = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => typeof(ICharacterModule).IsAssignableFrom(t)
                                && !t.IsInterface
                                && !t.IsAbstract
                                && typeof(MonoBehaviour).IsAssignableFrom(t))
                    .OrderBy(t => t.Name)
                    .ToList();
            }

            // Convert to names for display
            string[] moduleNames = _cachedModules.Select(t => t.Name).ToArray();

            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Select Module", GUILayout.Width(160));
            selectedModuleIndex = EditorGUILayout.Popup(selectedModuleIndex, moduleNames);

            // Only add if not already attached
            if (GUILayout.Button("Add", GUILayout.Width(60)) && selectedModuleIndex >= 0)
            {
                Type selectedType = _cachedModules[selectedModuleIndex];

                bool alreadyExists = controller.GetComponent(selectedType) != null;
                if (alreadyExists)
                {
                    EditorUtility.DisplayDialog(
                        "Duplicate Module",
                        $"A {selectedType.Name} is already attached to this object.",
                        "OK"
                    );
                }
                else
                {
                    Undo.AddComponent(controller.gameObject, selectedType);
                    EditorUtility.SetDirty(controller);

                    // Optional: auto-select the new module
                    Selection.activeObject = controller.GetComponent(selectedType);
                    EditorGUIUtility.PingObject(controller.GetComponent(selectedType));
                }

                // Reset dropdown to first item
                selectedModuleIndex = 0;
            }

            EditorGUILayout.EndHorizontal();
        }
    }

}

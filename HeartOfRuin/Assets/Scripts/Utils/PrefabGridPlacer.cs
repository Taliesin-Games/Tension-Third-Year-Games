#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class PrefabGridPlacer : EditorWindow
{
    private float spacing = 2f;
    private int columns = 5;

    [MenuItem("Tools/Place Prefabs in Grid")]
    static void ShowWindow()
    {
        GetWindow<PrefabGridPlacer>("Prefab Grid Placer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Grid Placement Settings", EditorStyles.boldLabel);
        spacing = EditorGUILayout.FloatField("Spacing", spacing);
        columns = EditorGUILayout.IntField("Columns", columns);

        if (GUILayout.Button("Place Selected Prefabs"))
        {
            PlacePrefabsInGrid();
        }
    }

    private void PlacePrefabsInGrid()
    {
        Object[] selectedObjects = Selection.objects;
        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            Debug.LogWarning("No prefabs selected.");
            return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();

        Vector3 startPosition = Vector3.zero;
        int row = 0, col = 0;

        foreach (Object obj in selectedObjects)
        {
            if (obj is GameObject prefab)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                if (instance != null)
                {
                    Undo.RegisterCreatedObjectUndo(instance, "Place Prefab Grid");

                    Vector3 position = startPosition + new Vector3(col * spacing, 0, -row * spacing);
                    instance.transform.position = position;

                    col++;
                    if (col >= columns)
                    {
                        col = 0;
                        row++;
                    }
                }
            }
        }

        Undo.CollapseUndoOperations(undoGroup);
        Debug.Log($"Placed {selectedObjects.Length} prefabs in a {columns}-column grid.");
    }
}
#endif

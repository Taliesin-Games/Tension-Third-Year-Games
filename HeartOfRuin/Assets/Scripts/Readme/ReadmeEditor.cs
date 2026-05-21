#if UNITY_EDITOR

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(Readme))]
public class ReadmeEditor : Editor
{
    const int INSPECTOR_FONT_SIZE = 16; // adjust as needed
    private bool isEditing;
    Vector2 scroll;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty textProperty = serializedObject.FindProperty("text");

        EditorGUILayout.Space();

        if (isEditing)
        {
            EditorGUILayout.LabelField("Readme", EditorStyles.boldLabel);

            var textAreaStyle = new GUIStyle(EditorStyles.textArea);
            textAreaStyle.fontSize = 16;
            textAreaStyle.wordWrap = true;

            // Estimate content height (same logic as before)
            float contentHeight = textAreaStyle.CalcHeight(
                new GUIContent(textProperty.stringValue),
                EditorGUIUtility.currentViewWidth
            );

            // Visible height (your scroll view height)
            float viewHeight = 200f;

            // Check if user is near the bottom (with small tolerance)
            bool wasAtBottom = scroll.y >= (contentHeight - viewHeight - 5f);

            // Begin scroll
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(200));

            // Draw text field
            EditorGUI.BeginChangeCheck();

            string newText = EditorGUILayout.TextArea(
                textProperty.stringValue,
                textAreaStyle,
                GUILayout.ExpandHeight(true)
            );

            if (EditorGUI.EndChangeCheck())
            {
                textProperty.stringValue = newText;

                // If text changed, scroll to bottom
                // Large Y ensures we hit the bottom regardless of content size
                if (wasAtBottom)  scroll.y = float.MaxValue;
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            if (GUILayout.Button("Save"))
            {
                isEditing = false;
                GUI.FocusControl(null);
            }
        }
        else
        {
            DrawFormattedReadme(textProperty.stringValue);

            EditorGUILayout.Space();

            if (GUILayout.Button("Edit"))
            {
                isEditing = true;
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawFormattedReadme(string text)
    {
        GUIStyle displayStyle = new GUIStyle(EditorStyles.helpBox)
        {
            richText = true,
            wordWrap = true,
            fontSize = INSPECTOR_FONT_SIZE,
            padding = new RectOffset(10, 10, 8, 8)
        };

        EditorGUILayout.LabelField(text, displayStyle);
    }
}

public static class ReadmeEditorUtility
{
    public static void MoveReadmeBelowTransform(Readme readme)
    {
        if (readme == null)
        {
            return;
        }

        EditorApplication.delayCall += () =>
        {
            if (readme == null)
            {
                return;
            }

            while (ComponentUtility.MoveComponentUp(readme))
            {
                // Moves as high as Unity permits.
                // Transform will remain above it.
            }
        };
    }
}

#endif
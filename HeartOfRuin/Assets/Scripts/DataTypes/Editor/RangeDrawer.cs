using UnityEditor;
using UnityEngine;

namespace BMD.DataTypes
{
    [CustomPropertyDrawer(typeof(IntRange))]
    [CustomPropertyDrawer(typeof(FloatRange))]
    public class RangeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position.height = EditorGUIUtility.singleLineHeight;

            Rect content = EditorGUI.PrefixLabel(position, label);

            SerializedProperty minProp = property.FindPropertyRelative("Min");
            SerializedProperty maxProp = property.FindPropertyRelative("Max");

            float spacing = 6f;
            float width = (content.width - spacing) * 0.5f;

            Rect minRect = new Rect(content.x, content.y, width, content.height);
            Rect maxRect = new Rect(content.x + width + spacing, content.y, width, content.height);

            float oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 28f;

            EditorGUI.PropertyField(minRect, minProp, new GUIContent("Min"));
            EditorGUI.PropertyField(maxRect, maxProp, new GUIContent("Max"));

            EditorGUIUtility.labelWidth = oldLabelWidth;

            EditorGUI.EndProperty();
        }
    }
}
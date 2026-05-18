using UnityEditor;
using UnityEngine;

namespace BMD.DataTypes
{
    [CustomPropertyDrawer(typeof(Vector2Range))]
    [CustomPropertyDrawer(typeof(Vector3Range))]
    public class CompositeRangeDrawer : PropertyDrawer
    {
        private const float Spacing = 6f;
        private const float LabelWidth = 32f;
        private const float MinFieldWidth = 200f;

        float AvailableWidth => EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth;
        public bool UseTwoLines => AvailableWidth < MinFieldWidth * 2f + Spacing;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {

            bool useTwoLines = AvailableWidth < MinFieldWidth * 2f + Spacing;

            return useTwoLines
                ? EditorGUIUtility.singleLineHeight * 2f + EditorGUIUtility.standardVerticalSpacing
                : EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty minProp = property.FindPropertyRelative("Min");
            SerializedProperty maxProp = property.FindPropertyRelative("Max");

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;

            Rect content = EditorGUI.PrefixLabel(
                new Rect(position.x, position.y, position.width, lineHeight),
                label
            );

            bool useTwoLines = AvailableWidth < MinFieldWidth * 2f + Spacing;

            float oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = LabelWidth;

            if (useTwoLines)
            {
                Rect minRect = new Rect(content.x, position.y, content.width, lineHeight);
                Rect maxRect = new Rect(
                    content.x,
                    position.y + lineHeight + verticalSpacing,
                    content.width,
                    lineHeight
                );

                DrawVectorField(minRect, minProp, "Min");
                DrawVectorField(maxRect, maxProp, "Max");
            }
            else
            {
                float width = (content.width - Spacing) * 0.5f;

                Rect minRect = new Rect(content.x, position.y, width, lineHeight);
                Rect maxRect = new Rect(content.x + width + Spacing, position.y, width, lineHeight);

                DrawVectorField(minRect, minProp, "Min");
                DrawVectorField(maxRect, maxProp, "Max");
            }

            EditorGUIUtility.labelWidth = oldLabelWidth;

            EditorGUI.EndProperty();
        }
        private static void DrawVectorField(Rect rect, SerializedProperty property, string label)
        {
            const float localLabelWidth = 28f;
            const float spacing = 4f;

            Rect labelRect = new Rect(
                rect.x,
                rect.y,
                localLabelWidth,
                EditorGUIUtility.singleLineHeight
            );

            Rect fieldRect = new Rect(
                rect.x + localLabelWidth + spacing,
                rect.y,
                rect.width - localLabelWidth - spacing,
                EditorGUIUtility.singleLineHeight
            );

            EditorGUI.LabelField(labelRect, label);

            if (property.propertyType == SerializedPropertyType.Vector2)
            {
                property.vector2Value = EditorGUI.Vector2Field(
                    fieldRect,
                    GUIContent.none,
                    property.vector2Value
                );
            }
            else if (property.propertyType == SerializedPropertyType.Vector3)
            {
                property.vector3Value = EditorGUI.Vector3Field(
                    fieldRect,
                    GUIContent.none,
                    property.vector3Value
                );
            }
        }
    }
}
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FluidMidi
{
    [CustomPropertyDrawer(typeof(ToggleIntFoldoutAttribute))]
    public class ToggleIntFoldoutPropertyDrawer : PropertyDrawer
    {
        private bool foldout;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attribute = this.attribute as ToggleIntFoldoutAttribute;

            EditorGUI.BeginProperty(position, label, property);
            var enabledProperty = property.FindPropertyRelative("Enabled");
            var valueProperty = property.FindPropertyRelative("Value");
            var enabled = enabledProperty.boolValue;
            var value = valueProperty.intValue;
            EditorGUI.BeginChangeCheck();
            position.height = EditorGUIUtility.singleLineHeight;
            var tooltipAttribute =
                Attribute.GetCustomAttribute(
                    property.serializedObject.targetObject.GetType().GetField(
                        property.name, BindingFlags.NonPublic | BindingFlags.Instance),
                    typeof(TooltipAttribute)) as TooltipAttribute;
            enabled = EditorGUI.Toggle(position, new GUIContent(label.text, tooltipAttribute?.tooltip), enabled);
            if (enabled)
            {
                foldout = EditorGUI.Foldout(position, foldout, GUIContent.none);
                if (foldout)
                {
                    position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    EditorGUI.indentLevel += 1;
                    value = EditorGUI.IntField(position, new GUIContent(attribute.name, attribute.tooltip), value);
                    EditorGUI.indentLevel -= 1;
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                enabledProperty.boolValue = enabled;
                valueProperty.intValue = value;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = EditorGUIUtility.singleLineHeight;
            if (foldout && property.FindPropertyRelative("Enabled").boolValue)
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            return height;
        }
    }
}
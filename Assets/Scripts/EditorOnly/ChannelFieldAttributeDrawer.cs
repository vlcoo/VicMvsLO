using UnityEditor;
using UnityEngine;

namespace EditorOnly
{
    [CustomPropertyDrawer(typeof(ChannelFieldAttribute))]
    public class ChannelFieldAttributeDrawer: PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var channels = new string[16];
            for (var i = 0; i < 16; i++) channels[i] = (i+1).ToString();
            property.intValue = EditorGUI.MaskField(position, label, property.intValue, channels);
        }
    }
}
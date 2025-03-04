#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;
using static VInspector.Libs.VUtils;
using static VInspector.Libs.VGUI;



namespace VInspector
{
    [CustomPropertyDrawer(typeof(VariantsAttribute))]
    public class VIVariantsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
        {
            var variants = ((VariantsAttribute)attribute).variants;


            EditorGUI.BeginProperty(rect, label, prop);

            var iCur = prop.hasMultipleDifferentValues ? -1 : variants.ToList().IndexOf(prop.stringValue);

            var iNew = EditorGUI.IntPopup(rect, label.text, iCur, variants, Enumerable.Range(0, variants.Length).ToArray());

            if (iNew != -1)
                prop.stringValue = variants[iNew];
            else if (!prop.hasMultipleDifferentValues)
                prop.stringValue = variants[0];

            EditorGUI.EndProperty();

        }
    }
}
#endif
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;

public class CEditorDrawUtility : Editor
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ReadOnlyAttribute : PropertyAttribute
    {
        public readonly bool runtimeOnly;

        public ReadOnlyAttribute(bool runtimeOnly = false)
        {
            this.runtimeOnly = runtimeOnly;
        }
    }

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute), true)]
    public class ReadOnlyAttributeDrawer : PropertyDrawer
    {
        // Necessary since some properties tend to collapse smaller than their content
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        // Draw a disabled property field
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = !Application.isPlaying && ((ReadOnlyAttribute)attribute).runtimeOnly;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }

    //------------------------------------------------------------------------------------------
    public static void DoDrawUItilityLine(Color _Color, int _Thickness = 2, int _Padding = 10)
    {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(_Padding + _Thickness));
        r.height = _Thickness;
        r.y += _Padding / 2;
        r.x -= 2;
        r.width += 6;
        EditorGUI.DrawRect(r, _Color);
    }


  
}

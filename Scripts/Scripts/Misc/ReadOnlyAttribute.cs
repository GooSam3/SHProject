using System;
using UnityEngine;

//Inspector에 읽기 전용으로 노출한다.
//ReadOnly 혹은 ReadOnly(false)면 항상 수정 불가능
//ReadOnly(true) 면 RunTime에 수정 불가능

#if UNITY_EDITOR
namespace UnityEditor
{
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
}
#endif

[AttributeUsage(AttributeTargets.Field)]
public class ReadOnlyAttribute : PropertyAttribute
{
    public readonly bool runtimeOnly;

    public ReadOnlyAttribute(bool runtimeOnly = false)
    {
        this.runtimeOnly = runtimeOnly;
    }
}
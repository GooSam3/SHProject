using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(ZToggle), true)]
[CanEditMultipleObjects]
public class ZToggleEditor : ToggleEditor
{
    SerializedProperty m_SoundTIDProperty;
    SerializedProperty OnObject;
    SerializedProperty OffObject;
    SerializedProperty ToggleGroup;
    SerializedProperty m_ToggleChange;
    SerializedProperty m_MultiTransitionsProperty;

    protected override void OnEnable()
    {
        base.OnEnable();

        m_SoundTIDProperty = serializedObject.FindProperty("SoundTID");
        OnObject = serializedObject.FindProperty("On");
        OffObject = serializedObject.FindProperty("Off");
        ToggleGroup = serializedObject.FindProperty("ZToggleGroup");
        m_ToggleChange = serializedObject.FindProperty("ToggleChangeStyle");
        m_MultiTransitionsProperty = serializedObject.FindProperty("MultiTransitions");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();

        serializedObject.Update();
        EditorGUILayout.PropertyField(m_SoundTIDProperty);
        EditorGUILayout.PropertyField(OnObject, true);
        EditorGUILayout.PropertyField(OffObject, true);
        EditorGUILayout.PropertyField(ToggleGroup, true);
        EditorGUILayout.PropertyField(m_ToggleChange, true);
        EditorGUILayout.PropertyField(m_MultiTransitionsProperty, true);

        if (GUILayout.Button("Add Transition(Set DefaultColor)"))
        {
            (target as ZToggle).MultiTransitions.Add(new ZSelectTransition());
        }

        serializedObject.ApplyModifiedProperties();
    }
}
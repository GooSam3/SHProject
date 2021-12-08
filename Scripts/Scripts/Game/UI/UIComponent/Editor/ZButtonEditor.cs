using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(ZButton), true)]
[CanEditMultipleObjects]
public class ZButtonEditor : ButtonEditor
{
    SerializedProperty m_SoundTIDProperty;
    SerializedProperty m_MultiTransitionsProperty;

    protected override void OnEnable()
    {
        base.OnEnable();
        m_SoundTIDProperty = serializedObject.FindProperty("SoundTID");
        m_MultiTransitionsProperty = serializedObject.FindProperty("multiTransitions");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();

        serializedObject.Update();
        EditorGUILayout.PropertyField(m_SoundTIDProperty);
        EditorGUILayout.PropertyField(m_MultiTransitionsProperty, true);

        if (GUILayout.Button("Add Transition(Set DefaultColor)"))
        {
            (target as ZButton).multiTransitions.Add(new ZSelectTransition());
        }
        serializedObject.ApplyModifiedProperties();
    }
}
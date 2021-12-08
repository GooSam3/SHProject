using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(CText), true)]
[CanEditMultipleObjects]
public class CTextEditor : TextEditor
{
	SerializedProperty m_FontSetID;
	SerializedProperty m_LocalizingKey;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_FontSetID = serializedObject.FindProperty("FontSetID");
		m_LocalizingKey = serializedObject.FindProperty("LocalizeingKey");
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		serializedObject.Update();

		EditorGUILayout.PropertyField(m_FontSetID);
		EditorGUILayout.PropertyField(m_LocalizingKey);
	}
}
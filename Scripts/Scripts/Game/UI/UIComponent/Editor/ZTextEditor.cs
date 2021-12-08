using UnityEditor;
using System;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(ZText), true)]
[CanEditMultipleObjects]
public class ZTextEditor : UnityEditor.UI.TextEditor
{
	private SerializedProperty mTextLocalID;
	private SerializedProperty mFontSetID;
	protected override void OnEnable()
	{
		base.OnEnable();
		mTextLocalID = serializedObject.FindProperty("LocalizingTID");
		mFontSetID = serializedObject.FindProperty("FontSetID");
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		EditorGUILayout.PropertyField(mTextLocalID);
		EditorGUILayout.PropertyField(mFontSetID);
		serializedObject.ApplyModifiedProperties();
	}
}

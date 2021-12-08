using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RandomRadiusComponent))]
[CanEditMultipleObjects()]
public class RandomRadiusComponentEditor : Editor
{
	private RandomRadiusComponent mTarget;

	private void OnEnable()
	{
		mTarget = target as RandomRadiusComponent;
	}

	override public void OnInspectorGUI()
	{
		EditorGUILayout.HelpBox($"원형 범위내 랜덤 이동설정", MessageType.Info, true);

		base.OnInspectorGUI();
	}

	private void OnSceneGUI()
	{
		if (null == mTarget || null == mTarget.Data)
			return;

		Handles.color = new Color(1, 0, 1, 0.2f);
		Handles.DrawSolidDisc(mTarget.transform.position, Vector3.up, mTarget.Data.Radius);

		Handles.color = Color.white;
		mTarget.Data.Radius = Handles.ScaleValueHandle(
			mTarget.Data.Radius,
			mTarget.transform.position,
			Quaternion.Euler(90, 0, 0),
			4f,
			Handles.CircleHandleCap,
			1f);
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects()]
[CustomEditor(typeof(ZSceneSettings))]
public class SceneSettingsEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		GUILayout.Space(20f);
		if (GUILayout.Button("Global Shader셋팅값 바로 적용하기 (Editor, Play Mode)", GUILayout.Height(30f)))
		{
			(target as ZSceneSettings)?.ApplyShaderSettings();
		}
	}

#if UNITY_EDITOR
	[UnityEditor.MenuItem("Assets/Create/ZSceneSettings")]
	[UnityEditor.MenuItem("ZGame/Create/ZSceneSettings")]
	public static void CreateScriptableObject()
	{
		var scriptableObject = ScriptableObject.CreateInstance<ZSceneSettings>();
		UnityEditor.ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
			scriptableObject.GetInstanceID(),
			ScriptableObject.CreateInstance<EndNameEdit>(),
			string.Format("{0}.asset", nameof(ZSceneSettings)),
			UnityEditor.AssetPreview.GetMiniThumbnail(scriptableObject),
			null);

		if (null != scriptableObject)
		{
			if (EditorUtility.DisplayDialog("파일 생성 성공!", "Scene과 같은 이름으로 설정해주세요~", "확인"))
			{
				Selection.activeObject = scriptableObject;
			}
		}
	}
#endif
}

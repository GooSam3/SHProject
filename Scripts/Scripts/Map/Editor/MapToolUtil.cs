using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;
using System.IO;
using System;

public class MapToolUtil
{
	#region ========:: GUI Helper ::========

	public static void Draw_Separator()
	{
		GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(1f));
	}

	public static bool GUI_CenterButton(string label, params GUILayoutOption[] options)
	{
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		bool ret = GUILayout.Button(label, options);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		return ret;
	}

	public static void CenterLabel(string label, GUIStyle style, params GUILayoutOption[] options)
	{
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label(label, style, options);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}

	public static void HelpBox(string label, MessageType type, bool wide)
	{
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		{
			EditorGUILayout.HelpBox(label, MessageType.Info, wide);
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}

	public static bool ToggleChanged(string label, bool value)
	{
		bool retVal = GUILayout.Toggle(value, label, GUILayout.Width(150f));

		return retVal != value;
	}

	public static string OpenFilePanelWithFilters(string inFilePath, string[] filters = null)
	{
		string path = EditorUtility.OpenFilePanelWithFilters("Open File", Application.dataPath, filters);

		if (string.IsNullOrEmpty(path) == false)
		{
			inFilePath = path;
			// 유효하지 않을떄 처리
			//EditorPrefs.SetString("", path);
		}

		return inFilePath;
	}

	public static string SaveFilePanel(string inFilePath, string extension = "dat")
	{
		string path = EditorUtility.SaveFilePanel("Save File", Application.dataPath, string.Empty, extension);

		if (string.IsNullOrEmpty(path) == false)
		{
			inFilePath = path;
			// 유효하지 않을떄 처리
			//EditorPrefs.SetString("", path);
		}

		return inFilePath;
	}

	#endregion

	public static List<IMapObjectBase> GetMapObjectsInScene()
	{
		GameObject rootGO = GameObject.Find(MapToolDefines.MapManagementObjectName);// GameObject.FindWithTag(UnityConstants.Tags.MapObjectRoot);

		List<IMapObjectBase> results = new List<IMapObjectBase>();

		var childs = rootGO.GetComponentsInChildren<IMapObjectBase>(true);
		if (null != childs)
		{
			results.AddRange(childs);
		}

		return results;
	}

	public static List<OBJECT_TYPE> GetMapObjectsInScene<OBJECT_TYPE>() where OBJECT_TYPE : class, IMapObjectBase
	{
		List<OBJECT_TYPE> results = new List<OBJECT_TYPE>();

		var allList = GetMapObjectsInScene();

		foreach (var mapObj in allList)
		{
			if (mapObj is OBJECT_TYPE)
			{
				results.Add(mapObj as OBJECT_TYPE);
			}
		}

		return results;
	}

	public static GameObject CreateGameObjectOfType(System.Type type)
	{
		string typeName = type.Name;
		GameObject go = GameObject.Find(typeName);

		if (go == null)
			go = new GameObject(typeName);

		return go;
	}

	/// <summary> SceneView상 가운데 위치 찾아준다. </summary>
	public static Vector3 GetSceneviewCenterPosition()
	{
		Camera sceneViewCamera = SceneView.lastActiveSceneView.camera;

		RaycastHit hit;

		Ray ray = sceneViewCamera.ScreenPointToRay(new Vector3(sceneViewCamera.pixelWidth / 2, sceneViewCamera.pixelHeight / 2));
		if (Physics.Raycast(ray, out hit, float.MaxValue, UnityConstants.Layers.EverythingBut()))
		{
			return hit.point;
		}

		//
		if (null != Terrain.activeTerrain)
		{

		}

		return Vector3.zero;
	}

	public static TYPE Popup<TYPE>(string label, TYPE current, List<TYPE> dataList)
	{
		int index = dataList.IndexOf(current);
		string[] displays = dataList.Select(x => x.ToString()).ToArray();

		EditorGUI.BeginChangeCheck();
		index = EditorGUILayout.Popup(label, index, displays);

		if (EditorGUI.EndChangeCheck())
		{
			current = dataList[index];
		}

		return current;
	}

	/// <summary> 에디터내 View다시 그리기 </summary>
	public static void RepaintSceneAndGameViews()
	{
		UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
	}

	/// <summary> 현재 씬맵 크기를 구해준다. </summary>
	public static Bounds GetCurrectMapBounds()
	{
		Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
		foreach (Renderer r in GameObject.FindObjectsOfType(typeof(Renderer)))
		{
			bounds.Encapsulate(r.bounds);
		}

		Terrain terrain = GameObject.FindObjectOfType<Terrain>();
		if (null != terrain && null != terrain.terrainData)
		{
			bounds.Encapsulate(terrain.terrainData.bounds);
		}

		//ZLog.Log(ZLogChannel.Map, ZLogLevel.Info, $"center : {bounds.center} , size : {bounds.size}, min : {bounds.min}, max : {bounds.max}, extents : {bounds.extents}");

		return bounds;
	}

	/// <summary>
	/// xlsx파일에 존재하는 시트를 로드해준다.
	/// </summary>
	public static ExcelWorksheet LoadExcelSheet(string filePath, string sheetName, out ExcelPackage exp)
	{
		FileInfo fileInfo = new FileInfo(filePath);
		if (!fileInfo.Exists)
		{
			throw new Exception($"존재하지 않는 파일 : {fileInfo.FullName}");
		}

		if (FileHelper.IsFileinUse(fileInfo))
		{
			throw new Exception($"{fileInfo.FullName} 이 수정가능한지 확인바람!!(열려있을 가능성 높음)");
		}

		exp = new ExcelPackage(fileInfo);
		if (null == exp)
		{
			return null;
		}

		ExcelWorksheet foundSheet = exp.Workbook.Worksheets[sheetName];
		if (exp.Workbook.Worksheets.Count <= 0 || null == foundSheet)
		{
			throw new Exception($"{filePath}에 Worksheet가 존재하지 않습니다.");
		}

		return foundSheet;
	}
}
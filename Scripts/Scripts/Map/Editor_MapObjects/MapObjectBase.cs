using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public interface IMapObjectBase
{
	uint GroupID { get; set; }
	string DisplayName { get; }
}

/// <summary>
/// 맵 배치용 기본 객체
/// </summary>
/// <remarks>
/// 에디터상 편리하게 Edit하기 위해서 MonoBehaviour기반으로 작성.
/// </remarks>
[SelectionBase()]
[DisallowMultipleComponent()]
public abstract class MapObjectBase<DATA_TYPE> : MonoBehaviour, IMapObjectBase
{
	public uint GroupID
	{
		get => mGroupID;
		set => mGroupID = value;
	}

	[Tooltip("0이라면 그룹없음")]
	[SerializeField]
	protected uint mGroupID;

	/// <summary>에디터상 표시될 이름</summary>
	public abstract string DisplayName { get; protected set; }

	protected virtual void Awake()
	{
		// 에디터 전용 객체로 설정
		this.gameObject.tag = UnityConstants.Tags.EditorOnly;
		this.gameObject.hideFlags |= HideFlags.DontSave;
	}

	public virtual void OnValidate()
	{
	}

	public abstract DATA_TYPE Export();
	public abstract void Import(DATA_TYPE _data);

	//=============================================================
#if UNITY_EDITOR
	protected virtual void OnDrawGizmos()
	{
		//float zoom = Vector3.SqrMagnitude(SceneView.currentDrawingSceneView.camera.transform.position - transform.position);

		//var style = new GUIStyle();
		//int fontSize = 1600;
		//style.fontSize = Mathf.Max(5, Mathf.FloorToInt(fontSize / zoom));

		//Handles.Label(transform.position, this.DisplayName, style);
	}
#endif
}

[DisallowMultipleComponent()]
public abstract class MapObjectBase_NonGeneric : MapObjectBase<object>
{
}

public abstract class MapObject_MovableBase : MapObjectBase_NonGeneric
{
	[HideInInspector]
	public MovementCollectionData MovingData;
}

#if UNITY_EDITOR
[CanEditMultipleObjects()]
public class MapObjectBaseEditor<MAP_OBJECT_TYPE> : Editor where MAP_OBJECT_TYPE : MonoBehaviour, IMapObjectBase
{
	protected MAP_OBJECT_TYPE mFirstCachedObject;
	protected MAP_OBJECT_TYPE mObject;

	/// <summary> SceneView상 객체 선택시 표시될 Label의 색상 </summary>
	public virtual Color DisplayLabelColor => Color.white;

	protected virtual void OnEnable()
	{
		mFirstCachedObject = mObject = target as MAP_OBJECT_TYPE;
	}

	protected virtual void OnDisable()
	{
	}

	protected virtual void OnSceneGUI()
	{
		mObject = target as MAP_OBJECT_TYPE;
		if (null == mObject)
			return;

		DrawGroupConnectedLine();

		//Handles.Label(mObject.transform.position, mObject.DisplayName);
		DrawText(mObject.DisplayName, mObject.transform.position, DisplayLabelColor, 14);
	}

	protected void DrawGroupConnectedLine()
	{
		// 그룹표시용.
		if (mFirstCachedObject == mObject && mObject.GroupID != 0)
		{
			var monObjs = new List<MapObjectBase_NonGeneric>();

			var curScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
			foreach (var rootGO in curScene.GetRootGameObjects())
			{
				var mons = rootGO.GetComponentsInChildren<MapObjectBase_NonGeneric>();
				foreach (var monGO in mons)
				{
					if (mObject.GroupID != monGO.GroupID)
						continue;

					monObjs.Add(monGO);
				}
			}

			Handles.color = new Color(1, 0, 0, 0.5f);
			foreach (var mon in monObjs)
			{
				Handles.DrawLine(mObject.transform.position, mon.transform.position);
			}
		}
	}

	public static void DrawText(string text, Vector3 position, Color? color = null, int fontSize = 0, float yOffset = 0, FontStyle fontStyle = FontStyle.Bold)
	{
		GUIContent textContent = new GUIContent(text);

		GUIStyle style = new GUIStyle();
		if (color != null)
			style.normal.textColor = (Color)color;
		if (fontSize > 0)
			style.fontSize = fontSize;
		style.fontStyle = fontStyle;
		style.alignment = TextAnchor.MiddleCenter;

		Vector2 textSize = style.CalcSize(textContent);
		Vector3 screenPoint = Camera.current.WorldToScreenPoint(position);

		if (screenPoint.z > 0) // checks necessary to the text is not visible when the camera is pointed in the opposite direction relative to the object
		{
			var worldPosition = Camera.current.ScreenToWorldPoint(new Vector3(screenPoint.x - textSize.x * 0.5f, screenPoint.y + textSize.y * 0.5f + yOffset, screenPoint.z));
			UnityEditor.Handles.Label(worldPosition, textContent, style);
		}
	}
}

#endif
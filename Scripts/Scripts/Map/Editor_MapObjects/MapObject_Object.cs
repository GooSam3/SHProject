using UnityEngine;
using GameDB;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapObject_Object : MapObject_MovableBase
{
	public class EditorSummary
	{
		public static string Name = "Object";
		public static string Desc = "Object 배치를 위한 에디터 객체";
	}

	public override string DisplayName { get; protected set; }

	[ReadOnly()]
	public E_UnitType EntityType = E_UnitType.Object;
	/// <summary> <see cref="UnitType"/>에 해당하는 테이블 TID </summary>
	public uint TableTID;

	public override object Export()
	{
		MapData.ObjectInfo newSpawnInfo = new MapData.ObjectInfo()
		{
			GroupID = this.GroupID,
			Position = this.transform.position,
			Rotation = this.transform.rotation,
			TableTID = this.TableTID,
			MovingData = this.MovingData?.Clone(),
		};

		return newSpawnInfo;
	}

	public override void Import(object _data)
	{
		MapData.ObjectInfo info = _data as MapData.ObjectInfo;
		this.GroupID = info.GroupID;
		this.transform.position = info.Position;
		this.transform.rotation = info.Rotation;
		this.TableTID = info.TableTID;
		this.MovingData = info.MovingData?.Clone();

		OnValidate();
	}

#if UNITY_EDITOR

	private uint mPrevTableTID;
	private GameObject mCreatedModel;
	[SerializeField, ReadOnly]
	private string mCreatedModelName;
	public System.Action<uint> TidChanged;

	[SerializeField, ReadOnly]
	private float Scale;

	public void SetModel(GameObject newModel)
	{
		ClearModel();

		if (null == newModel)
			return;

		newModel.transform.SetParent(this.transform, false);
		foreach (var child in newModel.transform.GetComponentsInChildren<Transform>())
		{
			child.gameObject.hideFlags |= HideFlags.NotEditable | HideFlags.HideInHierarchy;
		}

		mCreatedModel = newModel;
		mCreatedModelName = newModel.name;

		Scale = newModel.transform.localScale.x * 100f;
	}

	public void ClearModel()
	{
		if (null != mCreatedModel)
		{
			DestroyImmediate(mCreatedModel);
			mCreatedModel = null;
		}
	}

	public override void OnValidate()
	{
		base.OnValidate();

		if (GroupID != 0)
			DisplayName = $"<color=orange>Group_{GroupID}</color>\n";
		else
			DisplayName = string.Empty;

		DisplayName += $"Object_[{TableTID}]";

		if (mPrevTableTID != TableTID)
		{
			mPrevTableTID = TableTID;
			TidChanged?.Invoke(mPrevTableTID);
		}
	}
#endif
}


#if UNITY_EDITOR
[CustomEditor(typeof(MapObject_Object), true)]
[CanEditMultipleObjects()]
public class MapObject_ObjectEditor : MapObjectBaseEditor<MapObject_Object>
{
}
#endif
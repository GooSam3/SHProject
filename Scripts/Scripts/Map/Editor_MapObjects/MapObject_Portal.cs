using GameDB;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapObject_Portal : MapObjectBase_NonGeneric
{
	public class EditorSummary
	{
		public static string Name = "포탈";
		public static string Desc = "포탈 배치를 위한 에디터 객체";
	}

	public override string DisplayName { get; protected set; }

	[ReadOnly()]
	public MapData.PortalInfo.PurposeType Purpose = MapData.PortalInfo.PurposeType.Portal;
	/// <summary> <see cref="Portal_Table"/>에 해당하는 테이블 TID </summary>
	public uint TableTID;
	/// <summary> 포탈위치 랜덤 범위 </summary>
	public float Radius;

	public override void OnValidate()
	{
		base.OnValidate();

		DisplayName = $"Portal_[{TableTID}]";
	}

	public override object Export()
	{
		MapData.PortalInfo newSpawnInfo = new MapData.PortalInfo()
		{
			GroupID = this.GroupID,
			Position = this.transform.position,
			Purpose = this.Purpose,
			TableTID = this.TableTID,
			StartInRadius = this.Radius,
		};

		return newSpawnInfo;
	}

	public override void Import(object _data)
	{
		MapData.PortalInfo info = _data as MapData.PortalInfo;
		this.GroupID = info.GroupID;
		this.transform.position = info.Position;
		this.Purpose = info.Purpose;
		this.TableTID = info.TableTID;
		this.Radius = info.StartInRadius;

		OnValidate();
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(MapObject_Portal), true)]
[CanEditMultipleObjects()]
public class MapObject_PortalPointEditor : MapObjectBaseEditor<MapObject_Portal>
{
}
#endif
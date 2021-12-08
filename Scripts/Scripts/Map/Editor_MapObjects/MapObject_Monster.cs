using GameDB;
using UnityEngine;

public class MapObject_Monster : MapObject_MovableBase
{
	public class EditorSummary
	{
		public static string Name = "몬스터";
		public static string Desc = "몬스터 스폰 정보를 위한 객체";
	}

	public override string DisplayName { get; protected set; } = string.Empty;

	[ReadOnly()]
	public E_UnitType EntityType = E_UnitType.Monster;
	/// <summary> <see cref="UnitType"/>에 해당하는 테이블 TID </summary>
	[Tooltip("몬스터 테이블에 존재하는 TID가 설정되어야함.")]
	public uint TableTID;

	[Tooltip("몬스터 스폰 방식 선택"), ReadOnly()]
	public E_SpawnType SpawnType = E_SpawnType.None;

	/// <summary> 
	/// <see cref="E_SpawnType.Die"/>일때만 유효함 (1.0 = 1초)
	/// ** 0 이라면 테이블 기본값으로 적용하도록 하자.
	/// </summary>
	[Tooltip("E_SpawnType.Die일때 사용되는 값 (서버에서 사용하는 체크필요)")]
	public float RespawnTime;

	public override object Export()
	{
		MapData.MonsterInfo newSpawnInfo = new MapData.MonsterInfo()
		{
			GroupID = this.GroupID,
			Position = this.transform.position,
			Rotation = this.transform.rotation,
			TableTID = this.TableTID,
			RespawnTime = this.RespawnTime,
			MovingData = this.MovingData?.Clone(),
		};

		return newSpawnInfo;
	}

	public override void Import(object _data)
	{
		MapData.MonsterInfo monInfo = _data as MapData.MonsterInfo;
		this.GroupID = monInfo.GroupID;
		this.transform.position = monInfo.Position;
		this.transform.rotation = monInfo.Rotation;
		this.TableTID = monInfo.TableTID;
		this.RespawnTime = monInfo.RespawnTime;
		this.MovingData = monInfo.MovingData?.Clone();

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

		Scale = newModel.transform.localScale.x * 100;
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

		DisplayName += $"Monster_[{TableTID}]";

		if (mPrevTableTID != TableTID)
		{
			mPrevTableTID = TableTID;
			TidChanged?.Invoke(mPrevTableTID);
		}
	}

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();

		// 모델보이기.
	}
#endif
}
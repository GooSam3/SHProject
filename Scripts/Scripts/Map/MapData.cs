using System.Collections.Generic;
using UnityEngine;
using GameDB;
using System;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(MapData))]
public class MapDataEditor : Editor
{
    MapData dataObject;

    public void OnEnable()
    {
        dataObject = (MapData)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("데이터 검사"))
        {
            bool isChanged = false;
            foreach (var info in dataObject.SpawnInfos)
            {
                if (info.UnitType != E_UnitType.None)
                    continue;

                Debug.Log($"{nameof(MapData)}의 {nameof(MapData.SpawnInfos)}[{info.TableTID}]에서 UnitType을 E_UnitType.None -> Monster 로 수정함.");

                // 기본값 강제로 설정
                info.UnitType = E_UnitType.Monster;
                isChanged |= true;
            }

            if (isChanged)
            {
                EditorUtility.SetDirty(dataObject);
                AssetDatabase.SaveAssets();
            }
        }
    }

}
#endif


/// <summary>
/// Runtime모드에서 사용될 맵별 Spawn,Portal 데이터 모음
/// </summary>
public class MapData : ScriptableObject
{
    // SpawnPoint
    [Serializable]
    public class InfoBase
    {
		/// <summary> 그룹 구분용ID </summary>
		public uint GroupID;

        public float StartInRadius = 1f;
        public Vector3 Position;
        public Quaternion Rotation = Quaternion.identity;

        /// <summary> <see cref="StartInRadius"/>영역내 랜덤 포지션 리턴</summary>
        public Vector3 GetRandomPosInRadius()
        {
            return VectorHelper.RandomCircleXZ(Position, StartInRadius);
        }
    }

    [Serializable]
    public class SpawnInfo : InfoBase
    {
        /// <summary> Monster, NPC만 사용바람. </summary>
        public E_UnitType UnitType = E_UnitType.Monster;
        /// <summary> <see cref="UnitType"/>에 해당하는 테이블 TID </summary>
        public uint TableTID;
        /// <summary> 재스폰시간 에디터에서 셋팅한 값 </summary>
        public float RespawnTime;
    }

	[Serializable]
	public class MonsterInfo : InfoBase
	{
		/// <summary> <see cref="UnitType"/>에 해당하는 테이블 TID </summary>
		public uint TableTID;
		/// <summary> 재스폰시간 에디터에서 셋팅한 값 </summary>
		public float RespawnTime;
		/// <summary> </summary>
		public MovementCollectionData MovingData;
	}

	[Serializable]
	public class NpcInfo : InfoBase
	{
		/// <summary> <see cref="UnitType"/>에 해당하는 테이블 TID </summary>
		public uint TableTID;
		/// <summary> </summary>
		public MovementCollectionData MovingData;
	}

    [Serializable]
    public class ObjectInfo : InfoBase
    {
        /// <summary> <see cref="UnitType"/>에 해당하는 테이블 TID </summary>
        public uint TableTID;
        /// <summary> </summary>
        public MovementCollectionData MovingData;
    }

    [Serializable]
    public class PortalInfo : InfoBase
    {
        public enum PurposeType
        {
            Portal = 0,
            AttackBoss, // 필드보스소환시 플레이어의 시작 위치
            Other, // 퀘스트 바로가기
        }

        /// <summary> <see cref="Portal_Table"/>에 해당하는 테이블 TID </summary>
        public uint TableTID;
        public PurposeType Purpose;
    }

    [Serializable]
    public class BotSpawnInfo : InfoBase
    {
        /// <summary>현재 범위에서 스폰될 봇 최대수</summary>
        public int SpawnCountLimit;
    }

    /// <summary> 파일이름에 스테이지TID포함되어 있긴함.</summary>
    public uint StageTID;

	[Header("Monster 전용")]
	public List<MonsterInfo> MonsterInfos;

	[Header("Npc 전용")]
	public List<NpcInfo> NpcInfos;

    [Header("Object 전용")]
    public List<ObjectInfo> ObjectInfos;

    [Header("SpawnPoint 전용")]
    public List<SpawnInfo> SpawnInfos;

    [Header("PotalPoint 전용")]
    public List<PortalInfo> PortalInfos;

    [Header("")]
    public List<BotSpawnInfo> BotSpawnInfos;

    [Header("")]
    public Bounds MapBounds;

    public void ClearAll()
    {
        StageTID = 0;

		NpcInfos = new List<NpcInfo>();
		MonsterInfos = new List<MonsterInfo>();
        ObjectInfos = new List<ObjectInfo>();

        SpawnInfos = new List<SpawnInfo>();
		PortalInfos = new List<PortalInfo>();
        BotSpawnInfos = new List<BotSpawnInfo>();
    }
}

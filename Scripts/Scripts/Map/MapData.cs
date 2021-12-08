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

        if (GUILayout.Button("������ �˻�"))
        {
            bool isChanged = false;
            foreach (var info in dataObject.SpawnInfos)
            {
                if (info.UnitType != E_UnitType.None)
                    continue;

                Debug.Log($"{nameof(MapData)}�� {nameof(MapData.SpawnInfos)}[{info.TableTID}]���� UnitType�� E_UnitType.None -> Monster �� ������.");

                // �⺻�� ������ ����
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
/// Runtime��忡�� ���� �ʺ� Spawn,Portal ������ ����
/// </summary>
public class MapData : ScriptableObject
{
    // SpawnPoint
    [Serializable]
    public class InfoBase
    {
		/// <summary> �׷� ���п�ID </summary>
		public uint GroupID;

        public float StartInRadius = 1f;
        public Vector3 Position;
        public Quaternion Rotation = Quaternion.identity;

        /// <summary> <see cref="StartInRadius"/>������ ���� ������ ����</summary>
        public Vector3 GetRandomPosInRadius()
        {
            return VectorHelper.RandomCircleXZ(Position, StartInRadius);
        }
    }

    [Serializable]
    public class SpawnInfo : InfoBase
    {
        /// <summary> Monster, NPC�� ���ٶ�. </summary>
        public E_UnitType UnitType = E_UnitType.Monster;
        /// <summary> <see cref="UnitType"/>�� �ش��ϴ� ���̺� TID </summary>
        public uint TableTID;
        /// <summary> �罺���ð� �����Ϳ��� ������ �� </summary>
        public float RespawnTime;
    }

	[Serializable]
	public class MonsterInfo : InfoBase
	{
		/// <summary> <see cref="UnitType"/>�� �ش��ϴ� ���̺� TID </summary>
		public uint TableTID;
		/// <summary> �罺���ð� �����Ϳ��� ������ �� </summary>
		public float RespawnTime;
		/// <summary> </summary>
		public MovementCollectionData MovingData;
	}

	[Serializable]
	public class NpcInfo : InfoBase
	{
		/// <summary> <see cref="UnitType"/>�� �ش��ϴ� ���̺� TID </summary>
		public uint TableTID;
		/// <summary> </summary>
		public MovementCollectionData MovingData;
	}

    [Serializable]
    public class ObjectInfo : InfoBase
    {
        /// <summary> <see cref="UnitType"/>�� �ش��ϴ� ���̺� TID </summary>
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
            AttackBoss, // �ʵ庸����ȯ�� �÷��̾��� ���� ��ġ
            Other, // ����Ʈ �ٷΰ���
        }

        /// <summary> <see cref="Portal_Table"/>�� �ش��ϴ� ���̺� TID </summary>
        public uint TableTID;
        public PurposeType Purpose;
    }

    [Serializable]
    public class BotSpawnInfo : InfoBase
    {
        /// <summary>���� �������� ������ �� �ִ��</summary>
        public int SpawnCountLimit;
    }

    /// <summary> �����̸��� ��������TID���ԵǾ� �ֱ���.</summary>
    public uint StageTID;

	[Header("Monster ����")]
	public List<MonsterInfo> MonsterInfos;

	[Header("Npc ����")]
	public List<NpcInfo> NpcInfos;

    [Header("Object ����")]
    public List<ObjectInfo> ObjectInfos;

    [Header("SpawnPoint ����")]
    public List<SpawnInfo> SpawnInfos;

    [Header("PotalPoint ����")]
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

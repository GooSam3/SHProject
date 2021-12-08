using GameDB;
using System.Collections.Generic;
using UnityEngine;

public class MapDataManagement : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    public MapData MainData;

    /// <summary>�� ũ���</summary>
    [SerializeField]
    public Bounds MapBounds;
    [SerializeField]
    public float MapRadius = 150f;

    public List<MapData.PortalInfo> PortalPoints { get { return mPortalPoints; } }
    public List<MapData.SpawnInfo> SpawnPoints { get { return mSpawnPoints; } }
    public List<MapData.BotSpawnInfo> BotSpawnPoints { get { return mBotSpawnPoints; } }

    private GameObject mMapRootGO = null;

    private List<MapData.PortalInfo> mPortalPoints = new List<MapData.PortalInfo>();
    private List<MapData.SpawnInfo> mSpawnPoints = new List<MapData.SpawnInfo>();
    private List<MapData.BotSpawnInfo> mBotSpawnPoints = new List<MapData.BotSpawnInfo>();

    private Dictionary<MapData.PortalInfo.PurposeType, List<MapData.PortalInfo>> mPortalDic = new Dictionary<MapData.PortalInfo.PurposeType, List<MapData.PortalInfo>>();

    private void Awake()
    {
        mMapRootGO = this.gameObject;
    }

    /// <summary>
    /// �־��� ���������� �ش��ϴ� �� ���� ���������� �Ѵ�.
    /// </summary>
    public void Initialize(uint _stageTid)
    {
        /*
		 * MainData GameInfo�� StageTID�� �´� ���Ϸ� �о�;���!!
		 * 
		 * ���� ���ε�, StageTID�� �ٸ� �� ����.
		 */
#if UNITY_EDITOR && !USE_ASSETBUNDLE
        MainData = (MapData)UnityEditor.AssetDatabase.LoadAssetAtPath($"Assets/BundlePrefab/Chaos/MapData/map_{ _stageTid.ToString()}.asset", typeof(Object));

        if (MainData == null)
            MainData = (MapData)UnityEditor.AssetDatabase.LoadAssetAtPath($"Assets/BundlePrefab/Mini/MapData/map_{ _stageTid.ToString()}.asset", typeof(Object));
#else
	//	MainData = (MapData)AssetBundleLoader.instance.LoadMapData($"chaos/mapdata/map_{_stageTid.ToString()}.asset");
#endif
        //MainData = (MapData)ResourceManager.instance.Load(System.IO.Path.Combine("Datas/Map", $"Map_{_stageTid.ToString()}"));
        if (null == MainData)
        {
            Debug.LogError($"StageTID[{_stageTid}] �� �ش��ϴ� MapData�� �������� �ʽ��ϴ�.");
            return;
        }

        mPortalPoints.Clear();
        mSpawnPoints.Clear();
        mBotSpawnPoints.Clear();

        mPortalPoints.AddRange(MainData.PortalInfos);
        mSpawnPoints.AddRange(MainData.SpawnInfos);
        mBotSpawnPoints.AddRange(MainData.BotSpawnInfos);

        // ������ �з�
        foreach (var portal in mPortalPoints)
        {
            if (!mPortalDic.ContainsKey(portal.Purpose))
            {
                mPortalDic.Add(portal.Purpose, new List<MapData.PortalInfo>());
            }
            mPortalDic[portal.Purpose].Add(portal);
        }
    }

    /// <summary> �ʵ庸�� ��ȯ��, �÷��̾� �̵��� ��ġ ���� ã�� </summary>
    /// <param name="warpPoint"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    public bool TryGetPointForAttackBoss(out Vector3 warpPoint, out Quaternion rotation)
    {
        var foundInfo = PortalPoints.Find((info) => info.Purpose == MapData.PortalInfo.PurposeType.AttackBoss);
        if (null != foundInfo)
        {
            warpPoint = foundInfo.Position;
            rotation = foundInfo.Rotation;
            return true;
        }

        warpPoint = Vector3.zero;
        rotation = Quaternion.identity;

        return false;
    }

    public List<MapData.PortalInfo> GetPortals(MapData.PortalInfo.PurposeType purposeType)
    {
        return mPortalDic[purposeType];
    }

    /// <summary>����Ʈ�� �����ϰ� ã�Ƽ� ����</summary>
    public bool GetRandomPortal(MapData.PortalInfo.PurposeType purposeType, out MapData.PortalInfo portalInfo)
    {
        var portalList = mPortalDic[purposeType];
        portalInfo = portalList[Random.Range(0, portalList.Count)];

        return null != portalInfo && portalInfo.TableTID != 0;
    }

    /// <summary>�־��� ���� Ÿ�Կ� �ش��ϴ� ��ü�� ����</summary>
    public List<MapData.SpawnInfo> GetSpawnPoints(E_UnitType _unitType)
    {
        return mSpawnPoints.FindAll(point => point.UnitType == _unitType);
    }

    /// <summary>�ش� ���� ������ �������� �׸��� ��û</summary>
    public bool TryGetSpawnInfo(uint monsterTid, out MapData.SpawnInfo spawnInfo)
    {
        spawnInfo = mSpawnPoints.Find(info => info.TableTID == monsterTid);

        return null != spawnInfo;
    }

    /// <summary>
    /// [wisreal][2020.03.09]
    /// </summary>
    public class ReplaceMonsterSpawn
    {
        public uint SpawnKey;
        public uint SpawnMonsterTid;
        public uint SpawnCnt;

        public MapData.SpawnInfo spawnInfo;
    }

    /// <summary>�ش� ������ ���� ������ ���ڴ�. ���� ���̵� �ٲ�ġ���Ѵ�. �������� �̹��� �Ⱦ��ٴ� �ǹ�</summary>
    public void ChangeUseSpawnInfo(Dictionary<uint, ReplaceMonsterSpawn> usingMonsterList)
    {
        for (int i = 0; i < mSpawnPoints.Count; i++)
        {
            if (mSpawnPoints[i].UnitType == E_UnitType.Monster && !usingMonsterList.ContainsKey(mSpawnPoints[i].TableTID))
            {
                mSpawnPoints.RemoveAt(i);
                i--;
            }
            else if (mSpawnPoints[i].UnitType == E_UnitType.Monster && usingMonsterList.ContainsKey(mSpawnPoints[i].TableTID))
            {
                var newInfo = new MapData.SpawnInfo()
                {
                    Position = mSpawnPoints[i].Position,
                    Rotation = mSpawnPoints[i].Rotation,
                    RespawnTime = mSpawnPoints[i].RespawnTime,
                    StartInRadius = mSpawnPoints[i].StartInRadius,
                    UnitType = mSpawnPoints[i].UnitType,
                    TableTID = usingMonsterList[mSpawnPoints[i].TableTID].SpawnMonsterTid
                };

                usingMonsterList[mSpawnPoints[i].TableTID].spawnInfo = newInfo;

                mSpawnPoints.RemoveAt(i);
                mSpawnPoints.Insert(i, newInfo);
            }
        }
    }

    int BotSpawnUseCount = 0;
    int BotSpawnPointsIdx = 0;
    /// <summary>
    /// �� ȣ���, �� ���� ������ ��ġ ������ ��ȿ�Ҷ����� �������ش�.
    /// </summary>
    public MapData.BotSpawnInfo PopBotSpawnInfo()
    {
        if (BotSpawnPoints.Count == 0 || BotSpawnPointsIdx >= BotSpawnPoints.Count)
            return RandomBotSpawnInfo();

        if (BotSpawnUseCount >= BotSpawnPoints[BotSpawnPointsIdx].SpawnCountLimit)
        {
            ++BotSpawnPointsIdx;
            BotSpawnUseCount = 0;

            if (BotSpawnPointsIdx >= BotSpawnPoints.Count)
                return RandomBotSpawnInfo();
        }

        if (BotSpawnUseCount < BotSpawnPoints[BotSpawnPointsIdx].SpawnCountLimit)
        {
            ++BotSpawnUseCount;

            return BotSpawnPoints[BotSpawnPointsIdx];
        }

        return RandomBotSpawnInfo();
    }

    public MapData.BotSpawnInfo RandomBotSpawnInfo()
    {
        return BotSpawnPoints.Count > 0 ? BotSpawnPoints[Random.Range(0, BotSpawnPoints.Count)] : null;
    }

    public void RestoreBotSpawnInfo()
    {
        BotSpawnUseCount = 0;
        BotSpawnPointsIdx = 0;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        ZGizmos.DrawCircleGizmo(transform.position, MapRadius, Color.cyan);
    }
#endif
}

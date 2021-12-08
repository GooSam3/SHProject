using System.Collections.Generic;
using GameDB;
using UnityEngine;

/// <summary> 게임 모드 관리 </summary>
public class ZGameModeManager : Zero.Singleton<ZGameModeManager>
{
    public class BattleInfoData
	{
        public ulong TotalExp;
        public ulong TotalGold;
        public ulong TotalMonsterKillCount;


	}

    public class SChannelData
	{
        public uint UserMax = 0;
        public uint UserCur = 0;
        public uint ChannelID = 0;
        public uint BounusExp = 0;
        public uint BounusGold = 0;
        public string ChannelAddress;
        public string OwnerGuildName;
        public uint OwnerGuildMarkTID;
        public bool IsPvPZone = false;
        public bool IsBossZone = false;
	}

    /// <summary> 현재 게임 모드  </summary>
	public FSM.FSM<E_GameModeEvent, E_GameModeType, ZGameModeManager> FSM { get; private set; }
    /// <summary> 현재 StageTid </summary>
    public uint StageTid { get; private set; }
    /// <summary> 다시하기등을 위해 임시저장 id 신뢰할수 없음 </summary>
    public uint LastStageTid { get; private set; }
    /// <summary> 현재 Stage Table </summary>
    private Stage_Table mTable = null;
    /// <summary> 현재 Stage Table </summary>
    public Stage_Table Table { get { return mTable; } }

    public Stack<Dictionary<uint, BattleInfoData>> BattleInfoDataStack = new Stack<Dictionary<uint, BattleInfoData>>();

    private Dictionary<uint, BattleInfoData> BattleInfoDataDic = new Dictionary<uint, BattleInfoData>();
    
    public E_PKAreaChangeType PKType = E_PKAreaChangeType.None;

    /// <summary> 현재 게임 모드 타입 </summary>
    public E_GameModeType CurrentGameModeType { get { return currentGameMode.GameModeType; } }
    //--------------------------------------------------
    public ushort ChannelId { get; private set; }
    private bool mSendChannelPacket = false;
    private List<SChannelData> m_listChannelData = new List<SChannelData>();
    //--------------------------------------------------
    public MapData CurrentMapData;
    
    private System.Action mEventCompleteMapdataLoad;
    private System.Action<E_GameModeType> reserveActionSceneLoadComplete;

    public System.Action mEventBossSpawn;
    public System.Action mEventGuildDungeonStateChange;

    /// <summary> 현재 게임 모드 </summary>
    private ZGameModeBase currentGameMode { get { return FSM.Current as ZGameModeBase; } }
    public ZGameModeBase GameMode { get { return currentGameMode; } }

    public T CurrentGameMode<T>() where T : ZGameModeBase
    {
        if( FSM.Current is T ) {
            return ( T )FSM.Current;
        }
        else {
            return null;
        }
    }

    protected override void Init()
    {
        base.Init();
        FSM = new FSM.FSM<E_GameModeEvent, E_GameModeType, ZGameModeManager>(this);

        FSM.AddState(E_GameModeType.Empty, gameObject.GetOrAddComponent<ZGameModeEmpty>());        
        FSM.AddState(E_GameModeType.Field, gameObject.GetOrAddComponent<ZGameModeField>());        
        FSM.AddState(E_GameModeType.Temple, gameObject.GetOrAddComponent<ZGameModeTemple>());
        FSM.AddState(E_GameModeType.Colosseum, gameObject.GetOrAddComponent<ZGameModeColosseum>());
        FSM.AddState(E_GameModeType.TrialSanctuary, gameObject.GetOrAddComponent<ZGameModeTrialSanctuary>());
        FSM.AddState(E_GameModeType.GodLand, gameObject.GetOrAddComponent<ZGameModeGodLand>());
        FSM.AddState(E_GameModeType.Infinity, gameObject.GetOrAddComponent<ZGameModeInfinity>());
        FSM.AddState(E_GameModeType.Tower, gameObject.GetOrAddComponent<ZGameModeTower>());
        FSM.AddState(E_GameModeType.BossWar, gameObject.GetOrAddComponent<ZGameModeBossWar>());
        FSM.AddState(E_GameModeType.GuildDungeon, gameObject.GetOrAddComponent<ZGameModeGuildDungeon>());
        FSM.Enable(E_GameModeType.Empty);
        
        
    }

	protected override void OnDestroy()
	{
        base.OnDestroy();
	}

	public void SetStage(uint stageTid, ushort channdelId, long roomNo = 0)
    {
        LastStageTid = StageTid;
        StageTid = stageTid;
        ChannelId = channdelId;
        if(true ==  ZWebManager.hasInstance && true == ZWebManager.Instance.WebGame.IsUsable)
            ZNet.Data.Me.CurCharData.LastChannelId = channdelId;

        if (DBStage.TryGet(StageTid, out mTable))
        {
            if (mTable.StageType == E_StageType.Field || mTable.StageType == E_StageType.Tower)
            {
                if (BattleInfoDataDic.TryGetValue(stageTid, out BattleInfoData data))
                {
                    BattleInfoDataDic.Remove(stageTid);
                }

                BattleInfoDataDic.Add(stageTid, new BattleInfoData());
                BattleInfoDataStack.Push(BattleInfoDataDic);
            }
        }
        
        foreach(var table in BattleInfoDataStack)
		{
            foreach(var data in table)
			{
                ZLog.Log(ZLogChannel.UI, $"## StageTid {data.Key} : {data.Value.TotalExp} : {data.Value.TotalGold} : {data.Value.TotalMonsterKillCount}");
			}
		}

        UpdateMode();

        //[wisreal][2020.11.20] - RefreshSubHud 호출로 hud 갱신시 중복 호출 되므로 삭제
        /*if (stageTid != 0)
		{
            UIManager.Instance?.Find<UISubHUDMiniMap>()?.DoMinimapRefreshChannel();
        }*/
    }

    public void AddBattleInfoEvent()
	{
        RemoveBattleInfoEvent();

        ZNet.Data.Me.CurCharData.ExpUpdated += ZGameModeManager.Instance.UpdateBattleInfo;
        DropItemSpawner.DropGoldUpdate += ZGameModeManager.Instance.UpdateBattleGoldInfo;
    }

    public void RemoveBattleInfoEvent()
	{
        ZNet.Data.Me.CurCharData.ExpUpdated -= ZGameModeManager.Instance.UpdateBattleInfo;
        DropItemSpawner.DropGoldUpdate -= ZGameModeManager.Instance.UpdateBattleGoldInfo;
    }

    private void UpdateBattleGoldInfo(ulong amount)
	{
        if (BattleInfoDataDic.TryGetValue(StageTid, out BattleInfoData data))
        {
            data.TotalGold += amount;
        }
	}

    private void UpdateBattleInfo(ulong preExp, ulong newExp, bool isMonsterKill)
	{
        if (!isMonsterKill)
            return;

        if(BattleInfoDataDic.TryGetValue(StageTid, out BattleInfoData data))
		{
            ulong gainExp = newExp - preExp;
            
            data.TotalExp += gainExp;
            data.TotalMonsterKillCount++;
        }
	}

    #region 채널 관련 //////////////////////////////////////
    /// <summary>  현재 스테이지의 채널 상태는 수시로 갱신되므로 필요할때 마다 호출 할것  </summary>
    public void RefreshChannelList(System.Action<List<SChannelData>> _recvChannelList)
	{
        if (mSendChannelPacket) return;

        mSendChannelPacket = true;

        ZWebManager.Instance.WebGame.REQ_MMOChannelList(StageTid, (ChannelList) =>
        {
            m_listChannelData.Clear();
            ushort maxChannelUser = ChannelList.MaxJoinUser; 

            for (int i = 0; i < ChannelList.ServerListLength; i++)
			{
                SChannelData channelData = new SChannelData();
                m_listChannelData.Add(channelData);

                WebNet.MmoChannel mmoChannel = ChannelList.ServerList(i).Value;
                channelData.ChannelAddress = mmoChannel.ActualServerAddr;
                channelData.BounusExp = mmoChannel.BonusExpRate;
                channelData.BounusGold = mmoChannel.BonusGoldRate;
                channelData.ChannelID = mmoChannel.ChannelId;
                channelData.UserCur = mmoChannel.NumUser;
                channelData.UserMax = maxChannelUser;
                channelData.IsPvPZone = mmoChannel.IsPkZone;
                channelData.IsBossZone = mmoChannel.IsFieldBossSpawnZone;
                channelData.OwnerGuildName = mmoChannel.FieldBossKillerGuildName;
                channelData.OwnerGuildMarkTID = mmoChannel.FieldBossKillerGuildMarkTid;
			}
            mSendChannelPacket = false;
            _recvChannelList?.Invoke(m_listChannelData);
        }, null);
    }

    public void RefreshChannelList(System.Action _recvChannelList)
	{
        RefreshChannelList((tempList) => { _recvChannelList?.Invoke(); });
    }

    public bool IsChaosChannel(bool _bossZone = true)
	{
        bool chaosChannel = false;
        for (int i = 0; i < m_listChannelData.Count; i++)
		{
            SChannelData channelData = m_listChannelData[i];
            if (channelData.ChannelID == ChannelId)
			{
                if (_bossZone)
                {
                    if (channelData.IsPvPZone == true && channelData.IsBossZone == true)
                    {
                        chaosChannel = true;
                    }
                }
                else if (channelData.IsPvPZone == true && channelData.IsBossZone == false)
				{
                    chaosChannel = true;
				}

                break;
			}
		}
        return chaosChannel;
	}
    public bool IsPvPChannel()
	{
        if(PKType == E_PKAreaChangeType.NonPKArea)
		{
            return false;
		}
        
        for (int i = 0; i < m_listChannelData.Count; i++)
        {
            SChannelData channelData = m_listChannelData[i];
            if (channelData.ChannelID == ChannelId)
            {
                return channelData.IsPvPZone;
            }
        }
        return false;
    }

    public SChannelData GetChannelData(uint _channelID)
	{
        SChannelData findData = null;
        for (int i = 0; i < m_listChannelData.Count; i++)
        {
            if (m_listChannelData[i].ChannelID == _channelID)
            {
                findData = m_listChannelData[i];
                break;
            }
        }
        return findData;
	}

    public SChannelData GetChaosChannel()
	{
        SChannelData findData = null;
        for (int i = 0; i < m_listChannelData.Count; i++)
        {
            if (m_listChannelData[i].IsBossZone == true && m_listChannelData[i].IsPvPZone)
            {
                findData = m_listChannelData[i];
                break;
            }
        }
        return findData;
    }

	#endregion
	//----------------------------------------------------------------------

	private void UpdateMode()
    {
        if(0 >= StageTid)
        {            
            FSM.ChangeState(E_GameModeType.Empty);
        }
        else
        {
            switch(Table.StageType)
            {
                case E_StageType.Temple: UpdateMode(E_GameModeType.Temple, false); break;
                case E_StageType.Colosseum: UpdateMode( E_GameModeType.Colosseum, false); break;
                case E_StageType.GodLand: UpdateMode( E_GameModeType.GodLand, false); break;
                case E_StageType.Instance: UpdateMode(E_GameModeType.TrialSanctuary, true); break;
                case E_StageType.Infinity: UpdateMode(E_GameModeType.Infinity, true); break;
                case E_StageType.Tower: UpdateMode( E_GameModeType.Tower, false); break;
                case E_StageType.Town: UpdateMode(E_GameModeType.Field, true); break;
                case E_StageType.InterServer: UpdateMode(E_GameModeType.BossWar, true); break;
                case E_StageType.GuildDungeon: UpdateMode(E_GameModeType.GuildDungeon, true); break;
                default: UpdateMode(E_GameModeType.Field, false); break;
            }
        }
    }

    private void UpdateMode(E_GameModeType mode, bool bLoadMapData)
    {
        //같은 게임 모드
        //if (CurrentGameMode().GameModeType == mode)
        //    return;

        FSM.ChangeState(mode, true);

        if (CurrentMapData != null)
        {
            Resources.UnloadAsset(CurrentMapData);
        }

        if (true == bLoadMapData)
        {
            ZResourceManager.Instance.Load<MapData>($"Map_{StageTid}", (string _loadedName, MapData _mapData) =>
            {
                CurrentMapData = _mapData;

                ZLog.Log(ZLogChannel.UI, "## MapData Load Complete !!!");
                currentGameMode.MapDataLoadComplete();
                mEventCompleteMapdataLoad?.Invoke();
            });
        }
    }

    #region ===== :: 현재 스테이지의 맵 정보 관련 :: =====
    /// <summary> 맵정보에서 npc 정보를 얻어온다. </summary>
    public MapData.NpcInfo GetNpcInfo(uint tid)
	{
        if (null == CurrentMapData)
            return null;

        var info = CurrentMapData.NpcInfos.Find((item) => item.TableTID == tid);

        return info;
    }

    /// <summary> 맵정보에서 monster 정보를 얻어온다. </summary>
    public MapData.MonsterInfo GetMonsterInfo(uint tid)
    {
        if (null == CurrentMapData)
            return null;

        var info = CurrentMapData.MonsterInfos.Find((item) => item.TableTID == tid);

        return info;
    }

    /// <summary> 맵정보에서 object 정보를 얻어온다. </summary>
    public MapData.ObjectInfo GetObjectInfo(uint tid)
    {
        if (null == CurrentMapData)
            return null;

        var info = CurrentMapData.ObjectInfos.Find((item) => item.TableTID == tid);

        return info;
    }

    /// <summary> 맵정보에서 portal 정보를 얻어온다. </summary>
    public MapData.PortalInfo GetPortalInfo(uint tid)
    {
        if (null == CurrentMapData)
            return null;

        var info = CurrentMapData.PortalInfos.Find((item) => item.TableTID == tid);

        return info;
    }
	#endregion

	#region 현재 게임모드에 중요 정보 전달 //////////////////////////////////////

	/// <summary> 씬로드 완료시  </summary>
	public void SceneLoadComplete()
    {
        reserveActionSceneLoadComplete?.Invoke( CurrentGameModeType );
        reserveActionSceneLoadComplete = null;

        currentGameMode.SceneLoadComplete();
    }

    #endregion

    #region 이벤트들 //////////////////////////////////////

    public void AddEventCompleteMapdataLoad(System.Action action)
    {
        if (CurrentMapData != null)
        {
            action?.Invoke();
        }

        RemoveEventCompleteMapDataLoad(action);
        mEventCompleteMapdataLoad += action;
    }

    public void RemoveEventCompleteMapDataLoad(System.Action action)
    {
        mEventCompleteMapdataLoad -= action;
    }

    public void ReserveActionSceneLoadedComplete( System.Action<E_GameModeType> action )
    {
        reserveActionSceneLoadComplete -= action;
        reserveActionSceneLoadComplete += action;
    }

	#endregion
}

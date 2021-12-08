using GameDB;
using MmoNet;
using System;
using System.Collections.Generic;
using ZNet.Data;

public class ZGameModeInfinity : ZGameModeBase
{
    public override E_GameModeType GameModeType { get { return E_GameModeType.Infinity; } }
    protected override E_GameModePrepareStateType CheckPrepareStateType { get; } = E_GameModePrepareStateType.DefaultCheckState | E_GameModePrepareStateType.CreateMyPc | E_GameModePrepareStateType.MapDataLoadComplete;

    private Action<bool> EventInstanceFinish;
    private Action EventAutoReturnCountDown;
    private Action EventCountMonsterKill;
    private Action EventCompleteDungeonInfoLoad;
    private InfinityTowerContainer Container;

	protected override void EnterGameMode()
	{
        Container = Me.CurCharData.InfinityTowerContainer;

        ResetInfinityDungeonData();

        ZPawnManager.Instance.DoAddEventDieEntity(EntityDieEvent);
    }

	protected override void ExitGameMode()
    {
        ZPawnManager.Instance.DoRemoveEventDieEntity(EntityDieEvent);

        EventAutoReturnCountDown = null;
        EventCountMonsterKill = null;
        EventInstanceFinish = null;
        EventCompleteDungeonInfoLoad = null;
    }

    protected override void StartGameMode()
    {        
    }

    private void EntityDieEvent(uint entityId, ZPawn pawn)
    {
        if (pawn.IsMyPc)
        {
            return;
        }

        Container.NormalMonsterKillCount++;

        EventCountMonsterKill?.Invoke();
    }

    public void AddEventCountMonsterKill(Action action)
    {
        RemoveEventCountMonsterKill(action);
        EventCountMonsterKill += action;
    }

    public void RemoveEventCountMonsterKill(Action action)
	{
        EventCountMonsterKill -= action;
	}

    // 자동 귀환 남은 시간 이벤트
    public void AddEventAutoReturnCountDown(Action action)
    {
        RemoveEventAutoReturnCountDown(action);
        EventAutoReturnCountDown += action;
    }

    public void RemoveEventAutoReturnCountDown(Action action)
	{
        EventAutoReturnCountDown -= action;
	}

    // 던전 종료 이벤트
    public void AddEventInstanceFinish(Action<bool> action)
    {
        RemoveEventInstanceFinish(action);
        EventInstanceFinish += action;
    }

    public void RemoveEventInstanceFinish(Action<bool> action)
	{
        EventInstanceFinish -= action;
	}

    public void AddEventCompleteDungeonInfoLoad(Action action)
	{
        RemoveEventCompleteDungeonInfoLoad(action);
        EventCompleteDungeonInfoLoad += action;
	}

    public void RemoveEventCompleteDungeonInfoLoad(Action action)
	{
        EventCompleteDungeonInfoLoad -= action;
	}

    // info.RemainSec > 0 이면 던전 입장 시 남은 시간 갱신
    // info.ExpireDt > 0 이면 던전 종료 후 자동 마을 귀환 시간 갱신
    public override void RECV_StageState(S2C_StageState info)
    {
        base.RECV_StageState(info);
        
        if(info.RemainSec > 0)
        {
            Container.DungeonEndTime = TimeManager.NowSec + info.RemainSec;
        }
        
        if (info.ExpireDt > 0)
        {
            Container.ReturnTownRemainTime = info.ExpireDt;

            EventAutoReturnCountDown?.Invoke();
        }
    }

    public override void RECV_InstanceFinish(S2C_InstanceFinish info)
    {
        ResetInfinityDungeonData();
        base.RECV_InstanceFinish(info);

        uint dungeonId = Container.ChallengeDungeonStage.DungeonID;

        EventInstanceFinish?.Invoke(info.IsClear);

        if (info.IsClear)
        {
            AudioManager.Instance.PlaySFX(30003);
            ZWebManager.Instance.WebGame.REQ_InfinityDungeonClearReward(dungeonId, delegate
            {
                
            });
        }
    }
    protected override void OnMapDataLoadComplete()
    {
        SetMapData();
        EventCompleteDungeonInfoLoad?.Invoke();
    }

    public void SetMapData()
	{
        Container.NormalMonsterKillCount = 0;
        Container.NormalMonsterSpawnCount = 0;

        Dictionary<uint, uint> monsterTableDic = new Dictionary<uint, uint>();

        for (int i = 0; i < Container.ChallengeDungeonStage.NormalMonster.Count; i++)
        {
            string[] str = Container.ChallengeDungeonStage.NormalMonster[i].Split(':');

            monsterTableDic.Add(Convert.ToUInt32(str[0]), Convert.ToUInt32(str[1]));
        }

        for (int i = 0; i < ZGameModeManager.Instance.CurrentMapData.MonsterInfos.Count; i++)
        {
            if (monsterTableDic.TryGetValue(ZGameModeManager.Instance.CurrentMapData.MonsterInfos[i].GroupID, out uint monsterTableId))
            {
                if (DBMonster.TryGet(monsterTableId, out Monster_Table table))
                {
                    Container.NormalMonsterSpawnCount += table.SpawnCnt;
                }
            }
        }

        Container.IsBossStage = !string.IsNullOrEmpty(Container.ChallengeDungeonStage.UseRankerName);
    }

    private void ResetInfinityDungeonData()
	{
        Container.NormalMonsterKillCount = 0;
        Container.NormalMonsterSpawnCount = 0;
        Container.ReturnTownRemainTime = 0;
        Container.DungeonEndTime = 0;
	}
}
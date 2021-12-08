using GameDB;
using MmoNet;
using System;
using System.Collections.Generic;

public class ZGameModeGuildDungeon : ZGameModeBase
{
	public override E_GameModeType GameModeType { get { return E_GameModeType.GuildDungeon; } }
	protected override E_GameModePrepareStateType CheckPrepareStateType { get; } = E_GameModePrepareStateType.DefaultCheckState | E_GameModePrepareStateType.CreateMyPc;

	private Action<bool> EventInstanceFinish;
	private Action EventCountBossKill;
	private Action EventCompleteDungeonInfoLoad;

	protected override void ExitGameMode()
	{
		ZPawnManager.Instance.DoRemoveEventDieEntity(EntityDieEvent);

		EventInstanceFinish = null;
		EventCountBossKill = null;
		EventCompleteDungeonInfoLoad = null;
	}

	protected override void StartGameMode()
	{
		ZNet.Data.Me.CurCharData.GuildDungeonContainer.RemainTime = 0;

		ZPawnManager.Instance.DoAddEventDieEntity(EntityDieEvent);
	}

	private void EntityDieEvent(uint entityId, ZPawn pawn)
	{
		if (pawn.IsMyPc)
		{
			return;
		}

		ZNet.Data.Me.CurCharData.GuildDungeonContainer.BossMonsterKillCount++;

		EventCountBossKill?.Invoke();
	}

	public void AddEventCountMonsterKill(Action action)
	{
		RemoveEventCountMonsterKill(action);
		EventCountBossKill += action;
	}

	public void RemoveEventCountMonsterKill(Action action)
	{
		EventCountBossKill -= action;
	}

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

	public override void RECV_StageState(S2C_StageState info)
	{
		base.RECV_StageState(info);

		if (info.RemainSec > 0)
		{
			ZNet.Data.Me.CurCharData.GuildDungeonContainer.RemainTime = TimeManager.NowSec + info.RemainSec;
		}
	}

	public override void RECV_InstanceFinish(S2C_InstanceFinish info)
	{
		base.RECV_InstanceFinish(info);
		ZLog.Log(ZLogChannel.UI, $"## RECV_InstanceFinish {info.IsClear}");
		ZNet.Data.Me.CurCharData.GuildDungeonContainer.EndTime = TimeManager.NowSec + DBConfig.GuildDungeon_Close_Exit_Time;

		EventInstanceFinish?.Invoke(info.IsClear);

		if (info.IsClear)
		{
			AudioManager.Instance.PlaySFX(30003);
		}
	}

	protected override void OnMapDataLoadComplete()
	{
		SetMapData();
		EventCompleteDungeonInfoLoad?.Invoke();
	}

	public void SetMapData()
	{
		for (int i = 0; i < ZGameModeManager.Instance.CurrentMapData.MonsterInfos.Count; i++)
		{
			if (DBMonster.TryGet(ZGameModeManager.Instance.CurrentMapData.MonsterInfos[i].TableTID, out Monster_Table monster))
			{
				ZNet.Data.Me.CurCharData.GuildDungeonContainer.BossMonsterSpawnCount += monster.SpawnCnt;
			}
		}
	}
}

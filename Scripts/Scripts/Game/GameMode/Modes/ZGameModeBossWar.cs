using System;
using System.Collections.Generic;

public class ZGameModeBossWar : ZGameModeBase
{
	public override E_GameModeType GameModeType { get { return E_GameModeType.BossWar; } }

	protected override E_GameModePrepareStateType CheckPrepareStateType { get; } = E_GameModePrepareStateType.DefaultCheckState | E_GameModePrepareStateType.CreateMyPc;

	private Action<List<BossWarPointRanking>> EventRankingUpdate;
	private Action EventMyPointUpdate;
	private Action EventBossDead;
	private Action EventBossWarExit;

	protected override void ExitGameMode()
	{
		ZNet.Data.Me.CurCharData.BossWarContainer.Location = BossWarContainer.E_Location.None;

		EventBossWarExit?.Invoke();
		EventBossWarExit = null;
		EventRankingUpdate = null;
		EventMyPointUpdate = null;
		EventBossDead = null;
	}

	protected override void PreStartGameMode()
	{
		
	}

	protected override void StartGameMode()
	{
		if (DBMonster.TryGet(ZGameModeManager.Instance.Table.SummonBossID, out GameDB.Monster_Table monster))
		{
			ZNet.Data.Me.CurCharData.BossWarContainer.BossName = DBLocale.GetText(monster.MonsterTextID);
		}

		ZNet.Data.Me.CurCharData.BossWarContainer.MyPoint = 0;
	}

	public void AddEventBossWarExit(Action action)
	{
		RemoveEventBossWarExit(action);
		EventBossWarExit += action;
	}

	public void RemoveEventBossWarExit(Action action)
	{
		EventBossWarExit -= action;
	}

	public void AddEventRankingUpdate(Action<List<BossWarPointRanking>> action)
	{
		RemoveEventRankingUpdate(action);
		EventRankingUpdate += action;
	}

	public void RemoveEventRankingUpdate(Action<List<BossWarPointRanking>> action)
	{
		EventRankingUpdate -= action;
	}

	public void AddEventMyPointUpdate(Action action)
	{
		RemoveEventMyPointUpdate(action);
		EventMyPointUpdate += action;			
	}

	public void RemoveEventMyPointUpdate(Action action)
	{
		EventMyPointUpdate -= action;
	}

	public void AddEventBossDead(Action action)
	{
		RemoveEventBossDead(action);
		EventBossDead += action;
	}

	public void RemoveEventBossDead(Action action)
	{
		EventBossDead -= action;
	}

	public void RECV_BossWarPointRankingList(MmoNet.S2C_BossPointRankingList info)
	{
		List<BossWarPointRanking> list = new List<BossWarPointRanking>();

		for (int i = 0; i < info.PointListLength; i++)
		{
			uint rankNum = (uint)i + 1;
			var rank = info.PointList(i);

			if(rank.HasValue)
			{
				list.Add(new BossWarPointRanking(rankNum, rank.Value.CharId, rank.Value.Point, rank.Value.ServerIdx));
			}
		}

		EventRankingUpdate?.Invoke(list);
	}

	public void RECV_BossWarDeathPenalty(MmoNet.S2C_BossPointDeathPenalty info)
	{

	}

	public void RECV_BossWarMyPlayerPoint(MmoNet.S2C_UserBossPoint info)
	{
		ZNet.Data.Me.CurCharData.BossWarContainer.MyPoint = info.Point;
		EventMyPointUpdate?.Invoke();
	}

	public void RECV_BossDead()
	{
		EventBossDead?.Invoke();
	}
}

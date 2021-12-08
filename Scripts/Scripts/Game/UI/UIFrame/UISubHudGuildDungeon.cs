using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISubHudGuildDungeon : ZUIFrameBase
{
	[SerializeField] private ZText DungeonTitle;
	[SerializeField] private ZText DungeonClearCondition;
	[SerializeField] private ZText DungeonRemainTime;
	[SerializeField] private ZText AutoReturnRemainTime;
	[SerializeField] private GameObject DungeonEndObject;

	private ZGameModeGuildDungeon gameMode;

	protected override void OnInitialize()
	{
		base.OnInitialize();
	}

	protected override void OnShow(int _LayerOrder)
	{
		AddEvent();

		base.OnShow(_LayerOrder);
	}

	protected override void OnHide()
	{
		base.OnHide();
	}

	private void AddEvent()
	{
		gameMode = ZGameModeManager.Instance.CurrentGameMode<ZGameModeGuildDungeon>();

		if (gameMode != null)
		{
			gameMode.AddEventInstanceFinish(DungeonFinish);
			gameMode.AddEventCountMonsterKill(UpdateBossMonsterKillCount);

			if (ZGameModeManager.Instance.CurrentMapData.StageTID == ZGameModeManager.Instance.StageTid)
			{
				gameMode.SetMapData();
				DungeonStart();
			}
			else
			{
				gameMode.AddEventCompleteDungeonInfoLoad(DungeonStart);
			}
		}
	}

	private void RemoveEvent()
	{
		if (gameMode != null)
		{
			gameMode.RemoveEventInstanceFinish(DungeonFinish);
			gameMode.RemoveEventCountMonsterKill(UpdateBossMonsterKillCount);
			gameMode.RemoveEventCompleteDungeonInfoLoad(DungeonStart);
		}
	}

	private void DungeonStart()
	{
		DungeonTitle.text = DBLocale.GetText(ZGameModeManager.Instance.Table.StageTextID);
		DungeonClearCondition.text = DBLocale.GetText("Guild_Dungeon_Quest_Des", ZNet.Data.Me.CurCharData.GuildDungeonContainer.CurrentBossMonsterName, ZNet.Data.Me.CurCharData.GuildDungeonContainer.BossMonsterKillCount, ZNet.Data.Me.CurCharData.GuildDungeonContainer.BossMonsterSpawnCount);

		DungeonEndObject.SetActive(false);
		InvokeRepeating(nameof(UpdateDungeonRemainTime), 0, 1.0f);
	}

	private void DungeonFinish(bool isClear)
	{
		DungeonEndObject.SetActive(true);
		InvokeRepeating(nameof(UpdateReturnTownRemainTime), 0, 1.0f);

		if(isClear)
		{
			List<GuildDungeonClearReward> itemList = new List<GuildDungeonClearReward>();

			for(int i = 0; i < ZGameModeManager.Instance.Table.ClearRewardID.Count; i++)
			{
				itemList.Add(new GuildDungeonClearReward(ZGameModeManager.Instance.Table.ClearRewardID[i], ZGameModeManager.Instance.Table.ClearRewardCount[i]));
			}

			UIManager.Instance.Open<UIFrameItemRewardShot>((str, frame) =>
			{
				frame.AddItem(itemList);
			});
		}
	}

	private void UpdateBossMonsterKillCount()
	{
		DungeonClearCondition.text = DBLocale.GetText("Guild_Dungeon_Quest_Des", ZNet.Data.Me.CurCharData.GuildDungeonContainer.CurrentBossMonsterName, ZNet.Data.Me.CurCharData.GuildDungeonContainer.BossMonsterKillCount, ZNet.Data.Me.CurCharData.GuildDungeonContainer.BossMonsterSpawnCount);
	}

	private void UpdateReturnTownRemainTime()
	{
		CancelInvoke(nameof(UpdateDungeonRemainTime));

		if (ZNet.Data.Me.CurCharData.GuildDungeonContainer.EndTime <= TimeManager.NowSec)
		{
			CancelInvoke(nameof(UpdateReturnTownRemainTime));

			AutoReturnRemainTime.text = DBLocale.GetText("Infinity_Dungeon_Time", 0);
			GoToTown();
			return;
		}

		float remainTime = Mathf.Clamp(ZNet.Data.Me.CurCharData.GuildDungeonContainer.EndTime - TimeManager.NowSec, 0, ZNet.Data.Me.CurCharData.GuildDungeonContainer.EndTime - TimeManager.NowSec);

		string time = TimeHelper.GetRemainFullTime((ulong)remainTime);

		AutoReturnRemainTime.text = DBLocale.GetText("Infinity_Dungeon_Time", time);
	}

	private void UpdateDungeonRemainTime()
	{
		if(ZNet.Data.Me.CurCharData.GuildDungeonContainer.RemainTime <= TimeManager.NowSec)
		{
			CancelInvoke(nameof(UpdateDungeonRemainTime));

			DungeonRemainTime.text = DBLocale.GetText("Infinity_Dungeon_Time", 0);
			GoToTown();
			return;
		}

		float remainTime = Mathf.Clamp(ZNet.Data.Me.CurCharData.GuildDungeonContainer.RemainTime - TimeManager.NowSec, 0, ZNet.Data.Me.CurCharData.GuildDungeonContainer.RemainTime - TimeManager.NowSec);

		string time = TimeHelper.GetRemainFullTime((ulong)remainTime);

		DungeonRemainTime.text = DBLocale.GetText("Infinity_Dungeon_Time", time);
	}

	public void DungeonExitMessagePopup()
	{
		UIMessagePopup.ShowPopupOkCancel(DBLocale.GetText("Guild_Dungeon_GoOut_Des"), GoToTown);
	}

	public void GoToTown()
	{
		RemoveEvent();

		CancelInvoke(nameof(UpdateDungeonRemainTime));
		CancelInvoke(nameof(UpdateReturnTownRemainTime));

		uint portalTid = DBConfig.Town_Portal_ID;

		if (DBStage.TryGet(ZGameModeManager.Instance.StageTid, out var stageTable))
		{
			if (0 < stageTable.DeathPortal)
			{
				portalTid = stageTable.DeathPortal;
			}
			else
			{
				ZLog.LogError(ZLogChannel.Map, $"해당 스테이지({ZGameModeManager.Instance.StageTid})의 DeathPortal이 셋팅되지 않았다.");
			}
		}
		else
		{
			ZLog.LogError(ZLogChannel.Map, $"해당 스테이지({ZGameModeManager.Instance.StageTid})를 찾을 수 없다.");
		}

		ZGameManager.Instance.TryEnterStage(portalTid, false, 0, 0);
	}
}

using GameDB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISubHudBossWarInfo : ZUIFrameBase
{
	[SerializeField] private GameObject DamagePanel;
	[SerializeField] private GameObject PortalIcon;
	[SerializeField] private ZText RemainTime;
	[SerializeField] private UIFrameBossWarReward BossWarReward;
	[SerializeField] private UIBossWarPointRankingScrollAdapter ScrollAdapter;
	[SerializeField] private UIBossWarRankingListItem RankingListItem;
	[SerializeField] private GameObject DungeonEndTime;
	[SerializeField] private ZText ReturnTownRemainTime;
	[SerializeField] private ZText MyPlayerDamagePoint;

	private ulong TownRemainTime = 0;
	private float MyRankingPoint = 0;
	private UIPopupItemInfo InfoPopup;
	private BossWarContainer Container;

	protected override void OnInitialize()
	{
		// 공헌도 순위 리스트 아이템 초기화
		ScrollAdapter.Parameters.ItemPrefab = RankingListItem.GetComponent<RectTransform>();
		var prefab = ScrollAdapter.Parameters.ItemPrefab;
		prefab.SetParent(RankingListItem.transform.parent);
		prefab.localScale = Vector2.one;
		prefab.localPosition = Vector3.zero;
		prefab.gameObject.SetActive(false);
		ScrollAdapter.Initialize();

		base.OnInitialize();
	}

	protected override void OnShow(int _LayerOrder)
	{
		Container = ZNet.Data.Me.CurCharData.BossWarContainer;

		var gameMode = ZGameModeManager.Instance.CurrentGameMode<ZGameModeBossWar>();

		if (gameMode != null)
		{
			gameMode.AddEventRankingUpdate(PointRankingUpdate);
			gameMode.AddEventBossDead(BossDead);
			gameMode.AddEventMyPointUpdate(MyDamagePointUpdate);
			gameMode.AddEventBossWarExit(BossWarExit);
		}

		if (ZGameModeManager.Instance.CurrentGameModeType == E_GameModeType.BossWar)
		{
			MyDamagePointUpdate();

			if (Container.KillableStartTsSec > TimeManager.NowSec)
			{
				CancelInvoke(nameof(UpdateFieldEnterRemainTime));
				InvokeRepeating(nameof(UpdateFieldEnterRemainTime), 0f, 1.0f);
			}
			else if (Container.KillableStartTsSec <= TimeManager.NowSec && Container.RoomExpireTsSec > TimeManager.NowSec)
			{
				CancelInvoke(nameof(UpdateBossWarEndRemainTime));
				InvokeRepeating(nameof(UpdateBossWarEndRemainTime), 0f, 1.0f);
			}

			//InvokeRepeating(nameof(UpdateRemainTIme), 0f, 1.0f);
		}

		base.OnShow(_LayerOrder);
	}
	
	protected override void OnHide()
	{
		base.OnHide();

		if (ZGameModeManager.Instance.CurrentGameModeType == E_GameModeType.BossWar)
		{
			return;
		}

		if (DungeonEndTime.activeSelf)
		{
			DungeonEndTime.SetActive(false);
		}

		if (BossWarReward.gameObject.activeSelf)
		{
			BossWarReward.gameObject.SetActive(false);
		}
	}

	private void BossWarExit()
	{
		if (UIManager.Instance.Find(out UIFrameBossWarPortalPopup portalPopup)) 
			portalPopup.Close();

		ScrollAdapter.RefreshData();
	}

	// 내 데미지 포인트 갱신
	private void MyDamagePointUpdate()
	{
		MyPlayerDamagePoint.text = Container.MyPoint.ToString();
	}

	// 보스가 죽었을 경우 (수령가능한 공헌도 보상이 잇다면 팝업 오픈)
	private void BossDead()
	{
		TownRemainTime = DBConfig.BossWar_End_AfterTime + TimeManager.NowSec;
		DungeonEndTime.SetActive(true);
		CancelInvoke(nameof(UpdateFieldEnterRemainTime));
		CancelInvoke(nameof(UpdateBossWarEndRemainTime));
		InvokeRepeating(nameof(UpdateReturnTownTime), 0, 1.0f);

		BossWar_Table reward = GetGradeReward();

		if (reward != null)
		{
			OpenBossWarRewardPopup(reward);
		}
	}

	private void UpdateReturnTownTime()
	{
		if(TownRemainTime <= TimeManager.NowSec)
		{
			CancelInvoke(nameof(UpdateReturnTownTime));
			GoToTown();
			return;
		}
				
		ulong remainTime =  TownRemainTime - TimeManager.NowSec;

		ReturnTownRemainTime.text = DBLocale.GetText("Inter_Server_Boss_Dead", Container.BossName, remainTime);
	}

	// 공헌도 순위 갱신
	public void PointRankingUpdate(List<BossWarPointRanking> list)
	{
		ScrollAdapter.Refresh(list);
	}

	private void UpdateFieldEnterRemainTime()
	{
		float fRemainTime;
		
		if(Container.KillableStartTsSec <= TimeManager.NowSec)
		{
			CancelInvoke(nameof(UpdateFieldEnterRemainTime));

			UICommon.SetNoticeMessage(DBLocale.GetText("Boss_War_Boss_Spawn", Container.BossName), new Color(255, 255, 255), 2f, UIMessageNoticeEnum.E_MessageType.SubNotice);

			InvokeRepeating(nameof(UpdateBossWarEndRemainTime), 0f, 1.0f);
			return;
		}

		fRemainTime = ZNet.Data.Me.CurCharData.BossWarContainer.KillableStartTsSec - TimeManager.NowSec;
		RemainTime.text = DBLocale.GetText("Waiting_Time_Enter_Field", TimeHelper.GetRemainFullTime((ulong)fRemainTime));
	}

	private void UpdateBossWarEndRemainTime()
	{
		float fRemainTime;

		if(ZNet.Data.Me.CurCharData.BossWarContainer.RoomExpireTsSec <= TimeManager.NowSec)
		{
			RemainTime.text = DBLocale.GetText("End_Boss_War");
			CancelInvoke(nameof(UpdateBossWarEndRemainTime));
			GoToTown();
			return;
		}

		fRemainTime = ZNet.Data.Me.CurCharData.BossWarContainer.RoomExpireTsSec - TimeManager.NowSec;
		RemainTime.text = DBLocale.GetText("Boss_War_End_Time", TimeHelper.GetRemainFullTime((ulong)fRemainTime));
	}

	public void ClickPortalButton()
	{
		if (UIManager.Instance.Find(out UIFrameBossWarPortalPopup portalPopup)) return;

		UIManager.Instance.Open<UIFrameBossWarPortalPopup>((assetName, frame) =>
		{
			frame.Init();
		});
	}

	private void OpenBossWarRewardPopup(BossWar_Table reward)
	{
		UIManager.Instance.TopMost<UISubHudBossWarInfo>(true);
		BossWarReward.gameObject.SetActive(true);
		BossWarReward.Init(reward, GoToTown);
	}

	public void GoToTown()
	{
		UIManager.Instance.TopMost<UISubHudBossWarInfo>(false);
		RemoveInfoPopup();
		CancelInvoke(nameof(UpdateReturnTownTime));

		ZGameManager.Instance.TryEnterStage(DBConfig.Town_Portal_ID, false, 0, 0);
	}

	// 공헌도에 비례한 보상아이템 테이블을 가져옴. 없으면 null
	private BossWar_Table GetGradeReward()
	{
		List<BossWar_Table> rewardTable = new List<BossWar_Table>();
		BossWar_Table currentRewardTable = null;

		foreach (var table in GameDBManager.Container.BossWar_Table_data.Values)
		{
			if (table.StageID == ZNet.Data.Me.CurCharData.BossWarContainer.StageTid)
			{
				rewardTable.Add(table);
			}
		}

		foreach (var table in rewardTable)
		{
			if (table.MinDamage <= ZNet.Data.Me.CurCharData.BossWarContainer.MyPoint && table.MaxDamage >= ZNet.Data.Me.CurCharData.BossWarContainer.MyPoint)
			{
				currentRewardTable = table;
			}
		}

		return currentRewardTable;
	}

	public void SetInfoPopup(UIPopupItemInfo popup)
	{
		if(InfoPopup != null)
		{
			Destroy(InfoPopup.gameObject);
			InfoPopup = null;
		}

		InfoPopup = popup;
	}

	public void RemoveInfoPopup()
	{
		if(InfoPopup != null)
		{
			Destroy(InfoPopup.gameObject);
			InfoPopup = null;
		}
	}
}

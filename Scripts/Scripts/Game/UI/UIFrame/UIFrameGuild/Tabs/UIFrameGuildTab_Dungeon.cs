using GameDB;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using WebNet;
using ZNet.Data;

public class RewardItem
{
	public uint ItemId;
	public uint ItemCount;

	public RewardItem(uint itemId, uint itemCount)
	{
		ItemId = itemId;
		ItemCount = itemCount;
	}
}

public class UIFrameGuildTab_Dungeon : UIFrameGuildTabBase
{
	enum E_GuildDungeonStage
	{
		Wait,
		End,
		Enterable,
	}

	#region SerializedField
	#region Preference Variable
	[SerializeField] private UIGuildDungeonListScrollAdapter ScrollAdapter;
	[SerializeField] private UIDropItemListScrollAdapter DropItemListScrollAdapter;
	[SerializeField] private UIClearItemListScrollAdapter ClearItemListScrollAdapter;
	[SerializeField] private ZText GuildDungeonDescription;
	[SerializeField] private ZText CurrentGuildDungeonName;
	[SerializeField] private ZText CurrentGuildDungeonDescription;
	[SerializeField] private ZText DropItemTitle;
	[SerializeField] private ZText GuildDungeonRewardTitle;
	[SerializeField] private ZText ButtonText;
	[SerializeField] private GameObject BattleStateObject;
	[SerializeField] private ZButton GuildDungeonOpenButton;
	[SerializeField] private ZButton GuildDungeonEnterButton;
	[SerializeField] private ZText OpenButtonText;
	[SerializeField] private ZText EnterButtonText;
	[SerializeField] private ZImage OpenCostIcon;
	[SerializeField] private ZText OpenCostText;
	[SerializeField] private ZText RemainCount;
	[SerializeField] private GameObject CancelButton;
	#endregion

	#region UI Variables
	#endregion
	#endregion

	#region System Variables
	private uint SelectedStageId = 0;
	private E_GuildDungeonStatus DungeonState = E_GuildDungeonStatus.None;
	private Stage_Table CurrentStage;
	private Monster_Table CurrentBossMonster;
	private List<Stage_Table> GuildDungeonList = new List<Stage_Table>();
	private bool IsMasterOrSubMaster = false;
	private MyGuildDungeonInfo CurrentDungeonInfo;
	private byte TotalOpenableCount;
	#endregion

	#region Properties 
	#endregion

	#region Unity Methods
	#endregion

	#region Public Methods
	#endregion
	
	#region Overrides 
	/// <summary>
	/// 최초에 한번만 호출됩니다 
	/// </summary>
	public override void Initialize(UIFrameGuild guildFrame, FrameGuildTabType type)
	{
		base.Initialize(guildFrame, type);

		var itemPrefab = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIGuildDungeonListItem));
		ScrollAdapter.Parameters.ItemPrefab = itemPrefab.GetComponent<RectTransform>();
		ScrollAdapter.Parameters.ItemPrefab.SetParent(transform);
		ScrollAdapter.Parameters.ItemPrefab.localScale = Vector2.one;
		ScrollAdapter.Parameters.ItemPrefab.localPosition = Vector3.zero;
		ScrollAdapter.Parameters.ItemPrefab.gameObject.SetActive(false);

		ScrollAdapter.Initialize(ClickDungeonList);

		var rewardItemPrefab = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIRewardableListItem));
		ClearItemListScrollAdapter.Parameters.ItemPrefab = rewardItemPrefab.GetComponent<RectTransform>();
		ClearItemListScrollAdapter.Parameters.ItemPrefab.SetParent(transform);
		ClearItemListScrollAdapter.Parameters.ItemPrefab.localScale = Vector2.one;
		ClearItemListScrollAdapter.Parameters.ItemPrefab.localPosition = Vector3.zero;
		ClearItemListScrollAdapter.Parameters.ItemPrefab.gameObject.SetActive(false);

		ClearItemListScrollAdapter.Initialize(OnRewardListItemClick);

		DropItemListScrollAdapter.Parameters.ItemPrefab = rewardItemPrefab.GetComponent<RectTransform>();

		DropItemListScrollAdapter.Initialize(OnRewardListItemClick);

		InitDungeonList();
		InitDisplayString();
	}

	/// <summary>
	/// 던전 탭이 열릴때 호출됩니다
	/// </summary>
	public override void OnOpen()
	{
		base.OnOpen();

		ZGameModeManager.Instance.mEventGuildDungeonStateChange += RefreshDungeonDisplayAfterDungeonOpen;

		SelectedStageId = 0;

		foreach (var table in GameDBManager.Container.Guild_Table_data.Values)
		{
			if (table.GuildLevel == UIFrameGuildNetCapturer.MyGuildData.myGuildInfo.level)
			{
				TotalOpenableCount = table.DungeonOpenCnt;
			}
		}

		IsMasterOrSubMaster = UIFrameGuildNetCapturer.AmIMaster || UIFrameGuildNetCapturer.AmISubMaster;

		Me.CurCharData.GuildDungeonContainer.GuildDungeonInfoList.Clear();

		ZWebManager.Instance.WebGame.REQ_GuildDungeonInfo(Me.CurCharData.GuildId, (reqPacket, recvPacket) =>
		{
			for(int i = 0; i < recvPacket.InfoLength; i++)
			{
				Me.CurCharData.GuildDungeonContainer.AddGuildDungeonInfo(recvPacket.Info(i));
			}

			Me.CurCharData.GuildDungeonContainer.LastOpenDt = recvPacket.DungeonLastOpenDt;
			Me.CurCharData.GuildDungeonContainer.OpenCount = recvPacket.DungeonOpenCnt;
			
			for (int i = 0; i < Me.CurCharData.GuildDungeonContainer.GuildDungeonInfoList.Count; i++)
			{
				ZLog.Log(ZLogChannel.UI, $"## StageTid {Me.CurCharData.GuildDungeonContainer.GuildDungeonInfoList[i].StageTid}, OpenGuildId {Me.CurCharData.GuildDungeonContainer.GuildDungeonInfoList[i].OpenGuildId}, OpenTsSec {Me.CurCharData.GuildDungeonContainer.GuildDungeonInfoList[i].OpenTsSec}, RoomNo {Me.CurCharData.GuildDungeonContainer.GuildDungeonInfoList[i].RoomNo}, Addr {Me.CurCharData.GuildDungeonContainer.GuildDungeonInfoList[i].Addr}, BossKillTsSec {Me.CurCharData.GuildDungeonContainer.GuildDungeonInfoList[i].BossKillTsSec}, IsClose {Me.CurCharData.GuildDungeonContainer.GuildDungeonInfoList[i].Status}");
			}

			if (GuildDungeonList.Count > 0)
			{
				ClickDungeonList(GuildDungeonList[0]);
			}
		});
	}

	/// <summary>
	/// 던전 탭이 닫힐때 호출댑니다
	/// </summary>
	public override void OnClose()
	{
		ZGameModeManager.Instance.mEventGuildDungeonStateChange -= RefreshDungeonDisplayAfterDungeonOpen;

		CloseInfoPopup();
		base.OnClose();
	}

	public override void OnUpdateEventRise(UpdateEventType type, GuildDataUpdateEventParamBase param)
	{
		base.OnUpdateEventRise(type, param);
	}
	#endregion

	#region Private Methods
	private void InitDisplayString()
	{
		GuildDungeonDescription.text = DBLocale.GetText("GuildDungeon_Notice");
		DropItemTitle.text = "획득 가능 아이템";
		GuildDungeonRewardTitle.text = "길드 토벌 성공 보수";
	}

	// 길드 던전 리스트 세팅
	private void InitDungeonList()
	{
		GuildDungeonList = DBStage.GetStageList(E_StageType.GuildDungeon);

		ScrollAdapter.Refresh(GuildDungeonList);
	}

	// 현재 길드 던전에 해당하는 정보 세팅
	private void UpdateGuildDungeonDisplay(Stage_Table stage)
	{
		CurrentGuildDungeonName.text = DBLocale.GetText(stage.StageTextID);
		CurrentGuildDungeonDescription.text = DBLocale.GetText(stage.StageDescID);
		RemainCount.text = DBLocale.GetText("GuildDungeon_Enterable_Counter", TotalOpenableCount - Me.CurCharData.GuildDungeonContainer.OpenCount, TotalOpenableCount);
		Item_Table item = DBItem.GetItem(CurrentStage.OpenUseItemID);
		OpenCostIcon.sprite = ZManagerUIPreset.Instance.GetSprite(item.IconID);

		OpenCostText.color = UIFrameGuildNetCapturer.MyGuildData.myGuildInfo.money < CurrentStage.OpenUseItemCount ? Color.red : Color.white;
		
		OpenCostText.text = CurrentStage.OpenUseItemCount.ToString();

		if (UIManager.Instance.Find(out UIFrameGuild guild))
		{
			guild.ShowBossModel(CurrentStage);
		}

		SetDropItemList();
		SetClearItemList();
		SetButton();
	}

	public void RefreshDungeonDisplayAfterDungeonOpen()
	{
		ZWebManager.Instance.WebGame.REQ_GuildDungeonInfo(Me.CurCharData.GuildId, (reqPacket, recvPacket) =>
		{
			for (int i = 0; i < recvPacket.InfoLength; i++)
			{
				Me.CurCharData.GuildDungeonContainer.AddGuildDungeonInfo(recvPacket.Info(i));
			}

			Me.CurCharData.GuildDungeonContainer.LastOpenDt = recvPacket.DungeonLastOpenDt;
			Me.CurCharData.GuildDungeonContainer.OpenCount = recvPacket.DungeonOpenCnt;

			CurrentDungeonInfo = Me.CurCharData.GuildDungeonContainer.GetGuildDungeonInfo(SelectedStageId);

			SetButton();
		});
	}

	// 길드 멤버 등급, 길드 던전 상태에 따른 버튼 세팅
	private void SetButton()
	{
		DungeonState = CurrentDungeonInfo?.Status ?? E_GuildDungeonStatus.None;

		if (IsMasterOrSubMaster && DungeonState == E_GuildDungeonStatus.None && TotalOpenableCount > Me.CurCharData.GuildDungeonContainer.OpenCount)
		{
			GuildDungeonOpenButton.gameObject.SetActive(true);
			GuildDungeonEnterButton.gameObject.SetActive(false);
		}
		else
		{
			GuildDungeonEnterButton.gameObject.SetActive(true);
			GuildDungeonOpenButton.gameObject.SetActive(false);
		}

		if (CurrentDungeonInfo != null && (CurrentDungeonInfo.OpenTsSec != 0 && CurrentDungeonInfo.OpenTsSec + CurrentStage.ClearLimitTime <= TimeManager.NowSec || (CurrentDungeonInfo.BossKillTsSec != 0 && CurrentDungeonInfo.BossKillTsSec <= TimeManager.NowSec)))
		{
			DungeonState = E_GuildDungeonStatus.Close;
		}

		if(DungeonState == E_GuildDungeonStatus.None && TotalOpenableCount <= Me.CurCharData.GuildDungeonContainer.OpenCount)
		{
			DungeonState = E_GuildDungeonStatus.Close;
		}

		GuildDungeonEnterButton.interactable = DungeonState == E_GuildDungeonStatus.GenerateComp || DungeonState == E_GuildDungeonStatus.Generating;

		BattleStateObject.SetActive(false);

		switch (DungeonState)
		{
			case E_GuildDungeonStatus.None:
				if (IsMasterOrSubMaster)
				{
					if(UIFrameGuildNetCapturer.MyGuildData.myGuildInfo.money >= CurrentStage.OpenUseItemCount)
					{
						GuildDungeonOpenButton.interactable = true;
						OpenButtonText.text = DBLocale.GetText("GuildDungeon_Open_Ready_Master");
					}
					else
					{
						GuildDungeonOpenButton.interactable = false;
						OpenButtonText.text = DBLocale.GetText("GuildDungeon_Open_Cost_Lack");
					}
				}
				else
				{
					EnterButtonText.text = DBLocale.GetText("GuildDungeon_Open_Wait_Member");
				}
				break;
			case E_GuildDungeonStatus.ForceClose:
			case E_GuildDungeonStatus.Close:
				if (IsMasterOrSubMaster)
				{
					EnterButtonText.text = DBLocale.GetText("GuildDungeon_Cannot_Open_Master");
				}
				else
				{
					EnterButtonText.text = DBLocale.GetText("GuildDungeon_Close_Member");
				}
				break;
			case E_GuildDungeonStatus.Generating:
			case E_GuildDungeonStatus.GenerateComp:
				BattleStateObject.SetActive(true);
				CancelButton.SetActive(IsMasterOrSubMaster);
				EnterButtonText.text = DBLocale.GetText("GuildDungeon_Open_Now_All");
				break;
		}
	}

	// 길드 던전 클리어 보상 아이템 세팅
	private void SetClearItemList()
	{
		List<RewardItem> itemList = new List<RewardItem>();

		for(int i = 0; i < CurrentStage.ClearRewardID.Count; i++)
		{
			itemList.Add(new RewardItem(CurrentStage.ClearRewardID[i], CurrentStage.ClearRewardCount[i]));
		}

		ClearItemListScrollAdapter.Refresh(itemList);
	}

	// 획득 가능한 드랍 아이템 세팅
	private void SetDropItemList()
	{
		if(CurrentBossMonster == null)
		{
			ZLog.LogError(ZLogChannel.UI, $"## 보스 데이터가 존재하지 않습니다.");
			return;
		}

		List<Item_Table> tableList = new List<Item_Table>();
		List<RewardItem> itemList = new List<RewardItem>();

		List<MonsterDrop_Table> dropList = new List<MonsterDrop_Table>();

		var it = GameDBManager.Container.MonsterDrop_Table_data.GetEnumerator();

		while(it.MoveNext())
		{
			if(CurrentBossMonster.DropGroupID == it.Current.Value.DropGroupID)
			{
				dropList.Add(it.Current.Value);
			}
		}

		for(int i = 0; i < dropList.Count; i++)
		{
			if(GameDBManager.Container.Item_Table_data.TryGetValue(dropList[i].DropItemID, out var itemTable))
			{
				if (itemTable.DropModelType == E_DropModelType.Money)
				{
					continue;
				}

				tableList.Add(itemTable);
			}
		}
		
		tableList.Sort((a, b) =>
		{
			return b.Grade.CompareTo(a.Grade);
		});

		foreach(var t in tableList)
		{
			itemList.Add(new RewardItem(t.ItemID, 1));
		}

		DropItemListScrollAdapter.Refresh(itemList);
	}

	private void CloseInfoPopup()
	{
		if (UIManager.Instance.Find(out UIFrameGuild guild)) guild.RemoveInfoPopup();
	}

	private void OpenGuildDungeon()
	{
		ZLog.Log(ZLogChannel.UI, $"## Money : {UIFrameGuildNetCapturer.MyGuildData.myGuildInfo.money}, OpenUseItemCount : {CurrentStage.OpenUseItemCount}");
		if(UIFrameGuildNetCapturer.MyGuildData.myGuildInfo.money < CurrentStage.OpenUseItemCount)
		{
			UICommon.SetNoticeMessage("오픈 아이템이 부족합니다.", Color.red, 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
			ZLog.Log(ZLogChannel.UI, $"오픈 아이템이 부족합니다. {CurrentStage.OpenUseItemID}");
			return;
		}

		ZWebManager.Instance.WebGame.REQ_GuildDungeonOpen(Me.CurCharData.GuildId, CurrentStage.StageID, (reqPacket, recvPacket) =>
		{
			Me.CurCharData.GuildDungeonContainer.LastOpenDt = recvPacket.RemainDungeonLastOpenDt;
			Me.CurCharData.GuildDungeonContainer.OpenCount = recvPacket.RemainDungeonOpenCnt;
			Me.CurCharData.GuildDungeonContainer.AddGuildDungeonInfo(recvPacket.Info);
			UIFrameGuildNetCapturer.MyGuildData.myGuildInfo.money = recvPacket.RemainGuildMoney;
			CurrentDungeonInfo = Me.CurCharData.GuildDungeonContainer.GetGuildDungeonInfo(SelectedStageId);

			BattleStateObject.SetActive(true);
			CancelButton.SetActive(IsMasterOrSubMaster);
			SetButton();
		});
	}

	private void CancelGuildDungeon()
	{
		ZWebManager.Instance.WebGame.REQ_GuildDungeonClose(CurrentStage.StageID, (reqPacket, recvPacket) =>
		{
			Me.CurCharData.GuildDungeonContainer.AddGuildDungeonInfo(recvPacket.Info);
			BattleStateObject.SetActive(false);
			SetButton();
		});
	}

	#endregion

	#region OnClick Event (인스펙터 연결)
	private void ClickDungeonList(Stage_Table stage)
	{
		if(SelectedStageId == stage.StageID)
		{
			return;
		}

		if (UIManager.Instance.Find(out UIFrameGuild guild)) guild.RemoveInfoPopup();
		
		CurrentStage = stage;
		SelectedStageId = stage.StageID;
		CurrentBossMonster = DBMonster.Get(stage.SummonBossID);
		Me.CurCharData.GuildDungeonContainer.CurrentBossMonsterName = DBLocale.GetText(CurrentBossMonster.MonsterTextID);

		ScrollAdapter.UpdateScrollItem(SelectedStageId);
		CurrentDungeonInfo = Me.CurCharData.GuildDungeonContainer.GetGuildDungeonInfo(stage.StageID);

		UpdateGuildDungeonDisplay(stage);
	}

	private void OnRewardListItemClick(uint itemTid)
	{
		ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIPopupItemInfo), (obj) => 
		{
			var popupItemInfo = obj.GetComponent<UIPopupItemInfo>();
			var frame = UIManager.Instance.Find<UIFrameGuild>();

			if (frame != null)
			{
				frame.SetInfoPopup(popupItemInfo);

				popupItemInfo.transform.SetParent(frame.transform);
				popupItemInfo.Initialize(E_ItemPopupType.Reward, itemTid, () => 
				{
					frame = UIManager.Instance.Find<UIFrameGuild>();

					if (frame != null)
					{
						frame.RemoveInfoPopup();
					}
				});
			}
		});
	}

	// 길드전 전투 중단 (길마, 부길마만 가능)
	public void ClickCancel()
	{
		CloseInfoPopup();

		UIMessagePopup.ShowPopupOkCancel(DBLocale.GetText("Guild_Dungeon_Cancel"), CancelGuildDungeon);
	}

	// 길드전 오픈 (길마, 부길마만 가능)
	public void ClickOpen()
	{
		CloseInfoPopup();
		UIMessagePopup.ShowPopupOkCancel("길드 던전을 오픈하시겠습니까?", OpenGuildDungeon);
	}

	// 길드전 입장
	public void EnterGuildDungeon()
	{
		CloseInfoPopup();
		
		if (DBPortal.TryGet(CurrentStage.DefaultPortal, out Portal_Table portal))
		{
			ZGameManager.Instance.TryEnterGuildDungeon(portal.PortalID);
		}
	}
	#endregion
}

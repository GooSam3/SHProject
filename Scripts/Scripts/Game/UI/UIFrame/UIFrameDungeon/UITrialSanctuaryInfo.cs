using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

/// <summary>
/// 시련의 성역
/// </summary>
public sealed class UITrialSanctuaryInfo : MonoBehaviour, ITabContents
{
    [SerializeField] private GameObject rewardGroup;
    [SerializeField] private GameObject rewardGroup2;

    [SerializeField] private ScrollRect rewardScrollRect;
	[SerializeField] private ScrollRect rewardRuneScrollRect;
	[SerializeField] private ScrollRect dungeonDescScrollRect;
	[SerializeField] private ZText dungeonTitleText;
	[SerializeField] private ZText dungeonBossName;
	[SerializeField] private ZText dungeonDescText;
	[SerializeField] private GameObject dungeonLockImage;
	[SerializeField] private ZText dungeonLockMsgText;
	[SerializeField] private ZButton startButton;
	[SerializeField] private ZText startButtonText;
	[SerializeField] private ZText startCostText;
	[SerializeField] private ZImage startCostImage;
	[SerializeField] private ZText timeInfoText;
	[SerializeField] private ZText timeInfoDesc;
	[SerializeField] private ZText rewardTitle;
	[SerializeField] private ZText rewardRuneTitle;
	[SerializeField] private ContentSizeFitter dugeonDescFitter;
	[SerializeField] private ScrollRect stageScrollRect;

	public int Index { get; set; }

	private List<UITowerListItem> stageListItems = new List<UITowerListItem>();
	private List<UITowerRewardListItem> rewardListItems = new List<UITowerRewardListItem>();
	private List<UITowerRewardRuneListItem> rewardRuneListItems = new List<UITowerRewardRuneListItem>();
	
	private bool isSpawnLoad = false;
	private Action loadEvent;
	private uint selectDungeonTid;

	public void Initialize()
	{
		timeInfoDesc.text = DBLocale.GetText( "Instance_Dungeon_Reset_Notice" );
		rewardTitle.text = DBLocale.GetText( "Despair_Dungeon_Reward" );
		rewardRuneTitle.text = DBLocale.GetText( "Despair_Dungeon_Reward2" );
		startButtonText.text = DBLocale.GetText( "Move_Button" );
		dungeonTitleText.text = string.Empty;
		dungeonBossName.text = string.Empty;
		dungeonDescText.text = string.Empty;
		dungeonLockMsgText.text = string.Empty;
		startCostText.text = string.Empty;
		timeInfoText.text = string.Empty;
		startCostImage.enabled = false;

        rewardGroup.SetActive(false);
        rewardGroup2.SetActive(false);


        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UITowerListItem), delegate {
			ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UITowerRewardListItem), delegate {
				ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UITowerRewardRuneListItem), delegate {
					isSpawnLoad = true;
					if (loadEvent != null)
					{
						loadEvent.Invoke();
						loadEvent = null;
					}
				}, 0, 1, false);
			}, 0, 1, false);
		}, 0, 1, false);
	}

	public void Open()
	{
		this.gameObject.SetActive(true);

		startButton.interactable = false;

		if (isSpawnLoad == false)
		{
			loadEvent = () => { RefreshSelf(false); };
		}
		else
		{
			RefreshSelf(false);
		}
	}

	public void Refresh()
	{
	}

	private void RefreshSelf(bool isResetPos)
	{
		DoStageListUpdate(isResetPos);
		DoInfoUpdate(true);
	}

	public void Close()
	{
		this.gameObject.SetActive(false);
	}

	private void OnStageListItemClick(uint stageTid)
	{
		selectDungeonTid = stageTid;

		RefreshSelf(false);
	}

	private void OnRewardListItemClick(uint itemTid)
	{
		ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIPopupItemInfo), (obj) => {
			var popupItemInfo = obj.GetComponent<UIPopupItemInfo>();
			var frame = UIManager.Instance.Find<UIFrameDungeon>();

			if (frame != null)
			{
				frame.SetInfoPopup(popupItemInfo);

				popupItemInfo.transform.SetParent(frame.transform);
				popupItemInfo.Initialize(E_ItemPopupType.Reward, itemTid, () => {
					frame = UIManager.Instance.Find<UIFrameDungeon>();
					if (frame != null)
					{
						frame.RemoveInfoPopup();
					}
				});
			}
		});
	}

	private void DoStageListUpdate(bool isResetPos)
	{
		var towerList = DBStage.GetStageList(E_StageType.Instance);

		towerList.Sort((a, b) => {
			return a.StageID.CompareTo(b.StageID);
		});

		int loadItemCount = towerList.Count - stageListItems.Count;

		for (int i = 0; i < stageListItems.Count; i++)
		{
			stageListItems[i].gameObject.SetActive(false);
		}

		for (int i = 0; i < loadItemCount; i++)
		{
			UITowerListItem obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UITowerListItem)).GetComponent<UITowerListItem>();
			obj.transform.SetParent(stageScrollRect.content.transform);
			obj.transform.localScale = Vector3.one;
			obj.transform.localPosition = Vector3.zero;

			stageListItems.Add(obj);
			obj.gameObject.SetActive(false);
		}

		if (isResetPos == true)
		{
			stageScrollRect.content.anchoredPosition = Vector2.zero;
			stageScrollRect.velocity = Vector2.zero;
		}

		for (int i = 0; i < towerList.Count; i++)
		{
			// 선택된 던전이 없을때 루프의 최초 item 으로 선택
			if (selectDungeonTid == 0)
			{
				selectDungeonTid = towerList[i].StageID;
			}

			stageListItems[i].DoInit(towerList[i].StageID, OnStageListItemClick);
			stageListItems[i].DoUpdate(selectDungeonTid);
		}
	}


	private void UpdateRemainderTime(ulong remainderTime)
	{
		string title = string.Format("{0} {1}", DBLocale.GetText("WMap_Despair_1FDesc"), DBLocale.GetText("Despair_Tower_Time"));
		string time = TimeHelper.GetRemainFullTimeMin(remainderTime);
		timeInfoText.text = string.Format(title, time);
	}

	private void DoInfoUpdate(bool isResetPos)
	{
		var stageTable = DBStage.Get(selectDungeonTid);
		if (stageTable == null)
		{
			ZLog.LogError(ZLogChannel.Default, $"StageTable이 null이다, selectDungeonTid:{selectDungeonTid}");
			return;
		}

		// 보상 정보 세팅
		DoRewardItemUpdate(stageTable, isResetPos);
		DoRewardRuneItemUpdate(stageTable, isResetPos);

		// etc
		var monsterTable = DBMonster.Get(stageTable.SummonBossID);
		if (monsterTable != null)
		{
			dungeonBossName.text = DBLocale.GetText(monsterTable.MonsterTextID);
		}

		dungeonTitleText.text = DBLocale.GetText(stageTable.StageTextID);

		// 던전 설명
		dungeonDescScrollRect.content.anchoredPosition = Vector2.zero;
		dungeonDescScrollRect.velocity = Vector2.zero;
		dugeonDescFitter.enabled = false;
		dungeonDescText.text = DBLocale.GetText(stageTable.StageDescID);
		CoroutineManager.Instance.NextFrame(() => {
			dugeonDescFitter.enabled = true;
		});

		// 입장제화 표시
		var portalTable = DBPortal.Get(stageTable.DefaultPortal);
		if (portalTable != null)
		{
			if (GameDBManager.Container.Item_Table_data.TryGetValue(portalTable.UseItemID, out var itemTable))
			{
				startCostImage.sprite = ZManagerUIPreset.Instance.GetSprite(itemTable.IconID);
				startCostImage.enabled = true;

				startCostText.text = string.Format("{0}", portalTable.UseItemCount);
			}
		}

		// 남은시간 표시
		timeInfoText.text = string.Format(DBLocale.GetText("Instance_Dungeon_Join_Cnt"), ZNet.Data.Me.CurCharData.InstanceDungeonClearCnt, DBConfig.Instance_Dungeon_Reward_Cnt);

		byte currentLevel;

		if (DBStage.TryGet(ZNet.Data.Me.CurCharData.InstanceDungeonStageTID, out Stage_Table table))
		{
			currentLevel = table.StageLevel;
		}
		else
        {
			currentLevel = 0;
        }

		// 스테이지 잠김 표시 여부
		if (ZNet.Data.Me.CurCharData.Level >= stageTable.InMinLevel && currentLevel >= stageTable.StageLevel - 1)
		{
			dungeonLockImage.SetActive(false);

			startButton.interactable = DBConfig.Instance_Dungeon_Reward_Cnt > ZNet.Data.Me.CurCharData.InstanceDungeonClearCnt && ZGameModeManager.Instance.CurrentGameModeType != E_GameModeType.TrialSanctuary;
		}
		else
		{
			if (currentLevel + 1 < stageTable.StageLevel)
			{
				dungeonLockImage.SetActive(true);
				dungeonLockMsgText.text = string.Format(DBLocale.GetText("Instance_Dungeon_Close_Clear"), stageTable.StageLevel - 1);
			}
			else
			{
				dungeonLockImage.SetActive(true);
				dungeonLockMsgText.text = string.Format(DBLocale.GetText("Instance_Dungeon_Close_Level"), stageTable.InMinLevel);
			}

			startButton.interactable = false;
		}

		// 모델 로드
		UIManager.Instance.Find<UIFrameDungeon>().ShowBossModel( stageTable );
	}

	private void DoRewardItemUpdate(Stage_Table stageTable, bool isResetPos)
	{
		for (int i = 0; i < rewardListItems.Count; i++)
		{
			rewardListItems[i].gameObject.SetActive(false);
		}

		var dropItemList = SearchDropItemList(stageTable, false);
		dropItemList.Sort((a, b) => {
			return b.Grade.CompareTo(a.Grade);
		});

		int loadItemCount = dropItemList.Count - rewardListItems.Count;
		for (int i = 0; i < loadItemCount; i++)
		{
			var obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UITowerRewardListItem)).GetComponent<UITowerRewardListItem>();
			obj.transform.SetParent(rewardScrollRect.content.transform);
			obj.transform.localScale = Vector3.one;
			obj.transform.localPosition = Vector3.zero;
			rewardListItems.Add(obj);
			obj.gameObject.SetActive(false);
		}

		if (isResetPos == true)
		{
			rewardScrollRect.content.anchoredPosition = Vector2.zero;
			rewardScrollRect.velocity = Vector2.zero;
		}

		for (int i = 0; i < dropItemList.Count; i++)
		{
			var item = rewardListItems[i];
			item.gameObject.SetActive(true);
			item.DoUpdate(dropItemList[i].ItemID, 1, OnRewardListItemClick);
		}

        rewardGroup.SetActive(dropItemList.Count > 0);
    }

	private void DoRewardRuneItemUpdate(Stage_Table stageTable, bool isResetPos)
	{
		for (int i = 0; i < rewardRuneListItems.Count; i++)
		{
			rewardRuneListItems[i].gameObject.SetActive(false);
		}

		var dropItemList = SearchDropItemList(stageTable, true);
		dropItemList.Sort((a, b) => {
			return b.Grade.CompareTo(a.Grade);
		});

		int loadItemCount = dropItemList.Count - rewardRuneListItems.Count;
		var templeList = ZNet.Data.Me.CurCharData.TempleInfo.GetStage(stageTable.StageID);

		for (int i = 0; i < loadItemCount; i++)
		{
			var obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UITowerRewardRuneListItem)).GetComponent<UITowerRewardRuneListItem>();
			obj.transform.SetParent(rewardRuneScrollRect.content.transform);
			obj.transform.localScale = Vector3.one;
			obj.transform.localPosition = Vector3.zero;

			rewardRuneListItems.Add(obj);
			obj.gameObject.SetActive(false);
		}

		if (isResetPos == true)
		{
			rewardRuneScrollRect.content.anchoredPosition = Vector2.zero;
			rewardRuneScrollRect.velocity = Vector2.zero;
		}

		for (int i = 0; i < dropItemList.Count; i++)
		{
			var item = rewardRuneListItems[i];
			item.gameObject.SetActive(true);
			item.DoUpdate(dropItemList[i]);
        }
        rewardGroup2.SetActive(dropItemList.Count > 0);
    }

	private List<Item_Table> SearchDropItemList(Stage_Table stageTable, bool checkRuneType)
	{
		List<Item_Table> itemList = new List<Item_Table>();

		var bossTable = DBMonster.Get(stageTable.SummonBossID);
		if (bossTable == null)
		{
			return itemList;
		}

		if (bossTable.DropGroupID == 0)
		{
			return itemList;
		}

		List<MonsterDrop_Table> dropList = new List<MonsterDrop_Table>();
		var it = GameDBManager.Container.MonsterDrop_Table_data.GetEnumerator();
		while (it.MoveNext())
		{
			if (bossTable.DropGroupID == it.Current.Value.DropGroupID)
			{
				dropList.Add(it.Current.Value);
			}
		}

		for (int i = 0; i < dropList.Count; i++)
		{
			if (GameDBManager.Container.Item_Table_data.TryGetValue(dropList[i].DropItemID, out var itemTable))
			{
				if (checkRuneType && itemTable.ItemUseType != E_ItemUseType.Rune)
				{
					continue;
				}
				itemList.Add(itemTable);
			}
		}
		return itemList;
	}

	// 던전 입장 버튼 클릭
	public void OnEnterStage()
	{
		if (ZGameModeManager.Instance.CurrentGameModeType != E_GameModeType.Field)
		{
			UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
			{
				_popup.Open(string.Empty, "현재 진행중인 던전이 초기화됩니다. 입장하시겠습니까?",
					new string[]
					{
					ZUIString.LOCALE_CANCEL_BUTTON,
					ZUIString.LOCALE_OK_BUTTON
					},
					new Action[]
					{
					delegate { _popup.Close(); },
					delegate { EnterDungeon(); _popup.Close(); }
					});
			});
		}
		else
		{
			EnterDungeon();
		}
	}

	private void EnterDungeon()
    {
		var stageTable = DBStage.Get(selectDungeonTid);
		if (stageTable == null)
		{
			ZLog.LogError(ZLogChannel.Default, $"StageTable이 null이다, selectDungeonTid:{selectDungeonTid}");
			return;
		}

		var portalTable = DBPortal.Get(stageTable.DefaultPortal);
		if (portalTable == null)
		{
			ZLog.LogError(ZLogChannel.Default, $"portalTable이 null이다, DefaultPortal:{stageTable.DefaultPortal}");
			return;
		}

		ZLog.Log(ZLogChannel.WebSocket, $"{portalTable.PortalID}");

		AudioManager.Instance.PlaySFX(30004);
		ZGameManager.Instance.TryEnterStage(portalTable.PortalID, false, 0, 0);

		UIManager.Instance.Close<UIFrameDungeon>();
	}
} // class

using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UIBossWarInfo : MonoBehaviour, ITabContents
{
    [SerializeField] private GameObject rewardGroup;
    [SerializeField] private GameObject rewardGroup2;

    [SerializeField] private ZImage BossWarIcon;
	[SerializeField] private ZText BossWarTitle;
	[SerializeField] private ZText BossName;
	[SerializeField] private ZText BossWarDescription;
	[SerializeField] private ZText RewardItemTitle;
	[SerializeField] private ZText RewardRuneTitle;
	[SerializeField] private ZText BossWarRemainTime;
	[SerializeField] private ZText EnterButtonText;
	[SerializeField] private ZText TapName;
	[SerializeField] private ZText TabBossWarRemainTime;
	[SerializeField] private ZImage TabBossWarIcon;
	[SerializeField] private Transform RewardItemParent;
	[SerializeField] private Transform RewardPetEquipParent;
	[SerializeField] private Transform BossWarScheduleParent;
	[SerializeField] private Transform BossWarListParent;
	[SerializeField] private ZButton EnterButton;
	[SerializeField] private UIBossWarSpawnTimePopup BossWarSpawnTimePopup;
	[SerializeField] private UIBossWarListScrollAdapter BossWarListScrollAdapter;
	[SerializeField] private UIBossWarListItem BossWarListItem;
	[SerializeField] private UIBossWarScheduleListScrollAdapter BossWarScheduleListScrollAdapter;
	[SerializeField] private UIBossWarScheduleListItem BossWarScheduleListItem;
	[SerializeField] private ZImage EnterButtonCostIcon;
	[SerializeField] private ZText EnterButtonCost;

	private Stage_Table CurrentBossStage;
	private Monster_Table CurrentBossMonster;

	private List<UITowerRewardListItem> rewardListItem = new List<UITowerRewardListItem>();
	private List<UITowerRewardRuneListItem> rewardRuneListItem = new List<UITowerRewardRuneListItem>();

	private Action loadEvent;
	private bool IsLoadCompleted = false;
	private uint SelectStageId = 0;

    public int Index { get; set; }

    public void Initialize()
    {
        rewardGroup.SetActive(false);
        rewardGroup2.SetActive(false);


        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UITowerRewardListItem), delegate
		{
			ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UITowerRewardRuneListItem), delegate
			{
				IsLoadCompleted = true;
				BossWarSpawnTimePopup.Initialize();
				InitScrollAdapter();
				loadEvent?.Invoke();
				loadEvent = null;
			}, 0, 1, false);
		}, 0, 1, false);
	}

	public void InitScrollAdapter()
	{
		BossWarListScrollAdapter.Parameters.ItemPrefab = BossWarListItem.GetComponent<RectTransform>();
		var prefab = BossWarListScrollAdapter.Parameters.ItemPrefab;
		prefab.SetParent(BossWarListItem.transform.parent);
		prefab.localScale = Vector2.one;
		prefab.localPosition = Vector3.zero;
		prefab.gameObject.SetActive(false);
		BossWarListScrollAdapter.Initialize(ClickBossWarTab);

		BossWarScheduleListScrollAdapter.Parameters.ItemPrefab = BossWarScheduleListItem.GetComponent<RectTransform>();
		var schedulePrefab = BossWarScheduleListScrollAdapter.Parameters.ItemPrefab;
		prefab.SetParent(BossWarScheduleListItem.transform.parent);
		prefab.localScale = Vector2.one;
		prefab.localPosition = Vector3.zero;
		prefab.gameObject.SetActive(false);
		BossWarScheduleListScrollAdapter.Initialize();
	}

    public void Open()
	{
		// 서버보스가 스폰되었을 때 보스전 UI에서 입장 버튼을 활성화해주기 위해 이벤트 연결
		ZGameModeManager.Instance.mEventBossSpawn += RefreshServerBossInfo;

		this.gameObject.SetActive(true);

		InitString();

		if(IsLoadCompleted == false)
		{
			loadEvent = Init;
		}
		else
		{
			Init();
		}
	}

	public void Refresh()
	{

	}

    public void Close()
	{
		ZGameModeManager.Instance.mEventBossSpawn -= RefreshServerBossInfo;

		this.gameObject.SetActive(false);
	}

	private void Init()
	{
		SetBossWarList();
		SetRewardItemList();
		SetRewardPetEquipList();
		SetEnterButtonCost();
	}

	private void InitString()
	{
		RewardItemTitle.text = DBLocale.GetText("BossWar_UI_Reward");
		RewardRuneTitle.text = DBLocale.GetText("BossWar_UI_Reward_PetEquip");
		EnterButtonText.text = DBLocale.GetText("BossWar_UI_EnterBtn");
	}

	private void SetEnterButtonCost()
	{
		if (DBPortal.TryGet(CurrentBossStage.DefaultPortal, out Portal_Table portal))
		{
			Item_Table item = DBItem.GetItem(portal.UseItemID);
			
			if(item != null)
			{
				EnterButtonCostIcon.sprite = ZManagerUIPreset.Instance.GetSprite(item.IconID);
			}

			EnterButtonCost.text = portal.UseItemCount.ToString();
		}
	}

	private void SetBossWarList()
	{
		var BossStageList = DBStage.GetStageList(E_StageType.InterServer);

		BossWarListScrollAdapter.SetNormalizedPosition(1);
		BossWarListScrollAdapter.Refresh(BossStageList);

		ClickBossWarTab(BossStageList[0]);
	}

	private void ClickBossWarTab(Stage_Table stage)
	{
		SelectStageId = stage.StageID;
		CurrentBossStage = stage;
		CurrentBossMonster = DBMonster.Get(stage.SummonBossID);

		UIManager.Instance.Find<UIFrameDungeon>().ShowBossModel(CurrentBossStage);

		BossWarListScrollAdapter.UpdateScrollItem(SelectStageId);
		SetCurrentBossWarDisplay();
	}

	private void SetCurrentBossWarDisplay()
	{
		BossName.text = DBLocale.GetText(CurrentBossMonster.MonsterTextID);
		BossWarTitle.text = DBLocale.GetText(CurrentBossStage.StageTextID);
		BossWarDescription.text = DBLocale.GetText(CurrentBossStage.StageDescID);

		ZWebManager.Instance.WebGame.REQ_GetServerBossInfo(CurrentBossStage.StageID, (recv, recvPacket) =>
		{
			if (recvPacket.OpenBoss.HasValue)
			{
				ZNet.Data.Me.CurCharData.BossWarContainer = new BossWarContainer(recvPacket);
				SetEnterButtonInteractable();
				SetCurrentBossWarScheduleList();
			}
		});
	}

	private void RefreshServerBossInfo()
	{
		ZWebManager.Instance.WebGame.REQ_GetServerBossInfo(CurrentBossStage.StageID, (recv, recvPacket) =>
		{
			if (recvPacket.OpenBoss.HasValue)
			{
				ZNet.Data.Me.CurCharData.BossWarContainer = new BossWarContainer(recvPacket);
				SetEnterButtonInteractable();
			}
		});
	}

	private void SetCurrentBossWarScheduleList()
	{
		BossWarScheduleListScrollAdapter.SetNormalizedPosition(1);
		BossWarScheduleListScrollAdapter.Refresh(ZNet.Data.Me.CurCharData.BossWarContainer.SpawnTimeScheduleList);
		BossWarScheduleListScrollAdapter.SetPosition();
	}

	private void SetEnterButtonInteractable()
	{
		bool isEnterable = false;
		
		if (!ZNet.Data.Me.CurCharData.BossWarContainer.IsKill)
		{
			if (ZNet.Data.Me.CurCharData.BossWarContainer.IsEnterable)
			{
				if (TimeManager.NowSec >= ZNet.Data.Me.CurCharData.BossWarContainer.EnterableStartTsSec && TimeManager.NowSec < ZNet.Data.Me.CurCharData.BossWarContainer.KillableEndTsSec)
				{
					isEnterable = true;
				}
			}
			else
			{
				if (TimeManager.NowSec >= ZNet.Data.Me.CurCharData.BossWarContainer.EnterableStartTsSec && TimeManager.NowSec < ZNet.Data.Me.CurCharData.BossWarContainer.EnterableEndTsSec)
				{
					isEnterable = true;
				}
			}
		}
		
		EnterButton.interactable = isEnterable;
	}

	private void SetRewardItemList()
	{
		for(int i = 0; i < rewardListItem.Count; i++)
		{
			rewardListItem[i].gameObject.SetActive(false);
		}

		var dropItemList = SearchDropItenList(false);

		dropItemList.Sort((a, b) =>
		{
			return b.Grade.CompareTo(a.Grade);
		});

		int loadItemCount = dropItemList.Count - rewardListItem.Count;

		for(int i = 0; i < loadItemCount; i++)
		{
			var obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UITowerRewardListItem)).GetComponent<UITowerRewardListItem>();

			obj.transform.SetParent(RewardItemParent);
			obj.transform.localScale = Vector3.one;
			obj.transform.localPosition = Vector3.zero;
			rewardListItem.Add(obj);
			obj.gameObject.SetActive(false);
		}

		for(int i = 0; i < dropItemList.Count; i++)
		{
			var item = rewardListItem[i];
			item.gameObject.SetActive(true);
			item.DoUpdate(dropItemList[i].ItemID, 1, OnRewardListItemClick);
		}

        rewardGroup.SetActive(dropItemList.Count > 0);
    }

	private void SetRewardPetEquipList()
	{
		for(int i =0; i < rewardRuneListItem.Count; i++)
		{
			rewardRuneListItem[i].gameObject.SetActive(false);
		}

		var dropItemList = SearchDropItenList(true);

		dropItemList.Sort((a, b) =>
		{
			return b.Grade.CompareTo(a.Grade);
		});

		int loadItemCount = dropItemList.Count - rewardRuneListItem.Count;

		for (int i = 0; i < loadItemCount; i++)
		{
			var obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UITowerRewardRuneListItem)).GetComponent<UITowerRewardRuneListItem>();
			obj.transform.SetParent(RewardPetEquipParent);
			obj.transform.localScale = Vector3.one;
			obj.transform.localPosition = Vector3.zero;

			rewardRuneListItem.Add(obj);
			obj.gameObject.SetActive(false);
		}

		for(int i = 0; i < dropItemList.Count; i++)
		{
			var item = rewardRuneListItem[i];
			item.gameObject.SetActive(true);
			item.DoUpdate(dropItemList[i]);
        }
        rewardGroup2.SetActive(dropItemList.Count > 0);
    }

	private List<Item_Table> SearchDropItenList(bool isRuneType)
	{
		List<Item_Table> itemList = new List<Item_Table>();

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
				if(isRuneType && itemTable.ItemUseType != E_ItemUseType.Rune)
				{
					continue;
				}

				itemList.Add(itemTable);
			}
		}

		return itemList;
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

	public void OpenBossWarSpawnTimePopup()
	{
		BossWarSpawnTimePopup.gameObject.SetActive(true);
		BossWarSpawnTimePopup.Init(CurrentBossMonster);
	}

	public void EnterBossStage()
	{
		if (DBPortal.TryGet(CurrentBossStage.DefaultPortal, out Portal_Table portal))
		{
			if (!ZNet.Data.Me.CurCharData.BossWarContainer.IsEnterable)
			{
				if(!ZNet.Data.Me.CurCharData.CheckCountInvenItemUsingMaterial(portal.UseItemID, portal.UseItemCount))
				{
					UIMessagePopup.ShowPopupOk(DBLocale.GetText("Popup_BossWar_Not_Have_Ticket"));
					return;
				}
			}

			ZGameManager.Instance.TryEnterBossWarCamp(portal.PortalID, portal.UseItemID, () => ZNet.Data.Me.CurCharData.BossWarContainer.Location = BossWarContainer.E_Location.Camp);

			UIManager.Instance.Close<UIFrameDungeon>();
		}
		else
		{
			ZLog.LogError(ZLogChannel.Default, $"portalTable이 null이다, DefaultPortal:{CurrentBossStage.DefaultPortal}");
			return;
		}
	}
}

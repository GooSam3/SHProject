using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class UIFrameItemGem : ZUIFrameBase
{
	public enum E_GemUI
	{
		Equip = 0,
		Craft = 1,
	}

	#region UI Variable
	[SerializeField] private ZToggle[] TopTab;

	[SerializeField] private GameObject Equip = null;
	[SerializeField] private GameObject Craft = null;
	[SerializeField] private Text NotHaveEquipmentTxt = null;

	#region Equip
	[SerializeField] private ScrollRect GemEquipGemOptionListScrollView;
	[SerializeField] private Image SelectEquipIcon;
	[SerializeField] private Image SelectEquipIconGrade;
	[SerializeField] private Text SelectEquipSocketCount;
	[SerializeField] private Image SelectEquipSocketBg;

    [SerializeField] private GameObject GemEquipSelectInfoGroup;
    [SerializeField] private ZButton GemEquipSelectAutoEquipButton;
    [SerializeField] private ZButton GemEquipSelectAllUnEquipButton;
    #endregion

    #region Craft
    [SerializeField] private Image GemIcon = null;
	[SerializeField] private Text CraftCount = null;
	public Button CraftBtn = null;

	[SerializeField] private ScrollRect GemCraftListScrollView;
	[SerializeField] private ScrollRect GemCraftMaterialListScrollView;
	[SerializeField] private ScrollRect GemCraftGemOptionListScrollView;
	#endregion
	#endregion

	#region System Variable
	[SerializeField] private E_GemUI CurGemUI = E_GemUI.Equip;
	[SerializeField] private E_MakeTapType CurCraftGemGrade = E_MakeTapType.Make_Gem_2Tier;
	[SerializeField] private byte CurSelectEquipGemIndex = 0;

	[SerializeField] private UIGemEquipListAdapter GemEquipScrollAdapter = null;
	[SerializeField] private UIGemInvenListAdapter GemInvenListAdapter = null;
	[SerializeField] private List<UIGemOptionListItem> EquipOptionList = new List<UIGemOptionListItem>();
	[SerializeField] private GemSlotData[] EquipGemSlotList = new GemSlotData[ZUIConstant.GEM_SLOT_COUNT];

	[SerializeField] private List<UIGemCraftListItem> GemList = new List<UIGemCraftListItem>();
	[SerializeField] private List<UIGemCraftMaterialListItem> MaterialList = new List<UIGemCraftMaterialListItem>();
	[SerializeField] private List<UIGemOptionListItem> OptionList = new List<UIGemOptionListItem>();
	[SerializeField] private uint SelectMakeTid = 0;

	public UIPopupItemInfo InfoPopup = null;
	public override bool IsBackable => true;
	private bool Isinit = false;
	#endregion

	protected override void OnInitialize()
	{
		base.OnInitialize();

        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIGemCraftListItem), delegate
		{
			ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIGemCraftMaterialListItem), delegate
			{
				ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIGemOptionListItem), delegate
				{
					ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIGemEquipListHolder), delegate
					{
						ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIGemInvenListHolder), delegate
						{
							Isinit = true;

							GemEquipScrollAdapter.Initialize();
							GemInvenListAdapter.Initialize();

							TopTab[(int)E_GemUI.Equip].SelectToggleAction((ZToggle _toggle) => {
								OnSelectMenu((int)E_GemUI.Equip);
							});

							OnSelectEquipment(0);
							ClearEquipGemSlot();
						});
					});
				});
			});
		});
	}

	protected override void OnShow(int _LayerOrder)
	{
		base.OnShow(_LayerOrder);

        if (UIManager.Instance.Find(out UIFrameHUD _hud))
			_hud.SetSubHudFrame(E_UIStyle.FullScreen);

		if (!Isinit)
			return;
		
		if (GemEquipScrollAdapter.IsInitialized)
			GemEquipScrollAdapter.SetData();

		if (GemInvenListAdapter.IsInitialized)
			GemInvenListAdapter.SetData();

		if (GemEquipScrollAdapter.Data != null)
			NotHaveEquipmentTxt.gameObject.SetActive(GemEquipScrollAdapter.Data.List.Count == 0);

		TopTab[(int)E_GemUI.Equip].SelectToggleAction((ZToggle _toggle) => {
			OnSelectMenu((int)E_GemUI.Equip);
		});

		OnSelectEquipment(0);
		ClearEquipGemSlot();
	}

	protected override void OnHide()
	{
		base.OnHide();

		if (UIManager.Instance.Find(out UIFrameHUD _hud))
			_hud.SetSubHudFrame();
	}

	protected override void OnRemove()
	{
		base.OnRemove();

		ZPoolManager.Instance.Clear(E_PoolType.UI, nameof(UIGemCraftListItem));
		ZPoolManager.Instance.Clear(E_PoolType.UI, nameof(UIGemCraftMaterialListItem));
		ZPoolManager.Instance.Clear(E_PoolType.UI, nameof(UIGemOptionListItem));
		ZPoolManager.Instance.Clear(E_PoolType.UI, nameof(UIGemEquipListHolder));
		ZPoolManager.Instance.Clear(E_PoolType.UI, nameof(UIGemInvenListHolder));
	}

	public void OnSelectMenu(int _gemUIIdx)
	{
		if (!Isinit)
			return;

		CurGemUI = (E_GemUI)_gemUIIdx;

		Equip.SetActive(CurGemUI == E_GemUI.Equip);
		Craft.SetActive(CurGemUI == E_GemUI.Craft);

		RemoveInfoPopup();

        GemEquipSelectInfoGroup.SetActive(false);
        GemEquipSelectAutoEquipButton.interactable = false;
        GemEquipSelectAllUnEquipButton.interactable = false;

		CraftBtn.interactable = false;

		switch (CurGemUI)
		{
			case E_GemUI.Equip:
				GemEquipScrollAdapter.SetData();
				GemInvenListAdapter.SetData();
				UpdateEquipGemTotalAbility();
				NotHaveEquipmentTxt.gameObject.SetActive(GemEquipScrollAdapter.Data.List.Count == 0);
				break;

			case E_GemUI.Craft:

				ClearCraftData();

				OnSelectGemGrade((int)CurCraftGemGrade);
				break;
		}
	}

	public void RemoveInfoPopup()
	{
		if (InfoPopup != null)
		{
			if (InfoPopup.gameObject != null)
				Destroy(InfoPopup.gameObject);

			InfoPopup = null;
		}
	}

	public void Close()
	{
		UIManager.Instance.Close<UIFrameItemGem>();
	}

	#region Equip
	private void ClearEquipOptionData()
	{
		for (int i = 0; i < EquipOptionList.Count; i++)
			Destroy(EquipOptionList[i].gameObject);

		EquipOptionList.Clear();
	}

	public void OnSelectEquipment(ulong _itemId)
	{
		ClearEquipOptionData();
		RemoveInfoPopup();

		for (int i = 0; i < GemEquipScrollAdapter.Data.List.Count; i++)
			GemEquipScrollAdapter.Data.List[i].isSelect = false;

		var selectItem = GemEquipScrollAdapter.Data.List.Find(item => item.Item.item_id == _itemId);

		if (selectItem != null)
		{
			selectItem.isSelect = true;

			SelectEquipIcon.sprite = UICommon.GetItemIconSprite(selectItem.Item.item_tid);
			SelectEquipIconGrade.sprite = UICommon.GetItemGradeSprite(selectItem.Item.item_tid);
			SelectEquipSocketCount.text = UICommon.GetEquipSocketCount(selectItem.Item.Sockets).ToString();

			var itemSelectData = DBItem.GetItem(selectItem.Item.item_tid);
			var itemData = DBItem.GetGroupList(DBItem.GetGroupId(selectItem.Item.item_tid));

			for (int i = 0; i < selectItem.Item.Sockets.Count; i++)
			{
				if(selectItem.Item.Sockets[i] != 0)
				{
					//var socketData = DBItem.GetItem(selectItem.Item.Sockets[i]);
					EquipGemSlotList[i].ItemTid = selectItem.Item.Sockets[i];
					EquipGemSlotList[i].Icon.sprite = UICommon.GetItemIconSprite(EquipGemSlotList[i].ItemTid);
					EquipGemSlotList[i].Lock.gameObject.SetActive(false);
					EquipGemSlotList[i].Step.text = string.Empty;
				}
				else if (itemSelectData.SocketData.Count < i)
				{
					//EquipGemSlotList[i].Step.text = EquipGemSlotList[i].Step.text != string.Empty ? "+" + selectItem.Item.Sockets[i].ToString() : string.Empty;
					EquipGemSlotList[i].Lock.gameObject.SetActive(true);
				}
				else
					EquipGemSlotList[i].Step.text = string.Empty;

				EquipGemSlotList[i].Icon.gameObject.SetActive(selectItem.Item.Sockets[i] != 0);
				EquipGemSlotList[i].Lock.gameObject.SetActive(itemSelectData.SocketData.Count <= i);
			}

			for (int i = 0; i < itemData.Count; i++)
			{
				if(itemData[i].Step > itemSelectData.Step)
				{
					for (int j = 0; j < itemData[i].SocketData.Count; j++)
					{
						if(EquipGemSlotList[j].Lock.gameObject.activeSelf && EquipGemSlotList[j].Step.text == string.Empty)
							EquipGemSlotList[j].Step.text = "+" + itemData[i].Step.ToString();
					}
				}
			}

            GemEquipSelectInfoGroup.SetActive(true);
            GemEquipSelectAutoEquipButton.interactable = true;
            GemEquipSelectAllUnEquipButton.interactable = true;
        }
        else
        {
            GemEquipSelectInfoGroup.SetActive(false);
            GemEquipSelectAutoEquipButton.interactable = false;
            GemEquipSelectAllUnEquipButton.interactable = false;
        }

		GemEquipScrollAdapter.RefreshData();
		GemInvenListAdapter.SetData();

		SelectEquipIcon.gameObject.SetActive(selectItem != null);
		SelectEquipIconGrade.gameObject.SetActive(selectItem != null);
		SelectEquipSocketCount.gameObject.SetActive(selectItem != null);

		UpdateEquipGemTotalAbility();
	}

	private void ClearEquipGemSlot()
	{
		for(int i = 0; i < EquipGemSlotList.Length; i++)
		{
			EquipGemSlotList[i].Icon.gameObject.SetActive(false);
			EquipGemSlotList[i].Lock.gameObject.SetActive(false);
			EquipGemSlotList[i].Step.text = string.Empty;
		}
	}

	public void OnSelectInvenGem(ulong _itemId)
	{
		for (int i = 0; i < GemInvenListAdapter.Data.List.Count; i++)
			GemInvenListAdapter.Data.List[i].isSelect = false;

		var selectItem = GemInvenListAdapter.Data.List.Find(item => item.Item.item_id == _itemId);

		if (selectItem != null)
			selectItem.isSelect = true;

		GemInvenListAdapter.RefreshData();
	}
	
	private void RefreshEquipment(ulong _itemId)
	{
		GemEquipScrollAdapter.SetData();
		GemInvenListAdapter.SetData();
		OnSelectEquipment(_itemId);
	}

	public void OnEquipGemSlotInfo(int _slotIdx)
	{
		CurSelectEquipGemIndex = Convert.ToByte(_slotIdx);

		if (InfoPopup != null)
		{
			InfoPopup.Initialize(E_ItemPopupType.GemUnEquip, EquipGemSlotList[_slotIdx].ItemTid);
		}
		else
		{
			ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIPopupItemInfo), (_obj) =>
			{
				UIPopupItemInfo obj = _obj.GetComponent<UIPopupItemInfo>();
				
				if (obj != null)
				{
					InfoPopup = obj;
					obj.transform.SetParent(gameObject.transform);
					obj.Initialize(E_ItemPopupType.GemUnEquip, EquipGemSlotList[_slotIdx].ItemTid);
				}
			});
		}
	}

	private ulong GetSelectEquipment()
	{
		for(int i = 0; i < GemEquipScrollAdapter.Data.List.Count; i++)
			if(GemEquipScrollAdapter.Data.List[i].isSelect)
				return GemEquipScrollAdapter.Data.List[i].Item.item_id;

		return 0;
	}

	private void UpdateEquipGemTotalAbility()
	{
		ulong itemid = GetSelectEquipment();

		if(itemid == 0)
			return;

		var itemData = Me.CurCharData.GetItemData(itemid, NetItemType.TYPE_EQUIP);

		if (itemData == null)
			return;

		List<GemEquipAbility> totalAbilityList = new List<GemEquipAbility>();

		for (int i = 0; i < itemData.Sockets.Count; i++)
		{
			var tableData = DBItem.GetItem(itemData.Sockets[i]);

			if (tableData != null)
			{
				var abTable = DBAbilityAction.Get(tableData.AbilityActionID_01);

				if (abTable == null)
					return;

				List<uint> listAbilityActionIds = new List<uint>(); // abilityAction(최대 3개)를 저장할 리스트.
				Dictionary<GameDB.E_AbilityType, System.ValueTuple<float, float>> abilitys = new Dictionary<GameDB.E_AbilityType, System.ValueTuple<float, float>>();   //abilityAction 안에(최대 9개) 있는 abilityID를 담을 딕션.

				// 리스트에 abilityActionId 담기.
				if (tableData.AbilityActionID_01 != 0)
					listAbilityActionIds.Add(tableData.AbilityActionID_01);
				if (tableData.AbilityActionID_02 != 0)
					listAbilityActionIds.Add(tableData.AbilityActionID_02);
				if (tableData.AbilityActionID_03 != 0)
					listAbilityActionIds.Add(tableData.AbilityActionID_03);

				foreach (var abilityActionId in listAbilityActionIds)
				{
					var abilityActionData = DBAbility.GetAction(abilityActionId);
					switch (abilityActionData.AbilityViewType)
					{
						default:
							var enumer = DBAbility.GetAllAbilityData(abilityActionId).GetEnumerator();
							while (enumer.MoveNext())
							{
								if (!abilitys.ContainsKey(enumer.Current.Key))
								{
									abilitys.Add(enumer.Current.Key, enumer.Current.Value);
								}
							}
							break;
					}
				}

				foreach (var ability in abilitys)
				{
					if (!DBAbility.IsParseAbility(ability.Key))
						continue;

					float abilityminValue = (uint)abilitys[ability.Key].Item1;
					float abilitymaxValue = (uint)abilitys[ability.Key].Item2;

					var havAbility = totalAbilityList.Find(item => item.AbilityActionId == DBAbility.GetAbilityName(ability.Key));

					if (havAbility == null)
					{
						totalAbilityList.Add(new GemEquipAbility() { AbilityActionId = DBAbility.GetAbilityName(ability.Key), AbilityMinCount = abilityminValue, AbilityMaxCount = abilitymaxValue });
					}
					else
					{
						havAbility.AbilityMinCount += abilityminValue;
						havAbility.AbilityMaxCount += abilitymaxValue;
					}
				}
			}
		}

		List<string> totalAbility = new List<string>();
		for (int i = 0; i < totalAbilityList.Count; i++)
		{
			string abInfo = totalAbilityList[i].AbilityActionId.ToString();
			abInfo += totalAbilityList[i].AbilityMinCount > 0 ? "," + totalAbilityList[i].AbilityMinCount.ToString() : string.Empty;
			abInfo += totalAbilityList[i].AbilityMaxCount > 0 ? "," + totalAbilityList[i].AbilityMaxCount.ToString() : string.Empty;
			totalAbility.Add(abInfo);
		}

		for (int i = 0; i < totalAbility.Count; i++)
		{
			UIGemOptionListItem obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIGemOptionListItem)).GetComponent<UIGemOptionListItem>();

			if (obj != null)
			{
				string[] option = totalAbility[i].Split(',');
				string value = option.Length == 3 && option[2] != 0.ToString() ? DBLocale.GetText(option[1]) + " ~ " + DBLocale.GetText(option[2]) : DBLocale.GetText(option[1]);
				obj.transform.SetParent(GemEquipGemOptionListScrollView.content, false);
				obj.Initialize(DBLocale.GetText(option[0]), value);
				EquipOptionList.Add(obj);
			}
		}
	}

	public void EquipGem(ZItem _equipGem)
	{
		var item = Me.CurCharData.GetItemData(GetSelectEquipment(), NetItemType.TYPE_EQUIP);
		var gem = Me.CurCharData.GetItemData(_equipGem.item_id, NetItemType.TYPE_ACCOUNT_STACK);

		if (item == null)
        {
			UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
			{
				_popup.Open(ZUIString.WARRING, "장착할 아이템을 선택해주세요.",
				new string[] { ZUIString.LOCALE_OK_BUTTON },
				new Action[] { delegate
					{
						_popup.Close();
					}
				});
			});
			return;
		}
			
		if(gem == null)
			return;
		
		if (_equipGem.item_tid != 0)
		{
			byte emptySlot = 1;

			var tableData = DBItem.GetItem(item.item_tid);

			bool bUseEmptySlot = false;

			bool bAlreadyHasSameType = false;
			GameDB.E_ItemType equipType = DBItem.GetItemType(gem.item_tid);

			//같은 타입의 잼 있는지 체크
			for (int i = 0; i < tableData.SocketData.Count; i++)
			{
				if (item.Sockets.Count > i && item.Sockets[i] != 0)
				{
					if (DBItem.GetItemType(item.Sockets[i]) == equipType)
						bAlreadyHasSameType = true;

					continue;
				}

				if (item.Sockets[i] == 0 && !bUseEmptySlot)
				{
					bUseEmptySlot = true;
					emptySlot = (byte)(i + 1);
				}
			}

			if (bAlreadyHasSameType)
			{
				//UIManager.NoticeMessage(DBLocale.GetLocaleText("Already_Equiped_GemType"));
				UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
					_popup.Open(ZUIString.WARRING, DBLocale.GetText("Already_Equiped_GemType"), new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate {
					_popup.Close(); } });
				});

				return;
			}

			if (bUseEmptySlot)
			{
				ZWebManager.Instance.WebGame.REQ_EquipGemItem(item.item_id, item.item_tid, gem.item_tid, emptySlot, (recv, recvMsg) =>
				{
					RefreshEquipment(item.item_id);
				});
			}
			else
			{
				List<byte> slotids = new List<byte>();
				slotids.Add(emptySlot);
				ZWebManager.Instance.WebGame.REQ_UnEquipGemItem(item.item_id, item.item_tid, slotids, (recv, recvMsg) => {
					ZWebManager.Instance.WebGame.REQ_EquipGemItem(item.item_id, item.item_tid, gem.item_tid, emptySlot, (recv2, recvMsg2) =>
					{
						RefreshEquipment(item.item_id);
					});
				});
			}
		}
		else
		{
			List<byte> slotids = new List<byte>();
			slotids.Add(CurSelectEquipGemIndex);
			ZWebManager.Instance.WebGame.REQ_UnEquipGemItem(item.item_id, item.item_tid, slotids, (recv, recvMsg) => {
				RefreshEquipment(item.item_id);
			});
		}
	}

	public void OnAutoEquipGem(bool _firstCheck)
	{
		var item = Me.CurCharData.GetItemData(GetSelectEquipment(), NetItemType.TYPE_EQUIP);

		if (item != null)
		{
			List<byte> findEmptyslotids = new List<byte>();
			List<GameDB.E_ItemType> EquipedGemType = new List<GameDB.E_ItemType>();

			var tableData = DBItem.GetItem(item.item_tid);

			byte ableGrade = 0;

			for (int i = 0; i < item.Sockets.Count; i++)
			{
				//쓸수 없는 소켓
				if (tableData.SocketData.Count <= i)
					continue;

				ableGrade = (byte)tableData.SocketData[i];

				if (item.Sockets[i] == 0)
					findEmptyslotids.Add((byte)(i + 1));
				else
					EquipedGemType.Add(DBItem.GetItemType(item.Sockets[i]));
			}

			if (findEmptyslotids.Count <= 0)
			{
				if(_firstCheck)
					UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
						_popup.Open(ZUIString.WARRING, DBLocale.GetText("EmptyEquipGemSlot"), new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate {
						_popup.Close(); } });
					});
					return;
			}

			List<ZItem> EquipAbleGems = new List<ZItem>();
			foreach (var gem in GemInvenListAdapter.Data.List)
			{
				if (EquipedGemType.Contains(DBItem.GetItemType(gem.Item.item_tid)))
					continue;

				if (DBItem.GetItemGrade(gem.Item.item_tid) > ableGrade)
					continue;

				EquipAbleGems.Add(gem.Item);
				EquipedGemType.Add(DBItem.GetItemType(gem.Item.item_tid));
			}

			EquipAbleGems.Sort((x, y) => {
				if (DBItem.GetItemGrade(x.item_tid) > DBItem.GetItemGrade(y.item_tid)) return -1;
				else if (DBItem.GetItemGrade(x.item_tid) < DBItem.GetItemGrade(y.item_tid)) return 1;

				if (DBItem.GetEnchantStep(x.item_tid) > DBItem.GetEnchantStep(y.item_tid)) return -1;
				else if (DBItem.GetEnchantStep(x.item_tid) < DBItem.GetEnchantStep(y.item_tid)) return 1;

				return 0;
			});

			if (EquipAbleGems.Count > 0 && findEmptyslotids.Count > 0)
			{
				ZWebManager.Instance.WebGame.REQ_EquipGemItem(item.item_id, item.item_tid, EquipAbleGems[0].item_tid, findEmptyslotids[0], (recv, recvMsg) => {

					EquipAbleGems.RemoveAt(0);
					findEmptyslotids.RemoveAt(0);

					OnAutoEquipGem(false);

					RefreshEquipment(item.item_id);
				});
			}
		}
	}

	public void OnUnEquipAllGem()
	{
		UnEquipGem(null, true);
	}

	public void UnEquipGem(ZItem _unEquipGem, bool allUnEquip = false)
	{
		var unEquipItem = Me.CurCharData.GetItemData(GetSelectEquipment(), NetItemType.TYPE_EQUIP);

		if (unEquipItem == null)
			return;

		List<byte> unEquipGemList = new List<byte>();
		bool isUnequip = false;
		for (int i = 0; i < unEquipItem.Sockets.Count; i++)
		{
			if (!allUnEquip)
			{
				if (unEquipItem.Sockets[i] == _unEquipGem.item_tid)
				{
					unEquipGemList.Add(Convert.ToByte(i + 1));
					isUnequip = true;
					break;
				}
			}
			else
			{
				if(unEquipItem.Sockets[i] != 0)
				{
					isUnequip = true;
					unEquipGemList.Add(Convert.ToByte(i + 1));
				}	
			}
		}

		if (!isUnequip)
		{
			UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
				_popup.Open(ZUIString.WARRING, "장착한 마석이 존재하지 않습니다.", new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate {
					_popup.Close(); } });
			});
			return;
		}

		ZWebManager.Instance.WebGame.REQ_UnEquipGemItem(unEquipItem.item_id, unEquipItem.item_tid, unEquipGemList, (recvPacket, msg) => {
			RefreshEquipment(unEquipItem.item_id);
		});
	}
	#endregion

	#region Craft
	public void SelectCraftGem(Make_Table _table)
	{
		if (_table == null)
			return;

		CraftCount.text = 1.ToString();

		ClearCraftMaterialListData();

		SelectMakeTid = _table.MakeID;

		GemIcon.gameObject.SetActive(_table.SuccessGetItemID != 0);
		GemIcon.sprite = UICommon.GetItemIconSprite(_table.SuccessGetItemID);

		CraftBtn.interactable = true;

		var abData = DBAbilityAction.Get(DBItem.GetItem(_table.SuccessGetItemID).AbilityActionID_01);
		List<string> ab = new List<string>();
		if (abData.AbilityID_01 != 0) ab.Add(abData.AbilityID_01.ToString() + "," + abData.AbilityPoint_01_Min.ToString() + "," + abData.AbilityPoint_01_Max.ToString());
		if (abData.AbilityID_02 != 0) ab.Add(abData.AbilityID_02.ToString() + "," + abData.AbilityPoint_02.ToString());
		if (abData.AbilityID_03 != 0) ab.Add(abData.AbilityID_03.ToString() + "," + abData.AbilityPoint_03.ToString());
		if (abData.AbilityID_04 != 0) ab.Add(abData.AbilityID_04.ToString() + "," + abData.AbilityPoint_04.ToString());
		if (abData.AbilityID_05 != 0) ab.Add(abData.AbilityID_05.ToString() + "," + abData.AbilityPoint_05.ToString());
		if (abData.AbilityID_06 != 0) ab.Add(abData.AbilityID_06.ToString() + "," + abData.AbilityPoint_06.ToString());
		if (abData.AbilityID_07 != 0) ab.Add(abData.AbilityID_07.ToString() + "," + abData.AbilityPoint_07.ToString());
		if (abData.AbilityID_08 != 0) ab.Add(abData.AbilityID_08.ToString() + "," + abData.AbilityPoint_08.ToString());
		if (abData.AbilityID_09 != 0) ab.Add(abData.AbilityID_09.ToString() + "," + abData.AbilityPoint_09.ToString());
		
		for(int i = 0; i < ab.Count; i++)
		{
			UIGemOptionListItem obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIGemOptionListItem)).GetComponent<UIGemOptionListItem>();

			if (obj != null)
			{
				string[] option = ab[i].Split(',');
				string value = option.Length == 3 && option[2] != 0.ToString() ? DBLocale.GetText(option[1]) + " ~ " + DBLocale.GetText(option[2]) : DBLocale.GetText(option[1]);
				obj.transform.SetParent(GemCraftGemOptionListScrollView.content, false);
				obj.Initialize(DBLocale.GetText(option[0]), value);
				OptionList.Add(obj);
			}
		}

		var material = DBMake.GetMakeData(_table.MakeID);

		if(material != null)
		{
			if (material.MaterialItemID_01.Count != 0) createMaterial(material.MaterialItemID_01[0], material.MaterialItemCount_01[0]);
			if (material.MaterialItemID_02.Count != 0) createMaterial(material.MaterialItemID_02[0], material.MaterialItemCount_02[0]);
			if (material.MaterialItemID_03.Count != 0) createMaterial(material.MaterialItemID_03[0], material.MaterialItemCount_03[0]);
			if (material.MaterialItemID_04.Count != 0) createMaterial(material.MaterialItemID_04[0], material.MaterialItemCount_04[0]);
			if (material.MaterialItemID_05.Count != 0) createMaterial(material.MaterialItemID_05[0], material.MaterialItemCount_05[0]);
		}

		void createMaterial(uint _makeItemId, uint _makeCnt)
		{
			UIGemCraftMaterialListItem obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIGemCraftMaterialListItem)).GetComponent<UIGemCraftMaterialListItem>();

			if (obj != null)
			{
				obj.transform.SetParent(GemCraftMaterialListScrollView.content, false);
				obj.Initialize(DBItem.GetItem(_makeItemId), _makeCnt);
				MaterialList.Add(obj);
			}
		}
	}

	private void ClearCraftData()
	{
		SelectMakeTid = 0;

		CraftCount.text = 1.ToString();

		ClearCraftGemListData();
		ClearCraftMaterialListData();
		ClearOptionListData();
	}

	private void ClearCraftMaterialListData()
	{
		for (int i = 0; i < MaterialList.Count; i++)
			Destroy(MaterialList[i].gameObject);

		MaterialList.Clear();
	}

	private void ClearCraftGemListData()
	{
		for (int i = 0; i < GemList.Count; i++)
			Destroy(GemList[i].gameObject);

		GemList.Clear();
	}

	private void ClearOptionListData()
	{
		for (int i = 0; i < OptionList.Count; i++)
			Destroy(OptionList[i].gameObject);

		OptionList.Clear();
	}

	private bool CheckCost()
    {
		foreach(var iter in MaterialList)
        {
			if (ConditionHelper.CheckCompareCost(iter.Item.ItemID, iter.ItemCount) == false)
				return false;
        }
		return true;
    }

	private ulong CheckMakeMaxCount()
	{
		ulong maxCnt = 0;
		
		for(int i = 0; i < MaterialList.Count; i++)
		{
			var data = Me.CurCharData.InvenList.Find(item => item.item_tid == MaterialList[i].Item.ItemID);

			if (data != null)
			{
				ulong val = data.cnt / MaterialList[i].ItemCount;

				if (val == 0)
					return 0;

				if (maxCnt == 0)
					maxCnt = val;
				else
					maxCnt = val < maxCnt ? val : maxCnt;
			}
			else
				return 0;
		}

		return maxCnt;
	}

	public void OnSelectGemGrade(int gradeIdx)
	{
		CurCraftGemGrade = (E_MakeTapType)gradeIdx;
		GemIcon.gameObject.SetActive(false);

		ClearCraftData();

		var data = DBMake.GetMakeTypeDatas(E_MakeType.Gem, CurCraftGemGrade);

		if (data == null)
			return;

		for (int i = 0; i < data.Count; i++)
		{
			UIGemCraftListItem obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIGemCraftListItem)).GetComponent<UIGemCraftListItem>();

			if (obj != null)
			{
				obj.transform.SetParent(GemCraftListScrollView.content, false);
				obj.Initialize(data[i]);
				GemList.Add(obj);
			}
		}
	}

	public void OnChangeCraftCount(int _cnt)
	{
		int craftCnt = Convert.ToInt32(CraftCount.text);

		if (_cnt > 0 && CheckMakeMaxCount() < Convert.ToUInt64(craftCnt + _cnt))
		{
			UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
				_popup.Open(ZUIString.WARRING, DBLocale.GetText("Jam_Shortage_Materials"), new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate {
					_popup.Close(); } });
			});
			return;
		}

		if (_cnt < 0 && craftCnt == 1)
			return;

		DrawMaterialCount(Convert.ToUInt64(_cnt < 0 && craftCnt == 0 ? 0 : craftCnt + _cnt));
		CraftCount.text = _cnt < 0 && craftCnt == 0 ? 0.ToString() : (craftCnt + _cnt).ToString();
	}

	public void OnChangeCraftCountTen()
	{
		ulong craftCnt = Convert.ToUInt64(CraftCount.text);
		ulong checkMaterialCnt = CheckMakeMaxCount();
		ulong craftValue = 0;

		if (checkMaterialCnt == 0)
		{
			UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
				_popup.Open(ZUIString.WARRING, DBLocale.GetText("Jam_Shortage_Materials"), new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate {
					_popup.Close(); } });
			});
			return;
		}
		else if (checkMaterialCnt < craftCnt + 10)
		{
			UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
				_popup.Open(ZUIString.WARRING, DBLocale.GetText("Jam_Shortage_Materials"), new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate {
					_popup.Close(); } });
			});
			return;
		}

		craftValue = craftCnt + 10;

		DrawMaterialCount(craftValue);
		CraftCount.text = craftValue.ToString(); 
	}

	public void OnChangeCraftCountMax()
	{
		ulong craftCnt = Convert.ToUInt64(CraftCount.text);
		ulong checkMaterialCnt = CheckMakeMaxCount();

		if (checkMaterialCnt == 0)
		{
			UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
				_popup.Open(ZUIString.WARRING, DBLocale.GetText("Jam_Shortage_Materials"), new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate {
					_popup.Close(); } });
			});
			return;
		}
		else if (checkMaterialCnt < craftCnt + 1)
		{
			UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
				_popup.Open(ZUIString.WARRING, DBLocale.GetText("Jam_Shortage_Materials"), new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate {
					_popup.Close(); } });
			});
			return;
		}

		DrawMaterialCount(CheckMakeMaxCount());
		CraftCount.text = CheckMakeMaxCount().ToString();
	}

	public void OnResetCraftCount()
	{
		DrawMaterialCount(1);
		CraftCount.text = 1.ToString();
	}

	public void OnStartCraft()
	{
		if (SelectMakeTid == 0 || Convert.ToUInt32(CraftCount) == 0)
			return;

		if (CheckCost() == false)
			return;

		if (CheckMakeMaxCount() == 0)
		{
			UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
				_popup.Open(ZUIString.WARRING, DBLocale.GetText("Jam_Shortage_Materials"), new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate {
					_popup.Close(); } });
			});
			return;
		}
			
		List<List<ZItem>> matItems = new List<List<ZItem>>();

		for (int i = 0; i < MaterialList.Count; i++)
		{
			matItems.Add(new List<ZItem>(Me.CurCharData.GetAllInvenItemsUsingMaterial(MaterialList[i].Item.ItemID, MaterialList[i].ItemCount)));
		}

		ZWebManager.Instance.WebGame.REQ_MakeItem(SelectMakeTid, Convert.ToUInt32(CraftCount.text), matItems, (recvPacket, msg) => {
			
			if(recvPacket.ErrCode == WebNet.ERROR.NO_ERROR)
			{
				uint suc = 0;
				uint fail = 0;
				for (int i = 0; i < msg.ResultMakeLength; i++)
				{
					if (msg.ResultMake(i).Value.Result)
						suc++;
					else
						fail++;
				}

				UICommon.OpenSystemPopup_One(DBLocale.GetText("제작 결과"),
					"제작 성공 :" + suc.ToString() + "\n제작 실패 : " + fail.ToString(), ZUIString.LOCALE_OK_BUTTON);

				OnSelectGemGrade((int)CurCraftGemGrade);
				OnResetCraftCount();
				CraftBtn.interactable = false;
			}
		});
	}

	private void DrawMaterialCount(ulong _makeCnt)
	{
		for (int i = 0; i < MaterialList.Count; i++)
			MaterialList[i].SetCountText(_makeCnt);
	}
	#endregion
}

[Serializable]
public class GemSlotData
{
	public uint ItemTid;
	public GameObject Obj;
	public Text Step;
	public Image Icon;
	public Image IconBoard;
	public Image Lock;
	public RawImage EffectLine;
	public RawImage EffectGlow;
}

[Serializable]
public class GemEquipAbility
{
	public string AbilityActionId;
	public float AbilityMinCount;
	public float AbilityMaxCount;
}
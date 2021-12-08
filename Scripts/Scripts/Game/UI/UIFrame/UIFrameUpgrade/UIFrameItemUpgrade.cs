using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class UIFrameItemUpgrade : ZUIFrameBase
{
	#region UI Variable
	/// <summary>TargetSlot : 승급 아이템, MaterialSlot : 재료 아이템</summary>
	[SerializeField] private Image TargetSlot, MaterialSlot;
	/// <summary>MaterialDescText : 승급 아이템 텍스트, EnchantInfoText : 승급 정보 텍스트, EnchantCostText : 승급 비용 텍스트</summary>
	[SerializeField] private Text ItemText, TierUpInfoText, TierUpCostText;
	[SerializeField] private Text MatarialSlotName, MatarialSlotCost;
	/// <summary>업그레이드 아이탬 선택 팝업</summary>
	[SerializeField] private GameObject UpgradeSelectPopup;

	[SerializeField] private Button UpGradeBtn;

	[SerializeField] private List<UIItemUpgradeSelectSlot> UpgradeSelectSlot = new List<UIItemUpgradeSelectSlot>();
	[SerializeField] private List<UIInfoTextSlot> UIItemInfoTextSlotList = new List<UIInfoTextSlot>();
	[SerializeField] private List<UIInfoTextSlot> UIItemEffectTextSlotList = new List<UIInfoTextSlot>();
	[SerializeField] private GameObject Ability;
	[SerializeField] private Image AbilityImage;
	[SerializeField] private Text AbilityText, AbilityValue;
	[SerializeField] private GameObject ClassGameObject;
	[SerializeField] private Text ClassText;
	[SerializeField] private GameObject ItemInfo;
	[SerializeField] private Image GradeBoard;
	[SerializeField] private Text GradeText;
	[SerializeField] private ScrollRect UpgradeSelectScrollRect;
	#endregion

	#region System Variable
	[SerializeField] private ZItem Material = null;
	public ZItem Item { get; private set; } = null;

	private uint SelectItemTid;
	private uint UpgadeListTid;
	private List<uint> ItemTidList = new List<uint>();
	private Dictionary<E_CharacterType, List<uint>> UpgradeDic = new Dictionary<E_CharacterType, List<uint>>();
	private List<UICommon.ItemDescInfo> ItemDetailInfoList = new List<UICommon.ItemDescInfo>();

	public override bool IsBackable => true;
	#endregion

	protected override void OnHide()
	{
		base.OnHide();

		Item = null;
		Material = null;
		TargetSlot.sprite = null;
		MaterialSlot.sprite = null;
		GradeBoard.sprite = null;
		GradeText.text = string.Empty;
		TierUpCostText.text = "0";
		SelectItemTid = 0;
		UpgadeListTid = 0;
		ItemTidList.Clear();
		UpgradeDic.Clear();
		ItemDetailInfoList.Clear();
		ItemInfo.SetActive(false);
		MatarialSlotName.text = string.Empty;
		MatarialSlotCost.text = string.Empty;
		UpgradeSelectPopup.SetActive(false);
		UpGradeBtn.interactable = false;

		if (UIManager.Instance.Find(out UIFrameInventory _inventory))
			_inventory.ShowInvenSort((int)E_InvenSortType.All);
	}

	public void Set(ZItem _item)
	{
		if (_item == null)
			return;

		Item = _item;
		ItemText.text = DBLocale.GetItemLocale(DBItem.GetItem(Item.item_tid));
		TargetSlot.sprite = UICommon.GetItemIconSprite(Item.item_tid);
		GradeBoard.sprite = UICommon.GetItemGradeSprite(Item.item_tid);
		UpGradeBtn.interactable = false;

		bool isEnchanted = DBItem.GetItem(Item.item_tid).Step > 1;
		GradeText.gameObject.SetActive(isEnchanted);
		if (isEnchanted)
			GradeText.text = string.Format("+{0}", DBItem.GetItem(Item.item_tid).Step);

		var tableenchant = DBItem.GetEnchantData(Item.item_tid);

		//재료가 1:1 대응임(기획 확인)
		var material = tableenchant.UpgradeUseItemID.Count == 0 ? null : Me.CurCharData.GetItem(tableenchant.UpgradeUseItemID[0]);
		if (tableenchant.UpgradeUseItemID.Count != 0 || tableenchant.UpgradeUseGoldCount != 0)
		{
			if (tableenchant.UpgradeUseItemCount != 0)
			{
				Material = material;
				MaterialSlot.sprite = UICommon.GetItemIconSprite(tableenchant.UpgradeUseItemID[0]);

				ItemEnchant_Table enchantData = DBItem.GetEnchantData(Item.item_tid);
				ulong CostCount = enchantData.UpgradeUseItemCount;

				MatarialSlotName.text = String.Format("{0}", DBLocale.GetText(DBItem.GetItem(Material.item_tid).ItemTextID));
				MatarialSlotCost.text = String.Format("{0}/{1}", Material.cnt, CostCount);

				MatarialSlotCost.color = Material.cnt < CostCount ? new Color(255,0,0) : new Color(255,255,255);
				UpGradeBtn.interactable = Material.cnt >= CostCount;
			}
			else
			{
				Material = null;
				MaterialSlot.sprite = null;
				MatarialSlotName.text = string.Empty;
				MatarialSlotCost.text = string.Empty;
			}
		}
		else
		{
			MaterialSlot.sprite = null;
			MatarialSlotName.text = string.Empty;
			MatarialSlotCost.text = string.Empty;
		}

		TargetSlot.gameObject.SetActive(Item != null);
		MaterialSlot.gameObject.SetActive(Material != null);
		if(UpGradeBtn.interactable == true)
			UpGradeBtn.interactable = tableenchant.UpgradeUseGoldCount <= ZNet.Data.Me.GetCurrency(DBConfig.Gold_ID);
		TierUpCostText.text = string.Format("{0}", tableenchant.UpgradeUseGoldCount);

		// 승급창이 뜨면 인벤토리는 열려있을 경우 닫아주면 될 듯
		UIManager.Instance.Find<UIFrameInventory>().Close();
	}

	/// <summary>승급 아이템 선택 버튼 콜백 함수</summary>
	public void StartTierUp()
	{
		ItemTidList.Clear();
		ItemInfo.SetActive(false);

		if (UpgradeSelectPopup.activeSelf)
			UpgradeSelectPopup.SetActive(false);
		else
		{
			UpgradeSelectPopup.SetActive(true);
			UpgradeSelectPopupSetting();
		}
	}

	private void UpgradeSelectPopupSetting()
	{
		InitUpdateItem();
		UpdateSlotSetting();

		UpgradeSelectScrollRect.verticalNormalizedPosition = 1;

		Ability.gameObject.SetActive(false);
		ClassGameObject.gameObject.SetActive(false);
		for (int i = 0; i < UIItemInfoTextSlotList.Count; i++)
		{
			UIItemInfoTextSlotList[i].gameObject.SetActive(false);
		}
		for (int i = 0; i < UIItemEffectTextSlotList.Count; i++)
		{
			UIItemEffectTextSlotList[i].gameObject.SetActive(false);
		}
	}

	/// <summary>승급 장비 slot 셋팅</summary>
	private void UpdateSlotSetting()
	{
		// to do : 초기 개발자가 구조를 잘못 짜놓음..
		// 나중에 새로 개발해야함

		for (int i = 0; i < UpgradeSelectSlot.Count; i++)
		{
			UpgradeSelectSlot[i].gameObject.SetActive(false);
		}

		ItemTidList.AddRange(UpgradeDic[E_CharacterType.Archer]);
		ItemTidList.AddRange(UpgradeDic[E_CharacterType.Assassin]);
		ItemTidList.AddRange(UpgradeDic[E_CharacterType.Knight]);
		ItemTidList.AddRange(UpgradeDic[E_CharacterType.Wizard]);

		if (Item == null)
			return;

		var itemType = DBItem.GetItemType(Item.item_tid);
		if (itemType == E_ItemType.Artifact || itemType == E_ItemType.Earring || itemType == E_ItemType.Necklace ||
			itemType == E_ItemType.Bracelet || itemType == E_ItemType.Ring)
		{
			var itne = DBUpgrade.GetUpgradeList(DBItem.GetEnchantData(Item.item_tid).UpgradeGroupID);

			Debug.Log(itne);
			for (int i = 0; i < itne.Count; i++)
            {
				UpgradeSelectSlot[i].Set(ItemTidList[i]);
				UpgradeSelectSlot[i].gameObject.SetActive(true);
			}
		}
		else
		{
			for (int i = 0; i < ItemTidList.Count; i++)
			{
				UpgradeSelectSlot[i].Set(ItemTidList[i]);
				UpgradeSelectSlot[i].gameObject.SetActive(true);
			}
		}
	}

	/// <summary>승급 장비들 셋팅 함수</summary>
	public void InitUpdateItem()
	{
		UpgradeDic.Clear();

		if (Item != null)
		{
			var tableData = DBItem.GetItem(Item.item_tid);
			uint UpgradeGroupId = DBItem.GetUpgradeGroupId(Item.item_tid);

			foreach (UpgradeList_Table tabledata in DBUpgrade.GetUpgradeList(UpgradeGroupId))
			{
				E_CharacterType charType = DBItem.GetUseCharacterType(tabledata.UpgradeItemID);
				E_ItemType itemType = DBItem.GetItemType(tabledata.UpgradeItemID);


				foreach (var e_charType in EnumHelper.Values<E_CharacterType>())
				{
					if (e_charType == E_CharacterType.None)
						continue;

					if (EnumHelper.CheckFlag(charType, e_charType))
					{
						if (!UpgradeDic.ContainsKey(e_charType))
							UpgradeDic.Add(e_charType, new List<uint>());

						UpgradeDic[e_charType].Add(tabledata.UpgradeListID);
					}
				}
			}
		}
		else
		{
			ItemTidList.Clear();
		}
	}

	//private void SetItemDesc()
	//{
	//	ItemDetailInfoList.Clear();

	//	var tableData = DBItem.GetItem(SelectItemTid);

	//	List<uint> listAbilityActionIds = new List<uint>();
	//	Dictionary<GameDB.E_AbilityType, System.ValueTuple<float, float>> abilitys = new Dictionary<GameDB.E_AbilityType, System.ValueTuple<float, float>>();

	//	if (tableData.AbilityActionID_01 != 0)
	//		listAbilityActionIds.Add(tableData.AbilityActionID_01);
	//	if (tableData.AbilityActionID_02 != 0)
	//		listAbilityActionIds.Add(tableData.AbilityActionID_02);
	//	if (tableData.AbilityActionID_03 != 0)
	//		listAbilityActionIds.Add(tableData.AbilityActionID_03);

	//	foreach (var abilityActionId in listAbilityActionIds)
	//	{
	//		var abilityActionData = DBAbility.GetAction(abilityActionId);
	//		switch (abilityActionData.AbilityViewType)
	//		{
	//			case GameDB.E_AbilityViewType.ToolTip:
	//				var itemDescInfo = new ItemDescInfo();
	//				itemDescInfo.strDesc = string.Format("{0}{1}", "", DBLocale.ParseAbilityTooltip(abilityActionData, "2", "1"));
	//				itemDescInfo.Value = null;

	//				ItemDetailInfoList.Add(itemDescInfo);
	//				break;
	//			case GameDB.E_AbilityViewType.Not:
	//			default:
	//				var enumer = DBAbility.GetAllAbilityData(abilityActionId).GetEnumerator();
	//				while (enumer.MoveNext())
	//				{
	//					if (!abilitys.ContainsKey(enumer.Current.Key))
	//					{
	//						abilitys.Add(enumer.Current.Key, enumer.Current.Value);
	//					}
	//				}
	//				break;
	//		}
	//	}

	//	foreach (var ability in abilitys)
	//	{
	//		if (!DBAbility.IsParseAbility(ability.Key))
	//			continue;

	//		float abilityminValue = (uint)abilitys[ability.Key].Item1;
	//		float abilitymaxValue = (uint)abilitys[ability.Key].Item2;

	//		var itemDescInfo = new ItemDescInfo();

	//		itemDescInfo.strDesc = DBLocale.GetText(DBAbility.GetAbilityName(ability.Key));
	//		var newValue = DBAbility.ParseAbilityValue(ability.Key, abilityminValue, abilitymaxValue);
	//		itemDescInfo.Value = string.Format("{0}", newValue);

	//		ItemDetailInfoList.Add(itemDescInfo);
	//	}
	//}

	private void UpdateItemInfoSetting()
	{
		for (int i = 0; i < UIItemInfoTextSlotList.Count; i++)
			UIItemInfoTextSlotList[i].gameObject.SetActive(false);
		for (int i = 0; i < UIItemEffectTextSlotList.Count; i++)
			UIItemEffectTextSlotList[i].gameObject.SetActive(false);

		if (ItemDetailInfoList.Count > 0)
		{
			for (int i = 0; i < ItemDetailInfoList.Count; i++)
			{
				if (ItemDetailInfoList[i].Value != null)
				{
					UIItemInfoTextSlotList[i].Initialize("", ItemDetailInfoList[i].Value, ItemDetailInfoList[i].strDesc);
				}
				else
				{
					UIItemEffectTextSlotList[i].Initialize("", "", ItemDetailInfoList[i].strDesc);
				}
			}
		}

		for (int i = 0; i < UIItemInfoTextSlotList.Count; i++)
		{
			if (UIItemInfoTextSlotList[i].GetSlotActiveCheck())
			{
				UIItemInfoTextSlotList[i].gameObject.SetActive(true);

			}
			UIItemInfoTextSlotList[i].SetSlotActiveCheck();
		}
		for (int i = 0; i < UIItemEffectTextSlotList.Count; i++)
		{
			if (UIItemEffectTextSlotList[i].GetSlotActiveCheck())
			{
				UIItemEffectTextSlotList[i].gameObject.SetActive(true);
			}
			UIItemEffectTextSlotList[i].SetSlotActiveCheck();
		}

		DBItem.GetItem(SelectItemTid, out Item_Table table);

		Ability.SetActive(table.AbilityActionID_01 != 0);
		if (table.AbilityActionID_01 != 0)
		{
			AbilityAction_Table abilityAction = DBAbilityAction.Get(table.AbilityActionID_01);

			if (abilityAction != null)
			{
				Ability_Table ability = DBAbility.GetAbility(abilityAction.AbilityID_01);

				if (ability != null && ability.AbilityIcon != null)
				{
					AbilityImage.sprite = ZManagerUIPreset.Instance.GetSprite(ability.AbilityIcon);
					AbilityText.text = DBLocale.GetText(ability.StringName);
					AbilityValue.text = DBAbilityAction.Get(table.AbilityActionID_01).AbilityPoint_01_Min.ToString();
				}
				else
					Ability.SetActive(false);
			}
		}
		ClassGameObject.SetActive(true);
		ClassText.text = table.UseCharacterType.ToString();
	}

	public void OnSelectItem(int _idx)
	{
		ItemInfo.SetActive(true);

		for (int i = 0; i < UpgradeSelectSlot.Count; i++)
		{
			UpgradeSelectSlot[i].SelectObj.SetActive(false);
		}

		UpgradeSelectSlot[_idx].SelectObj.SetActive(true);
		SelectItemTid = DBUpgrade.GetItemTid(UpgradeSelectSlot[_idx].UpgadeListTid);
		UpgadeListTid = UpgradeSelectSlot[_idx].UpgadeListTid;

		//SetItemDesc();
		ItemDetailInfoList = UICommon.GetItemDesc(SelectItemTid);

		UpdateItemInfoSetting();
	}

	/// <summary>승급 장비 선택 버튼 콜백.</summary>
	public void OnUpgradeItemSelect()
	{
		ulong materialID = Material == null ? 0 : Material.item_id;
		uint materialTID = Material == null ? 0 : Material.item_tid;
		if (UpgadeListTid != 0)
		{
			ZWebManager.Instance.WebGame.REQ_UpgradeItem(Item.item_id, Item.item_tid, Item.slot_idx, materialID, materialTID, UpgadeListTid, (recvPacket, rechPacketMsg) =>
			{
				if(rechPacketMsg.UpgradeItem.HasValue)
				{
					uint targetTid = rechPacketMsg.UpgradeItem.Value.ToItemTid;

					if(DBItem.GetItem(targetTid, out var table))
					{
						UIManager.Instance.Open<UIPopupEquipPromotionResult>((str, popup) =>
						{
							popup.PlayFX(table);
						});
					}
				}

				UIManager.Instance.Find<UIFrameInventory>().ScrollAdapter.SetData();
				Close();
			});
		}
	}

	public void Close()
	{
		UpgradeSelectPopup.SetActive(false);
		UIManager.Instance.Close<UIFrameItemUpgrade>();
	}
}

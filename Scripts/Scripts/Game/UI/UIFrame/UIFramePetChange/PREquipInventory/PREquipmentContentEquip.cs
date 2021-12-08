using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZDefine;
using ZNet.Data;
using UnityEngine.UI;
using DG.Tweening;

public class PREquipmentContentEquip : PREquipContentBase
{
	private enum E_DetailFilterType
	{
		None = 0,
		SetType = 1,
		MainOption = 2,
		SubOption = 3
	}

	[SerializeField] private PREquipInfo equipInfoOrigin;
	[SerializeField] private PREquipInfo equipInfoTemp;

	[SerializeField] private UIAbilityListAdapter osaAbility;
	[SerializeField] private OSA_PREquipFilterAdapter osaFilter;

	[SerializeField] private GameObject objDetailFilter;
	[SerializeField] private GameObject objFilterSelect;

	[SerializeField] private ZButton btnReset;

	[SerializeField] private Text txtName;

	private List<UIAbilityData> originAbilityData = new List<UIAbilityData>();

	private Dictionary<E_EquipSlotType, PetRuneData> dicTempEquip = new Dictionary<E_EquipSlotType, PetRuneData>();

	private PREquipItemData tempSlot;

	[SerializeField] private Text txtEquipCost;
	[SerializeField] private ZButton btnConfirm;

	[SerializeField] private ZUIButtonRadio toggleSortByStep;
	[SerializeField] private ZUIButtonRadio toggleSortByGrade;

	[SerializeField] Text txtSetFilter;
	[SerializeField] Text txtMainFilter;
	[SerializeField] Text txtSubFilter;

	private List<PREquipFilterData> listFilterSetData = new List<PREquipFilterData>();// multiple
	private List<PREquipFilterData> listFilterOptData = new List<PREquipFilterData>();
	private List<PREquipFilterData> listFilterSubOptData = new List<PREquipFilterData>();

	private PREquipFilterData filterOptionSet = null;// 선택된 세트옵션
	private PREquipFilterData filterOptionMain = null;//선택된 주옵션
	private PREquipFilterData filterOptionSub = null;//선택된 부옵션

	private ulong equipCost = 0;

	private bool isFilterToggle = false;

	E_DetailFilterType curDetailOption = E_DetailFilterType.None;

	[SerializeField] private CanvasGroup cgSetInfoToolTip;
	[SerializeField] private Text txtTooltipTitle;
	[SerializeField] private Text txtTooltipDesc;

	public override void Init(PREquipmentInventory _owner)
	{
		base.Init(_owner);

		osaAbility.Initialize();
		equipInfoOrigin.Initialize(OnClickEquipSlotOrigin, OnClickSetInfo);
		equipInfoTemp.Initialize(OnClickEquipSlotTemp, OnClickSetInfo);
		osaFilter.Initialize(OnClickFilerSlot);
		foreach (var iter in EnumHelper.Values<E_RuneSetType>())
		{
			if (iter == E_RuneSetType.None)
				continue;

			if (iter == E_RuneSetType.Max)
				continue;

			listFilterSetData.Add(new PREquipFilterData(iter) { isNotUsedInOSA = false, isOn = true, intData = 1 });
		}

		foreach (var iter in EnumHelper.Values<E_RuneAbilityViewType>())
		{
			if (iter == E_RuneAbilityViewType.None)
				continue;

			listFilterOptData.Add(new PREquipFilterData(iter) { isNotUsedInOSA = false, isOn = true, intData = 2 });
			listFilterSubOptData.Add(new PREquipFilterData(iter) { isNotUsedInOSA = false, isOn = true, intData = 3 });
		}
	}

	private void OnClickSetInfo(E_RuneSetType type)
	{
		CancelInvoke(nameof(FadeOutSetToolTip));
		cgSetInfoToolTip.DOKill(false);

		if (type == E_RuneSetType.None)
		{
			cgSetInfoToolTip.DOFade(0, .2f);
			return;
		}


		var info = UICommon.GetRuneSetAbilityTextArray(type);

		txtTooltipTitle.text = info[0];
		txtTooltipDesc.text = info[1];

		cgSetInfoToolTip.DOFade(1, .2f);
		Invoke(nameof(FadeOutSetToolTip), 2f);

	}
	private void FadeOutSetToolTip()
	{
		cgSetInfoToolTip.DOFade(0, .2f);
	}

	public override void Open()
	{
		base.Open();

		dicTempEquip.Clear();
		foreach (var iter in ZNet.Data.Me.CurCharData.GetEquipRuneDic(owner.EquipTarget.Tid))
		{
			dicTempEquip.Add(iter.Key, new PetRuneData(iter.Value));
		}

		if (DBPet.TryGet(owner.EquipTarget.Tid, out var table))
		{
			txtName.text = DBUIResouce.GetPetGradeFormat(DBLocale.GetText(table.PetTextID), table.Grade);
		}
		cgSetInfoToolTip.alpha = 0;


		Refresh();
		RefreshFilter();
		SetFilterState(E_DetailFilterType.None);

		Me.CurCharData.RuneRemoveUpdate -= OnRemoveRune;
		Me.CurCharData.RuneRemoveUpdate += OnRemoveRune;
	}

	public override void Close(bool invokeAction)
	{
		Me.CurCharData.RuneRemoveUpdate -= OnRemoveRune;

		OnClickSetInfo(E_RuneSetType.None);
		base.Close(invokeAction);
	}

	private void OnRemoveRune(ulong _runeID)
	{
		if (gameObject.activeSelf == false)
		{
			Me.CurCharData.RuneRemoveUpdate -= OnRemoveRune;
			return;
		}

		foreach (var iter in dicTempEquip)
		{
			if (iter.Value.RuneId == _runeID)
			{
				var refreshDic = ZNet.Data.Me.CurCharData.GetEquipRuneDic(owner.EquipTarget.Tid);

				if (refreshDic.TryGetValue(iter.Key, out var value))
					dicTempEquip[iter.Key].Reset(value);
				else
					dicTempEquip.Remove(iter.Key);

				break;
			}
		}
	}



	// 착용, 해제 시 들어옴
	public override void Refresh()
	{
		base.Refresh();

		equipInfoOrigin.SetEquipSlot(owner.EquipTarget.Tid);

		var refreshDic = ZNet.Data.Me.CurCharData.GetEquipRuneDic(owner.EquipTarget.Tid);

		bool isTempEquiped = false;

		foreach (var iter in dicTempEquip.Keys.ToList())
		{
			if (dicTempEquip[iter].OwnerPetTid > 0)
			{// 해제된 아이템 지움
				if (refreshDic.ContainsKey(iter) == false)
					dicTempEquip.Remove(iter);
				else
					dicTempEquip[iter].Reset(refreshDic[iter]);
			}
			else
			{// 기존 임시착용 슬롯 업데이트

				// 임시착용-> 착용 업데이트
				if (refreshDic.TryGetValue(dicTempEquip[iter].SlotType, out var refresh))
				{
					if (dicTempEquip[iter].RuneId == refresh.RuneId)
					{
						dicTempEquip[iter].Reset(refresh);
					}
				}
				else// 임시착용 인벤 업데이트
				{
					var data = owner.ListEquipData.Find(item => item.data.RuneId == dicTempEquip[iter].RuneId);

					if (data != null)
					{
						data.isTempEquiped = true;
						isTempEquiped = true;
					}
				}
			}
		}

		btnReset.interactable = isTempEquiped;

		owner.RefreshInven();
		equipInfoTemp.SetEquipSlotTemp(dicTempEquip);

		RefreshUI();
	}

	public override bool Filter(PetRuneData data)
	{
		if (DBItem.GetItem(data.RuneTid, out var table) == false)
			return false;

		if (filterOptionSet != null && filterOptionSet.runeSetType != table.RuneSetType)
			return false;

		if (DBRune.GetRuneEnchantTable(data.BaseEnchantTid, out var enchantTable) == false)
			return false;

		var mainAbility = DBRune.GetMainAbility(enchantTable);

		if (filterOptionMain != null && mainAbility!=null &&mainAbility.type != DBRune.ComareAbility(filterOptionMain.abilityType))
			return false;

		var firstAbility = DBRune.GetFirstAbility(data);

		if (data.FirstOptTid != 0)
		{
			if (filterOptionSub != null && firstAbility!=null && firstAbility.type != DBRune.ComareAbility(filterOptionSub.abilityType))
				return false;
		}
		else if(data.FirstOptTid == 0 && filterOptionSub != null)
		{
			return false;
		}
		return base.Filter(data);
	}

	public void OnClickEquipSlotOrigin(PREquipItemData data)
	{
		var popup = UIManager.Instance.Find<UIPopupItemInfoPREquipPair>();

		UIPopupItemInfoPREquip.E_PREquipPopupType pos = data.data.OwnerPetTid > 0 ? UIPopupItemInfoPREquip.E_PREquipPopupType.Pair_Up : UIPopupItemInfoPREquip.E_PREquipPopupType.Pair_Down;
		if (popup != null)
		{//정보팝업 열려있음
			popup.SetPopup(data, owner.EquipTarget, owner.OnInterect, pos, tempEquipMode: false);
		}
		else
		{//정보팝업 닫혀있음
			UIManager.Instance.Open<UIPopupItemInfoPREquipPair>((name, popupFrame) =>
			{
				popupFrame.SetPopup(data, owner.EquipTarget, owner.OnInterect, pos, tempEquipMode: false);
			});
		}
	}

	private bool IsTempEquiped(PREquipItemData data)
	{
		if (data.data.OwnerPetTid > 0)
			return false;

		if (dicTempEquip.ContainsKey(data.data.SlotType) == false)
			return false;

		return dicTempEquip[data.data.SlotType].OwnerPetTid == 0;
	}

	public void OnClickEquipSlotTemp(PREquipItemData data)
	{
		bool tempMode = data.data.OwnerPetTid == 0;

		Action act = null;

		if (tempMode)
		{
			tempSlot = data;
			act = OnTempEquip;
		}

		var popup = UIManager.Instance.Find<UIPopupItemInfoPREquipPair>();

		UIPopupItemInfoPREquip.E_PREquipPopupType pos = data.data.OwnerPetTid > 0 ? UIPopupItemInfoPREquip.E_PREquipPopupType.Pair_Up : UIPopupItemInfoPREquip.E_PREquipPopupType.Pair_Down;

		if (popup != null)
		{//정보팝업 열려있음
			popup.SetPopup(data, owner.EquipTarget, owner.OnInterect, pos, act, tempMode);
		}
		else
		{//정보팝업 닫혀있음
			UIManager.Instance.Open<UIPopupItemInfoPREquipPair>((name, popupFrame) =>
			{
				popupFrame.SetPopup(data, owner.EquipTarget, owner.OnInterect, pos, act, tempMode);
			});
		}
	}

	public override void OnInvenClick(PREquipItemData data)
	{
		OnClickEquipSlotTemp(data);
	}

	// 임시해제/착용
	public void OnTempEquip()
	{
		if (tempSlot == null) return;

		bool isTempEquiped = tempSlot.isTempEquiped == false;

		tempSlot.isTempEquiped = !isTempEquiped;

		var invenData = owner.ListEquipData.Find(item => item.data.RuneId == tempSlot.data.RuneId);

		if (invenData != null)
		{
			invenData.isTempEquiped = !invenData.isTempEquiped;
		}

		if (isTempEquiped)//착용됨
		{
			if (dicTempEquip.ContainsKey(invenData.data.SlotType))
			{
				if (invenData.data.OwnerPetTid == 0)
				{
					var pastData = owner.ListEquipData.Find(item => dicTempEquip[invenData.data.SlotType].RuneId == item.data.RuneId);
					pastData.isTempEquiped = !pastData.isTempEquiped;

				}

				dicTempEquip[invenData.data.SlotType].Reset(tempSlot.data);
			}
			else
			{
				dicTempEquip.Add(tempSlot.data.SlotType, new PetRuneData(tempSlot.data));
			}
		}
		else//해제됨
		{
			var dic = Me.CurCharData.GetEquipRuneDic(owner.EquipTarget.Tid);
			if (dic.TryGetValue(invenData.data.SlotType, out var tempData))
			{
				dicTempEquip[invenData.data.SlotType].Reset(tempData);
			}
			else
				dicTempEquip.Remove(invenData.data.SlotType);
		}

		tempSlot = null;

		owner.RefreshInven();

		equipInfoTemp.SetEquipSlotTemp(dicTempEquip);
		RefreshUI();
	}

	public void OnClickResetTempEquip()
	{
		foreach (var iter in dicTempEquip.Values)
		{
			var invenData = owner.ListEquipData.Find(item => item.data.RuneId == iter.RuneId);

			if (invenData == null)
				continue;

			invenData.isTempEquiped = !invenData.isTempEquiped;
		}
		dicTempEquip.Clear();

		var dic = Me.CurCharData.GetEquipRuneDic(owner.EquipTarget.Tid);

		foreach (var iter in ZNet.Data.Me.CurCharData.GetEquipRuneDic(owner.EquipTarget.Tid))
		{
			dicTempEquip.Add(iter.Key, new PetRuneData(iter.Value));
		}

		tempSlot = null;
		equipInfoTemp.SetEquipSlotTemp(dicTempEquip);
		owner.RefreshInven();
		RefreshUI();
	}

	private void RefreshUI()
	{
		equipCost = 0;
		var dic = Me.CurCharData.GetEquipRuneDic(owner.EquipTarget.Tid);
		foreach (var iter in dicTempEquip.Values)
		{
			if (iter.OwnerPetTid > 0)
				continue;

			if (dic.ContainsKey(iter.SlotType) == false)
				continue;

			if (DBItem.GetItem(dic[iter.SlotType].RuneTid, out var table) == false)
				continue;

			equipCost += table.RuneliftItemCount;
		}

		txtEquipCost.text = equipCost.ToString("N0");

		txtEquipCost.color = ConditionHelper.CheckCompareCost(DBConfig.Runelift_Use_Item, equipCost, false) ? Color.white : Color.red;

		bool isConfirmable = false;

		foreach (var iter in dicTempEquip.Values)
		{
			if (iter.OwnerPetTid <= 0)
			{
				isConfirmable = true;
				break;
			}
		}
		btnConfirm.interactable = isConfirmable;
		btnReset.interactable = isConfirmable;

		originAbilityData = UIStatHelper.GetPetStat(owner.EquipTarget.Tid);

		UIStatHelper.SetCompareStat(ref originAbilityData, UIStatHelper.GetPetStat(owner.EquipTarget.Tid, dicTempEquip.Values.ToList()));

		osaAbility.RefreshListData(originAbilityData);
	}


	public void OnClickSortType(int i)
	{
		owner.SetSortType(i > 0);
	}

	public void OnClickConfirm()
	{
		if (ConditionHelper.CheckCompareCost(DBConfig.Runelift_Use_Item, equipCost) == false)
			return;

		UIManager.Instance.Open<UIMessagePopupPREquip>((_name, _popup) =>
		{
			_popup.Set(true, owner.EquipTarget.Tid, dicTempEquip.Values.ToList(), owner.OnInterect, null);
		});
	}

	private void RefreshFilter()
	{
		toggleSortByStep.DoToggleAction(true, true);
		toggleSortByGrade.DoToggleAction(false, false);

		RefreshFilterAll();

		filterOptionMain = null;
		filterOptionSub = null;
		filterOptionSet = null;

		txtSetFilter.text = DBLocale.GetText("Set_Option_Text");
		txtMainFilter.text = DBLocale.GetText("Rune_Mian_Option_Text");
		txtSubFilter.text = DBLocale.GetText("Rune_Sub_Option_Text");
	}

	public void OnClickDetailBack()
	{
		switch (curDetailOption)
		{
			case E_DetailFilterType.None:
				break;
			case E_DetailFilterType.SetType:
				filterOptionSet = null;
				txtSetFilter.text = DBLocale.GetText("Set_Option_Text");
				break;
			case E_DetailFilterType.MainOption:
				filterOptionMain = null;
				txtMainFilter.text = DBLocale.GetText("Rune_Mian_Option_Text");

				break;
			case E_DetailFilterType.SubOption:
				filterOptionSub = null;
				txtSubFilter.text = DBLocale.GetText("Rune_Sub_Option_Text");
				break;
		}

		SetFilterState(E_DetailFilterType.None);

		owner.RefreshInven();
	}

	protected override void RefreshFilter(E_PREquipFilterType filter)
	{
		RefreshFilter();

		base.RefreshFilter(filter);
	}

	public void OnClickFilterDetail(int i)
	{
		var type = (E_DetailFilterType)i;
		switch (type)
		{
			case E_DetailFilterType.None://닫기
			case E_DetailFilterType.SetType:
			case E_DetailFilterType.MainOption:
			case E_DetailFilterType.SubOption:
				break;
			default:
				return;
		}

		curDetailOption = type;

		SetFilterState(curDetailOption);
	}

	private void SetFilterState(E_DetailFilterType type)
	{
		curDetailOption = type;
		objFilterSelect.SetActive(type == E_DetailFilterType.None);
		objDetailFilter.SetActive(type != E_DetailFilterType.None);

		switch (type)
		{
			case E_DetailFilterType.SetType:
				osaFilter.ResetListData(listFilterSetData);
				break;
			case E_DetailFilterType.MainOption:
				osaFilter.ResetListData(listFilterOptData);
				break;
			case E_DetailFilterType.SubOption:
				osaFilter.ResetListData(listFilterSubOptData);
				break;
			default:
				return;
		}
	}

	public void OnClickFilerSlot(PREquipFilterData data)
	{
		SetFilterState(E_DetailFilterType.None);

		switch (data.intData)
		{
			case 1:
				filterOptionSet = data;
				txtSetFilter.text = UICommon.GetRuneSetAbilityText(data.runeSetType);
				break;
			case 2:
				filterOptionMain = data;
				txtMainFilter.text = DBLocale.GetRuneAbilityTypeName(data.abilityType);
				break;
			case 3:
				filterOptionSub = data;
				txtSubFilter.text = DBLocale.GetRuneAbilityTypeName(data.abilityType);
				break;
		}

		owner.RefreshInven();
	}
}


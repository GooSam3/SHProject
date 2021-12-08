using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using GameDB;
using System;
using ZNet.Data;

// 현재 테이블 

public class UIPopupItemInfoPREquip : MonoBehaviour
{
	public enum E_PREquipPopupType
	{
		Single,// 한개만 뜸 , 닫을시 해제 **안씀 훗날 쓸날을 대비해 남겨둠
		Pair_Up,// 두개뜸(비교용), 닫을시 해제안함 -> 부모에서 관리됨, 위에놈(펫 장착 정보)
		Pair_Down, // 아랫놈(비교 아이템 정보, 장착관리에서만 임시착용)
	}

	[SerializeField] private GameObject objEquip;
	[SerializeField] private GameObject objBelong;

	[SerializeField] private Image imgIcon;
	[SerializeField] private Image imgSetType;
	[SerializeField] private Image imgGradeStar;

	// 이름 + 강화도
	[SerializeField] private Text txtItemName;

	// 주, 접두어 옵션
	[SerializeField] private UIAbilitySlot slotMainAbility;
	[SerializeField] private UIAbilitySlot slotFirstAbility;

	// 세트옵션
	[SerializeField] private Text txtSetOption;

	[SerializeField] private ZButton btnSell;
	[SerializeField] private ZButton btnEnchant;

	[SerializeField] private ZButton btnEquip;
	[SerializeField] private Text txtEquipBtn;

	[SerializeField] private ZButton btnTempEquip;
	[SerializeField] private Text txtTempEquipBtn;

	[SerializeField] private UIAbilityListAdapter osaAbility;

	private E_PREquipPopupType popupType;

	private C_PetChangeData interectTarget;

	private PREquipItemData runeData;

	private RuneEnchant_Table enchantTable;
	private RuneEnchant_Table nextEnchantTable;

	private bool isEquiped = false;

	private bool isMine = false;
	private bool isTempEquipMode = false;

	private Action onInterect;

	private Action onEquip;

	private UIPopupItemInfoPREquipPair owner;

	public void Initialize(UIPopupItemInfoPREquipPair _owner = null)
	{
		owner = _owner;

		osaAbility.Initialize();
		osaAbility.enabled = false;
		gameObject.SetActive(false);
	}

	// 기존 작업이 테이블 기반이 아닌 웹에서 보내온 데이터 기반임, 일단 웹데이터 기반으로 작성함
	// 혼란을 야기할 우려가 있는관계로 주석처리
	//// 들어올일 없긴한데 일단 생성, 단일모드로 작동
	//// 무조건 싱글모드
	//public void SetPopup(PetRuneData _data, E_PREquipPopupType type)
	//{
	//    runeData = _data;
	//
	//    SetPopupInfo(_data);
	//    SetPopupType(type, false);
	//}
	//
	//// 펫/탈것 장비창 외의 경우임, 펫 장비는 PREquipItemData로 관리됨
	//// 룬 tid로 호출
	//// 무조건 단일팝업임
	//public void SetPopup(uint runeTid)
	//{
	//    runeData = null;
	//
	//    var table = DBItem.GetItem(runeTid);
	//
	//    SetPopupInfo(table);
	//    SetPopupType(E_PREquipPopupType.Single, false);
	//}



	private void OnRemoveRune(ulong _runeID)
	{
		if (gameObject.activeSelf == false)
		{
			Me.CurCharData.RuneRemoveUpdate -= OnRemoveRune;
			return;
		}

		if(runeData.data.RuneId == _runeID)
		{
			OnClickClose();
		}
	}

	public void SetInteractTarget(C_PetChangeData _target)
	{
		interectTarget = _target;
	}

	// 펫장비 인벤토리에서 호출
	public void SetPopup(PREquipItemData _data, C_PetChangeData _target, Action _onInterect, Action _onEquip = null, bool tempMode = false)
	{
		onInterect = _onInterect;
		onEquip = _onEquip;

		runeData = _data;

		interectTarget = _target;

		isMine = (_target?.Tid ?? _data.data.OwnerPetTid + 1) == _data.data.OwnerPetTid;

		isTempEquipMode = tempMode;

		SetPopupInfo(_data.data);

		SetPopupType(_target != null);

		Me.CurCharData.RuneRemoveUpdate -= OnRemoveRune;
		Me.CurCharData.RuneRemoveUpdate += OnRemoveRune;
	}

	private void SetPopupInfo(PetRuneData data)
	{
		var table = DBItem.GetItem(data.RuneTid);

		SetPopupInfo(table, DBRune.GetRuneEnchantTable(data.BaseEnchantTid));

		enchantTable = DBRune.GetRuneEnchantTable(data.BaseEnchantTid);
		nextEnchantTable = DBRune.GetNextRuneEnchantTable(data.BaseEnchantTid, enchantTable.GroupID);

		//---- 주, 접두어 

		var mainAbility = DBRune.GetMainAbility(enchantTable);

		slotMainAbility.gameObject.SetActive(mainAbility != null);

		if (mainAbility != null)
			slotMainAbility.SetSlot(mainAbility);

		var firstAbility = DBRune.GetFirstAbility(data);

		slotFirstAbility.gameObject.SetActive(firstAbility != null);

		if (firstAbility != null)
			slotFirstAbility.SetSlot(firstAbility);

		var setType = UICommon.GetRuneSetAbilityText(DBItem.GetItem(data.RuneTid).RuneSetType);

		txtSetOption.text = setType.Replace("\n", " ");

		//----
		osaAbility.RefreshListData(DBRune.GetSubAbility(data));

		objEquip.SetActive(data.OwnerPetTid > 0);
	}

	private void SetPopupInfo(Item_Table table, RuneEnchant_Table enchantTable)
	{
		if (table.ItemUseType != E_ItemUseType.Rune)
		{
			ZLog.LogError(ZLogChannel.UI, "룬(펫 장비)도 아닌놈이 룬팝업을 호출했다!!!");
			UIManager.Instance.Close<UIPopupItemInfoPREquipSingle>(true);
			return;
		}

		imgIcon.sprite = ZManagerUIPreset.Instance.GetSprite(table.IconID);
		imgSetType.sprite = UICommon.GetRuneSetTypeSprite(table.RuneSetType);
		imgGradeStar.sprite = UICommon.GetRuneGradeStarSprite(table.Grade);

		objBelong.SetActive(table.BelongType == E_BelongType.Belong);

		string enchantStep = UICommon.GetEnchantText(enchantTable.EnchantStep);
		string itemName = DBUIResouce.GetItemGradeFormat(DBLocale.GetText(table.ItemTextID), (byte)(table.RuneGradeType + 1));
		txtItemName.text = $"{itemName}{enchantStep}";
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="type"></param>
	/// <param name="interactable"> 버튼 인터렉션 여부 </param>
	private void SetPopupType(bool interactable)
	{
		// 조건 다채로운관계로 안쓰는거부터 처리 후 리턴
		if (interactable == false)
		{
			btnEnchant.gameObject.SetActive(interactable);
			btnSell.gameObject.SetActive(interactable);
			btnEquip.gameObject.SetActive(interactable);
			btnTempEquip.gameObject.SetActive(interactable);
			btnSell.gameObject.SetActive(interactable);
			return;
		}

		// 강화는 강화수치 남아있는한 무조건뜸
		btnEnchant.gameObject.SetActive(nextEnchantTable != null);

		// 판매는 판매모드시에만(따로설정)
		btnSell.gameObject.SetActive(false);

		isEquiped = runeData.data.OwnerPetTid > 0;

		// 일단 기본설정은 단일 : 장착, 다중 : 임시장착
		// Temporary_Disarm_Text : 임시해제
		// Lift_Text : 해제

		// 위 팝업은 펫이 장착한 상태

		btnEquip.gameObject.SetActive(!isTempEquipMode);
		if (!isTempEquipMode)
			txtEquipBtn.text = isEquiped ? DBLocale.GetText("Lift_Text") : DBLocale.GetText("Equip_Text");

		btnTempEquip.gameObject.SetActive(isTempEquipMode);
		if (isTempEquipMode)
			txtTempEquipBtn.text = runeData.isTempEquiped == false ? DBLocale.GetText("Temporary_Equip_Text") : DBLocale.GetText("Temporary_Disarm_Text");

		btnSell.gameObject.SetActive(runeData.data.OwnerPetTid <= 0);

		SetButtonInteractive(true);

		gameObject.SetActive(true);
	}

	public void ActivateSellMode()
	{
		btnSell.gameObject.SetActive(true);
	}

	// 인터렉션 후 패킷 응답 대기처리 (대기시 false)
	public void SetButtonInteractive(bool state)
	{
		btnEnchant.interactable = state;
		btnSell.interactable = state;
		btnEquip.interactable = state;
		btnTempEquip.interactable = state;
	}

	public void OnClickClose()
	{
		Me.CurCharData.RuneRemoveUpdate -= OnRemoveRune;

		this.gameObject.SetActive(false);
	}

	// 단일모드에서만 연결됨(Button - EventHandler)
	// 비교모드시 상위에서 관리
	public void OnClickEquip()
	{
		if (interectTarget == null)
		{
			ZLog.LogError(ZLogChannel.System, "팝업은 열렸고, 장착은 눌렸는데, 대상이 없음..??!!?!");
			return;
		}

		if (isEquiped)
		{
			OnClickUnequip();
			return;
		}

		// 해당 슬롯에 아이템 장착 확인

		SetButtonInteractive(false);

		UIManager.Instance.Open<UIMessagePopupPREquip>((_name, _popup) =>
		{
			_popup.Set(true, interectTarget.Tid, runeData.data, OnInterect, () => SetButtonInteractive(true));
		});
	}

	private void OnInterect()
	{
		onInterect?.Invoke();

		owner?.SetClose();
	}

	private void OnClickUnequip()
	{
		SetButtonInteractive(false);

		UIManager.Instance.Open<UIMessagePopupPREquip>((_name, _popup) =>
		{
			_popup.Set(false, runeData.data.OwnerPetTid, runeData.data, OnInterect, () => SetButtonInteractive(true));
		});
	}

	public void OnClickEquipTemp()
	{
		if (interectTarget == null)
		{
			ZLog.LogError(ZLogChannel.System, "팝업은 열렸고, 장착은 눌렸는데, 대상이 없음..??!!?!");
			return;
		}

		if (runeData.isTempEquiped)
		{
			OnClickUnequipTemp();
			return;
		}

		onEquip?.Invoke();
		OnInterect();
	}

	private void OnClickUnequipTemp()
	{
		onEquip?.Invoke();
		OnInterect();
	}

	public void OnClickSell()
	{
		SetButtonInteractive(false);

		if (DBItem.GetItem(runeData.data.RuneTid, out var table)==false)
		{
			ZLog.Log(ZLogChannel.System, $"룬 테이블내 데이터 없음 : {runeData.data.RuneTid}");
			return;
		}

		UIMessagePopup.ShowCostPopup(DBLocale.GetText("Sell_Text"), DBLocale.GetText("Rune_Break_Message"), DBConfig.Gold_ID, table.SellItemCount, () =>
		{
			ZWebManager.Instance.WebGame.REQ_RuneSell(runeData.data.RuneId, (recvPacket, recvMsgPacket) =>
			{
				OnInterect();
			}, null);
		}, costDescKey: "Sell_Own_Desc", onCancel: () => SetButtonInteractive(true));
	}

	public void OnClickEnchant()
	{
		SetButtonInteractive(false);

		if (UIManager.Instance.Find<UIPopupEnhanceEquipPR>() == null)
		{
			ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIAbilitySlotEnhancePR), delegate
			{
				UIManager.Instance.Open<UIPopupEnhanceEquipPR>((popupName, popup) =>
				{
					popup.SetEnhanceEquip(runeData.data, OnInterect);
				});
			});
		}
		else
		{
			UIManager.Instance.Open<UIPopupEnhanceEquipPR>((popupNmae, popup) =>
			{
				popup.SetEnhanceEquip(runeData.data, OnInterect);
			});
		}
	}
}

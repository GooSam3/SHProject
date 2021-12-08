using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

using DG.Tweening;

public class UIFrameItemEnhance : ZUIFrameBase
{
	public struct EnhanceDestInfo
	{
		public string TypeText;
		public string PrevText;
		public string NextText;
		public float AddValue;
	}

	#region UI Variable

	public UIItemSlot MaterialSlot, EnhanceItemSlot;

	public Text EnhanceItemName;

	[SerializeField] private ZButton CloseBtn;
	/// <summary>마일리지</summary>
	[SerializeField] private GameObject Mileage;

	/// <summary>마일리지 텍스</summary>
	[SerializeField] private Text MileageText;
	/// <summary>BeforeEnhanceCount : 강화전에 강화 수치, AfterEnhanceCount : 강화후에 강화 수치</summary>
	[SerializeField] private Text BeforeEnhanceCount, AfterEnhanceCount;
	/// <summary>강화 정보 텍스트(ex.강화 실패 없음)</summary>
	[SerializeField] private Text EnhanceInfo;
	/// <summary>강화 수치 표시 Line, Dot, EffectDot (UI Prefab 참고..)</summary>
	[SerializeField] private List<Image> Links, Dots, EffectDots;
	[SerializeField] private List<Text> GageTexts;
	/// <summary>강화 아이템 정보 리스트</summary>
	[SerializeField] private ScrollRect EnhanceStatList;
	/// <summary>강화 비용</summary>
	[SerializeField] private Text EnchantCostText;
	/// <summary>스마트 강화 박스</summary>
	[SerializeField] private ZToggle SmartToggle;
	/// <summary>강화 버튼</summary>
	[SerializeField] private ZButton EnhanceButton;
	/// <summary>스마트 강화 가리게</summary>
	[SerializeField] private List<GameObject> SmartBoard = new List<GameObject>();
	/// <summary>스마트 강화 버튼</summary>
	[SerializeField] private List<GameObject> SmartButton = new List<GameObject>();
	/// <summary>스마트 강화 버튼 체크 박스</summary>
	[SerializeField] private GameObject SmartButtonCheckOn;
	/// <summary>스마트 강화 Default 버튼</summary>
	[SerializeField] private ZToggle SmartButtonDefault;
	/// <summary>스마트 강화 박스</summary>
	[SerializeField] private GameObject MultiEnhanceBox;
	#endregion

	#region System Variable
	private ZItem ItemOrigin; // 강화, 승급 전 아이템 캐싱용
	public ZItem MaterialItem = null; // 재료아이템
	public ZItem EnhanceItem = null; // 등록된 아이템

	[SerializeField] private ushort EnchantType = 0;
	List<EnhanceDestInfo> descList = new List<EnhanceDestInfo>();
	private uint SmartEnhanceCount;
	private E_EnchantResultType EnchantResultType;
	private string ItemUniqueKey;
	[SerializeField] private List<UIEnhanceStatListItem> EnhanceStatDataList;
	public override bool IsBackable => true;
	#endregion

	#region Fx Variable

	[Serializable]
	private class FxTweenObj
	{
		public Transform originPos;
		public Transform destPos;

		public GameObject objTarget;
	}

	private enum E_SafeFxIndex
	{
		Degrade = 0,
		Fail = 1,
		Success = 2,
		SuccessHigher = 3,
	}

	[SerializeField, Header("=::==Fx==::="), Space(10)] private FxInteractableGroup fxInteractableGroup;

	[SerializeField] private float ObjCollapseTweeningDuration = 1f;

	// E_FxIndex에 주의하여 설정
	[SerializeField] private List<UIFxParticle> listSafeFx;

	[SerializeField] private FxTweenObj tweenObjMatSlot;
	[SerializeField] private FxTweenObj tweenObjEquipSlot;

	private bool isDirectionWait = false;
	#endregion Fx Variable


	protected override void OnInitialize()
	{
		base.OnInitialize();
	}

	protected override void OnRemove()
	{
		base.OnRemove();

		ZPoolManager.Instance.Clear(E_PoolType.UI, nameof(UIEnhanceStatListItem));
	}

	protected override void OnShow(int _LayerOrder)
	{
		base.OnShow(_LayerOrder);
		ResetDirection();

		UIManager.Instance.Open<UIFrameInventory>();

		MaterialSlot.SetEmpty();
		EnhanceItemSlot.SetEmpty();

		EnhanceItemName.text = "";

		MaterialSlot.OnClickISlot = ClickMaterialSlot;
		EnhanceItemSlot.OnClickISlot = ClickEnhanceItemSlot;
	}

	protected override void OnHide()
	{
		base.OnHide();
		ResetDirection();

		ClearData();

		if (UIManager.Instance.Find(out UIFrameInventory _inventory))
			_inventory.ShowInvenSort((int)E_InvenSortType.All);

		// 연출 삭제
		UIManager.Instance.Close<UIPopupEnhanceDanger>(true);
	}

	private void ClearData()
	{
		MaterialSlot.SetEmpty();
		EnhanceItemSlot.SetEmpty();

		MultiEnhanceBox.SetActive(false);

		EnhanceButton.interactable = false;

		EnhanceInfo.text = string.Empty;
		BeforeEnhanceCount.text = string.Empty;
		AfterEnhanceCount.text = string.Empty;
		EnchantType = 0;
		EnchantCostText.text = "0";
		SmartToggle.isOn = false;
		SmartButtonDefault.SelectToggle();

		ClearEnhanceStatData();

		for (int i = 0; i < Links.Count; i++)
			Links[i].color = new Color32((byte)123, (byte)45, (byte)45, (byte)255);
		for (int i = 0; i < Dots.Count; i++)
			Dots[i].color = new Color32((byte)123, (byte)45, (byte)45, (byte)255);
		for (int i = 0; i < GageTexts.Count; i++)
			GageTexts[i].color = new Color32((byte)123, (byte)45, (byte)45, (byte)255);
	}

	private void ClearEnhanceStatData()
	{
		for (int i = 0; i < EnhanceStatDataList.Count; i++)
			Destroy(EnhanceStatDataList[i].gameObject);

		EnhanceStatDataList.Clear();
	}

	public void SetEnhanceItem(ZItem _item)
	{
		if (_item == null)
		{
			EnhanceItemName.text = "";
			ClickEquipment();
			return;
		}

		EnhanceItem = _item;
		EnhanceItemSlot.SetItem(_item);
		EnhanceItemName.text = DBItem.GetItemName(_item.item_tid);
		ItemUniqueKey = _item.GetUniqueKey;

		UpdateEnhanceUI();
	}

	public void SetMaterial(ZItem _item)
	{
		if (_item == null)
			return;

		MaterialItem = _item;
		MaterialSlot.SetItem(_item);

		UpdateEnhanceUI();
	}

	private void UpdateEnhanceUI()
    {
		MaterialSlot.Refresh();
		EnhanceItemSlot.Refresh();

		
		if (EnhanceItemSlot.item_Tid == 0 && MaterialSlot.item_Tid == 0)
        {
			//아무것도 없으면 ""
			EnhanceItemName.text = "";
		}
		else if (EnhanceItemSlot.item_Tid == 0 && MaterialSlot.item_Tid != 0)
        {
			//장비가 없으면 주문서 이름 표시
			EnhanceItemName.text = DBItem.GetItemName(MaterialSlot.item_Tid);
		}
		else if (EnhanceItemSlot.item_Tid != 0 && MaterialSlot.item_Tid == 0)
		{
			//주문서가 없으면 주문서 넣어달라는 Locale표시
			EnhanceItemName.text = DBLocale.GetText("Enchant_Need_Material_Des");
		}
		else if (EnhanceItemSlot.item_Tid != 0 && MaterialSlot.item_Tid != 0)
		{
			//주문서가 없으면 주문서 넣어달라는 Locale표시
			EnhanceItemName.text = DBItem.GetItemName(EnhanceItemSlot.item_Tid);
		}



		// 둘다 있으면 UI 갱신
		EnhanceButton.interactable = false;
		if(MaterialSlot.item_Tid != 0 && EnhanceItemSlot.item_Tid != 0)
        {
			UpdateCost();
			UpdateEnchantInfo();

			UpdateEnhanceGage();
			CheckEnhanceStat();
			EnhanceButton.interactable = true;
		}
		
	}

    private void CheckEnhanceStat()
	{
		ClearEnhanceStatData();

		if(EnhanceItemSlot.item_Tid == 0)
        {
			ResetEnchantInfo();
			return;
        }

		var tableEquipment = DBItem.GetItem(EnhanceItemSlot.item_Tid);
		var tableItemEnchantData = DBItem.GetEnchantData(EnhanceItemSlot.item_Tid);

		// 재료 없는 강화일 경우
		if (tableItemEnchantData.EnchantType.HasFlag(E_EnchantType.NoUseItemEnchant))
		{
			SetEnchantInfo(tableEquipment);

			SetAbility();
			return;
		}
      
		
		var tableMaterial = DBItem.GetItem(MaterialSlot.item_Tid);

		// 저주 강화일 경우 (테이블에 별도의 구분 데이터가 없어서 이렇게 체크)
		if (tableMaterial.ItemID == 3200 || tableMaterial.ItemID == 3300)
		{
			if (tableEquipment.Step > 0)
            {
				BeforeEnhanceCount.text = tableEquipment.Step.ToString();
				AfterEnhanceCount.text = (tableEquipment.Step - 1).ToString();
			}
			else
			{
				BeforeEnhanceCount.text = "강화 불가";
				AfterEnhanceCount.text = string.Empty;
			}

			EnhanceButton.interactable = tableEquipment.EnchantUseType.HasFlag(E_EnchantUseType.CurseEnchant);
			EnhanceStatList.gameObject.SetActive(tableEquipment.EnchantUseType.HasFlag(E_EnchantUseType.CurseEnchant));
			EnhanceInfo.gameObject.SetActive(tableEquipment.EnchantUseType.HasFlag(E_EnchantUseType.CurseEnchant));

			var table = DBItem.GetGroupList(tableEquipment.GroupID);
			var curseTable = table.Find(item => item.Step == tableEquipment.Step - 1);

			UpdateDescList(EnhanceItemSlot.item_Tid, curseTable.ItemID);
		}
		else
		{
			SetEnchantInfo(tableEquipment);
		}

		SetAbility();

		// to do : 나중에 정리
		void SetEnchantInfo(Item_Table _tableEquipment)
        {
			if (_tableEquipment.Step < 9)
            {
				BeforeEnhanceCount.text = tableEquipment.Step.ToString();
				AfterEnhanceCount.text = (_tableEquipment.Step + 1).ToString();
			}
			else
			{
				BeforeEnhanceCount.text = "MAX";
				AfterEnhanceCount.text = string.Empty;
			}

			EnhanceButton.interactable = _tableEquipment.Step < 9;
			EnhanceStatList.gameObject.SetActive(_tableEquipment.Step < 9);
			EnhanceInfo.gameObject.SetActive(_tableEquipment.Step < 9);

			UpdateDescList(EnhanceItemSlot.item_Tid, _tableEquipment.StepUpID);
		}

		void ResetEnchantInfo()
        {
			BeforeEnhanceCount.text = "";
			AfterEnhanceCount.text = "";
			EnhanceButton.interactable = false;
			EnhanceStatList.gameObject.SetActive(false);
			EnhanceInfo.gameObject.SetActive(false);
			EnhanceStatDataList.Clear();

		}

		// to do : 나중에 정리
		void SetAbility()
        {
			ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIEnhanceStatListItem), delegate
			{
				for (int i = 0; i < descList.Count; i++)
				{
					if (descList[i].PrevText == null || descList[i].NextText == null)
						continue;

					UIEnhanceStatListItem obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIEnhanceStatListItem)).GetComponent<UIEnhanceStatListItem>();

					if (obj != null)
					{
						obj.transform.SetParent(EnhanceStatList.content, false);
						obj.SetSlot(descList[i]);
						EnhanceStatDataList.Add(obj);
					}
				}
			});
		}
	}

	public void ClickEquipment()
	{
		// 연출을 하지 않을땐 기본동작
		if (isDirectionWait == false)
		{
			EnhanceItemSlot.SetEmpty();
			UpdateEnhanceUI();
		}
		else // 연출일땐 결과이펙트 출력
		{
			isDirectionWait = false;
		}
	}
	public void ClickMaterialSlot(UIItemSlot _itemSlot)
    {
		if(MaterialSlot.item_Tid != 0)
			MaterialSlot.SetEmpty();

		UpdateEnhanceUI();

		if (UIManager.Instance.Find(out UIFrameInventory _inventory))
			_inventory.ShowInvenSort((int)E_InvenSortType.Enhance);
	}

	public void ClickEnhanceItemSlot(UIItemSlot _itemSlot)
    {
		if(EnhanceItemSlot.item_Tid != 0)
			EnhanceItemSlot.SetEmpty();

		UpdateEnhanceUI();

		if (UIManager.Instance.Find(out UIFrameInventory _inventory))
			_inventory.ShowInvenSort((int)E_InvenSortType.EnhanceEquip);
	}

	/// <summary>아이템 강화 아이템 정보 셋팅 함수</summary>
	/// <param name="_currentTid">강화 아이템 테이블 아이디</param>
	/// <param name="_nextTid">강화 승급시 아이템 테이블 아이디</param>
	private void UpdateDescList(uint _currentTid, uint _nextTid)
	{
		descList.Clear();

		var itemData = DBItem.GetItem(_currentTid);
		var nextData = DBItem.GetItem(_nextTid);

		List<uint> abilityIds = new List<uint>();

		if (itemData.AbilityActionID_01 != 0)
			abilityIds.Add(itemData.AbilityActionID_01);
		if (itemData.AbilityActionID_02 != 0)
			abilityIds.Add(itemData.AbilityActionID_02);
		if (itemData.AbilityActionID_03 != 0)
			abilityIds.Add(itemData.AbilityActionID_03);

		Dictionary<GameDB.E_AbilityType, System.ValueTuple<float, float>> baseabilitys = new Dictionary<GameDB.E_AbilityType, System.ValueTuple<float, float>>();
		Dictionary<GameDB.E_AbilityType, System.ValueTuple<float, float>> abilitys = new Dictionary<GameDB.E_AbilityType, System.ValueTuple<float, float>>();

		foreach (var baseActionId in abilityIds)
		{
			var abilityActionData = DBAbility.GetAction(baseActionId);
			if (abilityActionData.AbilityViewType != GameDB.E_AbilityViewType.ToolTip)
			{
				var enumer = DBAbility.GetAllAbilityData(baseActionId).GetEnumerator();
				while (enumer.MoveNext())
				{
					if (baseabilitys.ContainsKey(enumer.Current.Key))
						baseabilitys[enumer.Current.Key] = (baseabilitys[enumer.Current.Key].Item1 + enumer.Current.Value.Item1, baseabilitys[enumer.Current.Key].Item2 + enumer.Current.Value.Item2);
					else
						baseabilitys.Add(enumer.Current.Key, enumer.Current.Value);
				}
			}
			else
			{
				descList.Add(new EnhanceDestInfo() { TypeText = DBLocale.ParseAbilityTooltip(abilityActionData, "2", "1") });
			}
		}

		if (nextData != null)
		{
			if (nextData.AbilityActionID_01 != 0)
			{
				var abilityActionData = DBAbility.GetAction(nextData.AbilityActionID_01);
				if (abilityActionData.AbilityViewType == GameDB.E_AbilityViewType.ToolTip)
				{
					if (!abilityIds.Contains(nextData.AbilityActionID_01))
						descList.Add(new EnhanceDestInfo() { TypeText = string.Format("<color=#FF8200>{0}</color>", DBLocale.ParseAbilityTooltip(abilityActionData, "2", "1")) });
				}
				else
				{
					var enumer = DBAbility.GetAllAbilityData(nextData.AbilityActionID_01).GetEnumerator();
					while (enumer.MoveNext())
					{
						if (abilitys.ContainsKey(enumer.Current.Key))
							abilitys[enumer.Current.Key] = (abilitys[enumer.Current.Key].Item1 + enumer.Current.Value.Item1, abilitys[enumer.Current.Key].Item2 + enumer.Current.Value.Item2);
						else
							abilitys.Add(enumer.Current.Key, enumer.Current.Value);
					}
				}
			}
			if (nextData.AbilityActionID_02 != 0)
			{
				var abilityActionData = DBAbility.GetAction(nextData.AbilityActionID_02);
				if (abilityActionData.AbilityViewType == GameDB.E_AbilityViewType.ToolTip)
				{
					if (!abilityIds.Contains(nextData.AbilityActionID_02))
						descList.Add(new EnhanceDestInfo() { TypeText = string.Format("<color=#FF8200>{0}</color>", DBLocale.ParseAbilityTooltip(abilityActionData, "2", "1")) });
				}
				else
				{
					var enumer = DBAbility.GetAllAbilityData(nextData.AbilityActionID_02).GetEnumerator();
					while (enumer.MoveNext())
					{
						if (abilitys.ContainsKey(enumer.Current.Key))
							abilitys[enumer.Current.Key] = (abilitys[enumer.Current.Key].Item1 + enumer.Current.Value.Item1, abilitys[enumer.Current.Key].Item2 + enumer.Current.Value.Item2);
						else
							abilitys.Add(enumer.Current.Key, enumer.Current.Value);
					}
				}
			}
			if (nextData.AbilityActionID_03 != 0)
			{
				var abilityActionData = DBAbility.GetAction(nextData.AbilityActionID_03);
				if (abilityActionData.AbilityViewType == GameDB.E_AbilityViewType.ToolTip)
				{
					if (!abilityIds.Contains(nextData.AbilityActionID_03))
						descList.Add(new EnhanceDestInfo() { TypeText = string.Format("<color=#FF8200>{0}</color>", DBLocale.ParseAbilityTooltip(abilityActionData, "2", "1")) });
				}
				else
				{
					var enumer = DBAbility.GetAllAbilityData(nextData.AbilityActionID_03).GetEnumerator();
					while (enumer.MoveNext())
					{
						if (abilitys.ContainsKey(enumer.Current.Key))
							abilitys[enumer.Current.Key] = (abilitys[enumer.Current.Key].Item1 + enumer.Current.Value.Item1, abilitys[enumer.Current.Key].Item2 + enumer.Current.Value.Item2);
						else
							abilitys.Add(enumer.Current.Key, enumer.Current.Value);
					}
				}
			}
		}
		else
		{
			foreach (var key in baseabilitys.Keys)
			{
				abilitys.Add(key, baseabilitys[key]);
			}
		}

		foreach (var key in abilitys.Keys)
		{
			if (!DBAbility.IsParseAbility(key))
				continue;

			float abilityminValue = (uint)abilitys[key].Item1;
			float abilitymaxValue = (uint)abilitys[key].Item2;

			if (abilityminValue <= 0 && abilitymaxValue <= 0)
				continue;

			if (baseabilitys.ContainsKey(key) && ((uint)baseabilitys[key].Item1 > 0 || (uint)baseabilitys[key].Item2 > 0))
			{
				float baseAbilityminValue = (uint)baseabilitys[key].Item1;
				float baseAbilitymaxValue = (uint)baseabilitys[key].Item2;

				var basevalue = DBAbility.ParseAbilityValue(key, baseAbilityminValue, baseAbilitymaxValue);

				if (baseAbilityminValue != abilityminValue || baseAbilitymaxValue != abilitymaxValue)
				{
					var addValue = abilityminValue - baseAbilityminValue;

					descList.Add(new EnhanceDestInfo()
					{
						TypeText = DBLocale.GetText(DBAbility.GetAbilityName(key)),
						PrevText = basevalue,
						NextText = DBAbility.ParseAbilityValue(key, abilityminValue, abilitymaxValue),
						AddValue = addValue
					});
				}
				else
					descList.Add(new EnhanceDestInfo()
					{
						TypeText = DBLocale.GetText(DBAbility.GetAbilityName(key)),
						PrevText = basevalue,
						NextText = DBAbility.ParseAbilityValue(key, abilityminValue, abilitymaxValue),
						AddValue = 0
					});
			}
			else
			{
				var newValue = DBAbility.ParseAbilityValue(key, abilityminValue, abilitymaxValue);

				descList.Add(new EnhanceDestInfo()
				{
					TypeText = DBLocale.GetText(DBAbility.GetAbilityName(key)),
					PrevText = "",
					NextText = DBAbility.ParseAbilityValue(key, abilityminValue, abilitymaxValue),
					AddValue = abilityminValue
				});
			}
		}
	}

	/// <summary>아이템 강화 게이지 셋팅</summary>
	private void UpdateEnhanceGage()
	{
		// 색상 초기화
		for (int i = 0; i < Links.Count; i++)
			Links[i].color = new Color32((byte)67, (byte)71, (byte)76, (byte)255);
		for (int i = 0; i < Dots.Count; i++)
			Dots[i].color = new Color32((byte)67, (byte)71, (byte)76, (byte)255);
		for (int i = 0; i < GageTexts.Count; i++)
			GageTexts[i].color = new Color32((byte)67, (byte)71, (byte)76, (byte)255);

		byte stepCount = 0;

		if(EnhanceItemSlot.item_Tid != 0)
			stepCount = DBItem.GetItem(EnhanceItemSlot.item_Tid).Step;
		
		if (MaterialSlot.item_Tid == 3200 || MaterialSlot.item_Tid == 3300)
		{
			stepCount -= 1;

			if (stepCount >= 0)
			{
				Links[stepCount].color = new Color32((byte)123, (byte)45, (byte)45, (byte)255);
				Dots[stepCount].color = new Color32((byte)123, (byte)45, (byte)45, (byte)255);
				GageTexts[stepCount].color = new Color32((byte)123, (byte)45, (byte)45, (byte)255);
			}
		}
		else
		{
			for (int i = 0; i < Links.Count; i++)
				Links[i].color = new Color32((byte)123, (byte)45, (byte)45, (byte)255);
			for (int i = 0; i < Dots.Count; i++)
				Dots[i].color = new Color32((byte)123, (byte)45, (byte)45, (byte)255);
			for (int i = 0; i < GageTexts.Count; i++)
				GageTexts[i].color = new Color32((byte)123, (byte)45, (byte)45, (byte)255);

			for (int i = 0; i < stepCount; i++)
				Links[i].color = new Color32((byte)67, (byte)71, (byte)76, (byte)255);
			for (int i = 0; i < stepCount + 1; i++)
				Dots[i].color = new Color32((byte)67, (byte)71, (byte)76, (byte)255);
			for (int i = 0; i < stepCount; i++)
				GageTexts[i].color = new Color32((byte)67, (byte)71, (byte)76, (byte)255);

			if (stepCount < 9)
			{
				Links[stepCount].color = new Color32((byte)250, (byte)213, (byte)98, (byte)255);
				Dots[stepCount + 1].color = new Color32((byte)250, (byte)213, (byte)98, (byte)255);
				GageTexts[stepCount].color = new Color32((byte)250, (byte)213, (byte)98, (byte)255);
			}
		}
	}

	/// <summary>강화 비용 업데이트 함수</summary>
	private void UpdateCost()
	{
		if (EnhanceItemSlot.item_Tid == 0 || MaterialSlot.item_Tid == 0)
			return;

		var itemTid = EnhanceItemSlot.item_Tid;
		var materialTid = MaterialSlot.item_Tid;

		var tableItemEnchantData = DBItem.GetEnchantData(itemTid);

		if (tableItemEnchantData != null)
		{
			if (tableItemEnchantData.EnchantType.HasFlag(E_EnchantType.NoUseItemEnchant))
				EnchantCostText.text = string.Format("{0}", tableItemEnchantData.NormalUseGoldCount);
			else if (tableItemEnchantData.NormalUseItemID.Contains(materialTid))
				EnchantCostText.text = string.Format("{0}", tableItemEnchantData.NormalUseGoldCount);
			else if (tableItemEnchantData.BlessUseItemID.Contains(materialTid))
				EnchantCostText.text = string.Format("{0}", tableItemEnchantData.BlessUseGoldCount);
			else if (tableItemEnchantData.CurseUseItemID.Contains(materialTid))
				EnchantCostText.text = string.Format("{0}", tableItemEnchantData.CurseUseGoldCount);
		}
	}

	/// <summary>강화 내용 업데이트 함수</summary>
	private void UpdateEnchantInfo()
	{
		if (EnhanceItemSlot.item_Tid == 0)
			return;

		var tableItemEnchantData = DBItem.GetEnchantData(EnhanceItemSlot.item_Tid);

		if (tableItemEnchantData != null)
		{
			var maxsafeEnchantStep = DBItem.GetMaxSafeEnchantStep(EnhanceItemSlot.item_Tid);
			EnhanceInfo.text = maxsafeEnchantStep == 0 || maxsafeEnchantStep < DBItem.GetItem(EnhanceItemSlot.item_Tid).Step ?
			DBLocale.GetText("Enchant_Destroy_Des") : EnhanceInfo.text = DBLocale.GetText("Enchant_Protection_Des");

			if(!tableItemEnchantData.EnchantType.HasFlag(E_EnchantType.NoUseItemEnchant))
				if ((MaterialSlot.item_Tid == 3200 || MaterialSlot.item_Tid == 3300))
					EnhanceInfo.text = "강화 수치가 1단계 내려갑니다.";
			
			if (tableItemEnchantData.CurseUseItemID.Contains(MaterialSlot.item_Tid))
			{
				EnhanceInfo.text = DBLocale.GetText("Enchant_Destroy_Des");
			}
		}
	}

	/// <summary> 강화 버튼 콜백 함수 </summary>
	public void ClickEnhanceItem()
	{
		if(MaterialSlot.item_Tid == 0)
        {
			ZLog.Log(ZLogChannel.UI, "강화주문서가 없는데슝");
			return;
        }

		if(EnhanceItemSlot.item_Tid == 0)
        {
			ZLog.Log(ZLogChannel.UI, "강화 장비가 없는데슝");
			return;
        }

		StartEnhance();
	}

	private void StartEnhance()
	{
		var tableItemEnchantData = DBItem.GetEnchantData(EnhanceItemSlot.item_Tid);
		bool notUseMaterial = false;
		//재료 필요 없는 경우
		if (null != tableItemEnchantData)
			notUseMaterial = tableItemEnchantData.EnchantType.HasFlag(GameDB.E_EnchantType.NoUseItemEnchant);

		// 골드체크
		if (notUseMaterial == false && ConditionHelper.CheckCompareCost(DBConfig.Gold_ID, tableItemEnchantData.NormalUseGoldCount) == false)
			return;

		if (!notUseMaterial)
		{
			if (MaterialSlot.item_Tid == 0)
			{
				UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
				{
					_popup.Open(ZUIString.WARRING, DBLocale.GetText("재료를 확인해주세요."),
					new string[] { ZUIString.LOCALE_OK_BUTTON },
					new Action[] { delegate
					{
						_popup.Close();
					}
					});
				});
				return;
			}

			if (tableItemEnchantData != null)
			{
				if (tableItemEnchantData.NormalUseItemID.Contains(MaterialSlot.item_Tid) && !(ZNet.Data.Me.CurCharData.CheckCountInvenItemUsingMaterial(MaterialSlot.item_Tid, tableItemEnchantData.NormalUseItemCount) &&
						ZNet.Data.Me.CurCharData.CheckCountInvenItemUsingMaterial(DBConfig.Gold_ID, tableItemEnchantData.NormalUseGoldCount)))
				{
					UIMessagePopup.ShowPopupOk(DBLocale.GetText("NOT_ENOUGH_ENCHANT_MATERIAL_COUNT"));
					return;
				}
				else if (tableItemEnchantData.BlessUseItemID.Contains(MaterialSlot.item_Tid) && !(ZNet.Data.Me.CurCharData.CheckCountInvenItemUsingMaterial(MaterialSlot.item_Tid, tableItemEnchantData.BlessUseItemCount) &&
						ZNet.Data.Me.CurCharData.CheckCountInvenItemUsingMaterial(DBConfig.Gold_ID, tableItemEnchantData.BlessUseGoldCount)))
				{
					UIMessagePopup.ShowPopupOk(DBLocale.GetText("NOT_ENOUGH_ENCHANT_MATERIAL_COUNT"));
					return;
				}
				else if (tableItemEnchantData.CurseUseItemID.Contains(MaterialSlot.item_Tid) && !(ZNet.Data.Me.CurCharData.CheckCountInvenItemUsingMaterial(MaterialSlot.item_Tid, tableItemEnchantData.CurseUseItemCount) &&
						ZNet.Data.Me.CurCharData.CheckCountInvenItemUsingMaterial(DBConfig.Gold_ID, tableItemEnchantData.CurseUseGoldCount)))
				{
					UIMessagePopup.ShowPopupOk(DBLocale.GetText("NOT_ENOUGH_ENCHANT_MATERIAL_COUNT"));
					return;
				}
			}

			if (tableItemEnchantData.NormalUseItemID.Contains(MaterialSlot.item_Tid) || notUseMaterial)
				EnchantType = (ushort)GameDB.E_EnchantUseType.NormalEnchant;
			else if (tableItemEnchantData.BlessUseItemID.Contains(MaterialSlot.item_Tid))
				EnchantType = (ushort)GameDB.E_EnchantUseType.BlessEnchant;
			else if (tableItemEnchantData.CurseUseItemID.Contains(MaterialSlot.item_Tid))
				EnchantType = (ushort)GameDB.E_EnchantUseType.CurseEnchant;
		}
		else
			EnchantType = (ushort)GameDB.E_EnchantUseType.NormalEnchant;

		ItemOrigin = new ZItem(EnhanceItem);

		if (notUseMaterial)
		{
			CloseBtn.gameObject.SetActive(false);
			ZWebManager.Instance.WebGame.REQ_EnchantItem(EnchantType, new List<ZItem>() { EnhanceItem }, 0, 0, (recv, recvPacket) =>
			{
				EnchantEffect(recvPacket);
			});
		}
		else
		{
			ZItem matItem = ZNet.Data.Me.CurCharData.GetInvenItemUsingMaterial(MaterialSlot.item_Tid);

			CloseBtn.gameObject.SetActive(false);
			ZWebManager.Instance.WebGame.REQ_EnchantItem(EnchantType, new List<ZItem>() { EnhanceItem }, matItem.item_id, matItem.item_tid, (recv, recvPacket) =>
			{
				EnchantEffect(recvPacket);
			});
		}
	}

	void EnchantEffect(WebNet.ResItemEnchant recvPacket)
	{
		CloseBtn.gameObject.SetActive(true);
		UpdateCost();
		UpdateEnchantInfo();

		// 안전 인첸트 유무
		bool bSafeEnchant = DBItem.GetMaxSafeEnchantStep(recvPacket.ResultEnchant(0).Value.FromItemTid) != 0 && DBItem.GetMaxSafeEnchantStep(recvPacket.ResultEnchant(0).Value.FromItemTid) >= DBItem.GetEnchantStep(recvPacket.ResultEnchant(0).Value.FromItemTid);
		//-------------------------------------------------
		if (false == recvPacket.ResultEnchant(0).Value.Result)
		{
			EnchantResultType = E_EnchantResultType.Fail;
		}
		else
		{
			// to do : 멀티 강화가 추가되면 변경해야함
			for (int i = 0; i < recvPacket.ResultEquipItemsLength; i++)
				EnhanceItem = new ZItem(recvPacket.ResultEquipItems(i).Value);

			var fromStep = DBItem.GetEnchantStep(recvPacket.ResultEnchant(0).Value.FromItemTid);
			var toStep = DBItem.GetEnchantStep(recvPacket.ResultEnchant(0).Value.ToItemTid);

			if (toStep < fromStep)
				EnchantResultType = E_EnchantResultType.DownSuccess;
			else if (toStep - fromStep > 1)
				EnchantResultType = E_EnchantResultType.BigSuccess;
			else
				EnchantResultType = E_EnchantResultType.Success;
		}

		this.SafeStartCoroutine(ShowDirection(bSafeEnchant));
	}

	/// <summary>
	/// 슬롯들 가운데로 모으거나 원위치 트윈
	/// </summary>
	/// <param name="isMoveCenter">가운데로 모이나?</param>
	/// <returns></returns>
	IEnumerator CoTweenSlot(bool isMoveCenter)
	{
		var originPosEquip = isMoveCenter ? tweenObjEquipSlot.originPos.localPosition : tweenObjEquipSlot.destPos.localPosition;
		var destPosEquip = isMoveCenter ? tweenObjEquipSlot.destPos.localPosition : tweenObjEquipSlot.originPos.localPosition;

		var originPosMat = isMoveCenter ? tweenObjMatSlot.originPos.localPosition : tweenObjMatSlot.destPos.localPosition;
		var destPosMat = isMoveCenter ? tweenObjMatSlot.destPos.localPosition : tweenObjMatSlot.originPos.localPosition;

		tweenObjMatSlot.objTarget.transform.localPosition = originPosMat;
		tweenObjEquipSlot.objTarget.transform.localPosition = originPosEquip;

		tweenObjMatSlot.objTarget.transform.DOLocalMoveX(destPosMat.x, ObjCollapseTweeningDuration).SetEase(Ease.OutBounce);
		tweenObjEquipSlot.objTarget.transform.DOLocalMoveX(destPosEquip.x, ObjCollapseTweeningDuration).SetEase(Ease.OutBounce);

		yield return new WaitForSeconds(ObjCollapseTweeningDuration);
	}

	IEnumerator ShowDirection(bool _bSafeEnchant)
	{
		fxInteractableGroup.SetInteractable(false);
		yield return CoTweenSlot(true);

		float waitDuration = 0f;

		if (_bSafeEnchant == true)
		{// 안전강화라면 기본이펙트 출력
			switch (EnchantResultType)
			{
				case E_EnchantResultType.Success:
					waitDuration = PlayFx(E_SafeFxIndex.Success);
					break;
				case E_EnchantResultType.BigSuccess:
					waitDuration = PlayFx(E_SafeFxIndex.SuccessHigher);
					break;
				case E_EnchantResultType.DownSuccess:
					waitDuration = PlayFx(E_SafeFxIndex.Degrade);
					break;
				case E_EnchantResultType.Fail:
					waitDuration = PlayFx(E_SafeFxIndex.Fail);
					break;
			}
			yield return new WaitForSeconds(waitDuration);
		}
		else
		{
			UIPopupEnhanceDanger popup = null;

			UIManager.Instance.Open<UIPopupEnhanceDanger>((str, frame) =>
			{
				popup = frame;
				popup.PlayFX(ItemOrigin, EnchantResultType);
			});

			yield return new WaitUntil(() => popup != null && popup.IsDirectionEnd);
		}

		// 연출 원래대로 돌림

		ResetDirection();

		yield return CoTweenSlot(false);

		fxInteractableGroup.SetInteractable(true);

		UpdateSelectUI();
		UpdateSmartButtons();

		if(EnchantResultType == E_EnchantResultType.Fail)
		{
			UpdateEnhanceGage();
			CheckEnhanceStat();
		}
		else
        {
			UpdateEnhanceUI();
        }

		if (_bSafeEnchant && SmartToggle.isOn && SmartEnhanceCount > DBItem.GetEnchantStep(EnhanceItemSlot.item_Tid))
		{
			StartEnhance();
		}
		else if (!_bSafeEnchant)
		{
			MultiEnhanceBox.SetActive(false);
			SmartButtonCheckOn.SetActive(false);
			SmartButtonDefault.SelectToggle();
		}
	}

	/// <summary>
	/// 이펙트 재생
	/// </summary>
	/// <param name="index"></param>
	/// <returns>duration</returns>
	private float PlayFx(E_SafeFxIndex index)
	{
		int idx = (int)index;
		if (listSafeFx.Count <= idx)
			return 0f;

		listSafeFx[idx].Play();
		return listSafeFx[idx].duration;
	}

	private void StopFx(E_SafeFxIndex index)
	{
		int idx = (int)index;
		if (listSafeFx.Count <= idx)
			return;

		listSafeFx[idx].Stop();
	}

	// 이펙트 초기화, 슬롯 제자리(혹시모르니)
	private void ResetDirection()
	{
		listSafeFx.ForEach(item => item.Stop());

		tweenObjMatSlot.objTarget.transform.localPosition = tweenObjMatSlot.originPos.localPosition;
		tweenObjEquipSlot.objTarget.transform.localPosition = tweenObjEquipSlot.originPos.localPosition;
	}

	private void UpdateSelectUI()
	{
		for (int i = 0; i < Me.CurCharData.InvenList.Count; i++)
		{
			if (Me.CurCharData.InvenList[i].GetUniqueKey == ItemUniqueKey)
			{
				UICommon.SetNoticeMessage("강화에 성공하여 [" + DBLocale.GetText(DBItem.GetItem(Me.CurCharData.InvenList[i].item_tid).ItemTextID) + "]가 획득하었습니다.", Color.red, 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
				EnhanceItemSlot.SetItem(Me.CurCharData.InvenList[i]);
				return;
			}
		}

		UICommon.SetNoticeMessage("강화 실패로 인해 [" + DBLocale.GetText(DBItem.GetItem(EnhanceItem.item_tid).ItemTextID) + "]가 파괴되었습니다.", Color.red, 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
		UIManager.Instance.Find<UIFrameInventory>().ScrollAdapter.SetData();
		EnhanceItemSlot.SetEmpty();
		UpdateEnhanceUI();
	}

	public void OnClickSmartEnhanceCount(int _idx)
	{
		SmartEnhanceCount = (uint)_idx;
	}

	/// <summary>스마트 강화 버튼 콜백 함수 </summary>
	public void OnClickSmartEnhanceItem(bool _check = false)
	{
		if (EnhanceItem == null || !_check)
			return;

		var item = DBItem.GetItem(EnhanceItem.item_tid);

		if (EnhanceItem == null ||
			item.EquipSlotType == GameDB.E_EquipSlotType.Earring ||
			item.EquipSlotType == GameDB.E_EquipSlotType.Bracelet ||
			item.EquipSlotType == GameDB.E_EquipSlotType.Ring ||
			item.EquipSlotType == GameDB.E_EquipSlotType.Necklace ||
			item.EquipSlotType == GameDB.E_EquipSlotType.Cape)
		{
			UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
			{
				_popup.Open(ZUIString.WARRING, DBLocale.GetText("장신구는 스마트 강화가 불가능합니다."),
				new string[] { ZUIString.LOCALE_OK_BUTTON },
				new Action[] { delegate
					{
						MultiEnhanceBox.SetActive(false);
						SmartToggle.isOn = false;
						SmartButtonDefault.SelectToggle();
						_popup.Close();
					}
				});
			});
			return;
		}

		bool bSafeEnchant = DBItem.GetMaxSafeEnchantStep(EnhanceItem.item_tid) != 0 && DBItem.GetMaxSafeEnchantStep(EnhanceItem.item_tid) >= DBItem.GetEnchantStep(EnhanceItem.item_tid);
		if (!bSafeEnchant)
		{
			UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
			{
				_popup.Open(ZUIString.WARRING, DBLocale.GetText("안전 강화 구간이 아닙니다."),
				new string[] { ZUIString.LOCALE_OK_BUTTON },
				new Action[] { delegate
					{
						MultiEnhanceBox.SetActive(false);
						SmartToggle.isOn = false;
						SmartButtonDefault.SelectToggle();
						_popup.Close();
					}
				});
			});
			return;
		}

		if (SmartToggle.isOn)
		{
			SmartButtonCheckOn.SetActive(true);
			MultiEnhanceBox.SetActive(true);
			UpdateSmartButtons();
		}
		else
		{
			SmartButtonCheckOn.SetActive(false);
			MultiEnhanceBox.SetActive(false);
			SmartButtonDefault.SelectToggle();
		}
	}

	private void UpdateSmartButtons()
	{
		if (EnhanceItem == null)
			return;

		for (int i = 0; i < SmartBoard.Count; i++)
		{
			SmartBoard[i].SetActive(true);
			SmartButton[i].SetActive(false);
		}

		var maxSmartNum = 0;
		if (DBItem.GetItem(EnhanceItem.item_tid).Step >= 5)
			maxSmartNum = 5;
		else
			maxSmartNum = DBItem.GetItem(EnhanceItem.item_tid).Step;

		for (int i = SmartBoard.Count - 1; i >= maxSmartNum; i--)
		{
			SmartBoard[i].SetActive(false);
		}
		for (int i = 0; i <= DBItem.GetMaxSafeEnchantStep(EnhanceItem.item_tid); i++)
		{
			SmartButton[i].SetActive(true);
		}
	}

	public void Close()
	{
		UIManager.Instance.Close<UIFrameItemEnhance>();
	}
}

using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class UIFrameItemMake : ZUIFrameBase
{
	#region External

	#endregion

	#region Variable
	public E_MakeType SelectMakeType = E_MakeType.Weapon;
	public E_MakeTapType SelectMakeSubType = E_MakeTapType.Weapon_Knight;
	public E_CharacterType SelectMakeClassType = E_CharacterType.All;

	public Image MakeItemIcon = null;
	public Text MakeCount = null;

	[SerializeField] private ZToggle[] TopTab;
	[SerializeField] private GameObject CountObject;
	[SerializeField] private GameObject[] EffectMakeItem = new GameObject[ZUIConstant.ITEM_GRADE_COUNT];

	public ZButton MakeBtn;

	[SerializeField] private Text ClassMenuTxt;

	[SerializeField] private ScrollRect SubTabListScrollView;
	[SerializeField] private ScrollRect ItemMakeMaterialListScrollView;
	[SerializeField] private ScrollRect ItemMakeOptionListScrollView;

	[SerializeField] private UIItemMakeListAdapter MakeItemListScrollAdapter = null;
	[SerializeField] private List<UIItemMakeMaterialListItem> MaterialList = new List<UIItemMakeMaterialListItem>();
	[SerializeField] private List<UIItemMakeOptionListItem> OptionList = new List<UIItemMakeOptionListItem>();
	[SerializeField] private List<UIItemMakeSubTabListItem> SubTabList = new List<UIItemMakeSubTabListItem>();

	[SerializeField] private GameObject MaterialInfoObj = null;
	[SerializeField] private Text NotSelectItemAlarm = null;
	[SerializeField] private Text MakeItemName = null;
	[SerializeField] private Text MakeItemClassName = null;
	[SerializeField] private Image MakeItemClassIcon = null;
	public uint SelectMakeTid { get; private set; } = 0;
	public override bool IsBackable => true;
	private bool isInit = false;
	#endregion


	protected override void OnInitialize()
	{
		base.OnInitialize();

		ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIItemMakeMaterialListItem), delegate
		{
			ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIItemMakeOptionListItem), delegate
			{
				ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIItemMakeListHolder), delegate
				{
					ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIItemMakeSubTabListItem), delegate
					{
						isInit = true;

						Initialize();
					});
				});
			});
		});
	}

	protected override void OnRemove()
	{
		base.OnRemove();

		ZPoolManager.Instance.Clear(E_PoolType.UI, nameof(UIItemMakeMaterialListItem));
		ZPoolManager.Instance.Clear(E_PoolType.UI, nameof(UIItemMakeOptionListItem));
		ZPoolManager.Instance.Clear(E_PoolType.UI, nameof(UIItemMakeListHolder));
		ZPoolManager.Instance.Clear(E_PoolType.UI, nameof(UIItemMakeSubTabListItem));
	}

	protected override void OnShow(int _LayerOrder)
	{
		base.OnShow(_LayerOrder);

		if (UIManager.Instance.Find(out UIFrameHUD _hud))
			_hud.SetSubHudFrame(E_UIStyle.FullScreen);

		Initialize();
	}

    protected override void OnHide()
    {
        base.OnHide();

		if (UIManager.Instance.Find(out UIFrameHUD _hud))
			_hud.SetSubHudFrame();
	}

    private void Initialize()
	{
		if (!isInit)
			return;

		ClearMakeData();

		SelectMakeType = E_MakeType.Weapon;
		SelectMakeSubType = E_MakeTapType.Weapon_Knight;
		SelectMakeClassType = E_CharacterType.All;

		NotSelectItemAlarm.gameObject.SetActive(true);
		MaterialInfoObj.SetActive(false);

		TopTab[(int)SelectMakeType].SelectToggleAction((ZToggle _toggle) => {
			OnSelectMainTab(Convert.ToInt32(SelectMakeType));
		});
	}

	#region Input
	public void OnSelectMainTab(int _idx)
	{
		SelectMakeType = (E_MakeType)_idx;

		SetSubTab();

		OnSelectSubTab(SelectMakeSubType);
	}

	public void OnSelectSubTab(E_MakeTapType _tabIndex)
	{
		SelectMakeSubType = _tabIndex;

		for (int i = 0; i < SubTabList.Count; i++)
			if(SubTabList[i].Type == _tabIndex)
				SubTabList[i].Bg.color = new Color(255, 255, 255, 255);
			else
				SubTabList[i].Bg.color = new Color(0, 0, 0, 255);

		OnSelectClassTab(Convert.ToInt32(SelectMakeClassType));
	}

	public void OnSelectClassTab(int _idx)
	{
		ClearMakeData();

		MaterialInfoObj.SetActive(false);
		NotSelectItemAlarm.gameObject.SetActive(true);

		SelectMakeClassType = (E_CharacterType)_idx;

		ClassMenuTxt.text = DBLocale.GetText(SelectMakeClassType.ToString());

		SetMakeList();
	}

	private void ClaerMakeSubTabList()
	{
		for (int i = 0; i < SubTabList.Count; i++)
			Destroy(SubTabList[i].gameObject);

		SubTabList.Clear();
	}

	private void SetSubTab()
	{
		ClaerMakeSubTabList();

		int idx = 0;
		foreach (E_MakeTapType tabType in DBMake.GetMakeTabTypes(SelectMakeType))
		{
			UIItemMakeSubTabListItem obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIItemMakeSubTabListItem)).GetComponent<UIItemMakeSubTabListItem>();

			if (obj != null)
			{
				obj.transform.SetParent(SubTabListScrollView.content, false);
				obj.Initialize(tabType);
				SubTabList.Add(obj);

				if (idx == 0)
					SelectMakeSubType = tabType;

				idx++;
			}
		}
	}

	private void SetMakeList()
	{
		MakeItemListScrollAdapter.SetData();
	}

	public void OnSelectMakeItem(Make_Table _selectItem)
	{
		ClearMakeData();

		var table = DBItem.GetItem(_selectItem.SuccessGetItemID);

		MaterialInfoObj.SetActive(_selectItem != null);
		NotSelectItemAlarm.gameObject.SetActive(_selectItem == null);
		MakeItemName.text = _selectItem != null ? UICommon.GetItemText(table) : string.Empty;
		MakeItemClassName.text = _selectItem != null ? DBLocale.GetText(table.UseCharacterType.ToString()) : string.Empty;
		MakeItemClassIcon.sprite = UICommon.GetClassIconSprite(table.UseCharacterType, UICommon.E_SIZE_OPTION.Small);

		SelectMakeTid = _selectItem.MakeID;

		MakeBtn.interactable = true;

		for (int i = 0; i < MakeItemListScrollAdapter.Data.List.Count; i++)
			MakeItemListScrollAdapter.Data.List[i].isSelect = false;

		ClearEffect();

		var selectItem = MakeItemListScrollAdapter.Data.List.Find(item => item.Item.SuccessGetItemID == _selectItem.SuccessGetItemID);

		if(selectItem != null)
		{
			selectItem.isSelect = true;
			SetMaterialList(_selectItem);

			var itemData = DBItem.GetItem(_selectItem.SuccessGetItemID);

			EffectMakeItem[itemData.Grade - 1].SetActive(true);

			List<UICommon.ItemDescInfo> itemDesc = new List<UICommon.ItemDescInfo>
			{
				new UICommon.ItemDescInfo() { strDesc = "등급", Value = DBLocale.GetText("Tier" + itemData.Grade.ToString() + "_Text") },
				new UICommon.ItemDescInfo() { strDesc = "클래스", Value = DBLocale.GetText(itemData.UseCharacterType.ToString()) },
				new UICommon.ItemDescInfo() { strDesc = "무게", Value = DBLocale.GetText(itemData.Weight.ToString()) },
				new UICommon.ItemDescInfo() { strDesc = string.Empty, Value = string.Empty },
				new UICommon.ItemDescInfo() { strDesc = "효과", Value = string.Empty }
			};

			var itemStatus = UICommon.GetItemDesc(_selectItem.SuccessGetItemID);
			if (itemStatus != null) itemDesc.AddRange(itemStatus); 

			for(int i = 0; i < itemDesc.Count; i++)
			{
				UIItemMakeOptionListItem obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIItemMakeOptionListItem)).GetComponent<UIItemMakeOptionListItem>();

				if (obj != null)
				{
					obj.transform.SetParent(ItemMakeOptionListScrollView.content, false);
					obj.Initialize(itemDesc[i].strDesc, itemDesc[i].Value);
					OptionList.Add(obj);
				}
			}
		}

		CountObject.SetActive(selectItem != null);

		MakeItemIcon.sprite = selectItem != null ? UICommon.GetItemIconSprite(selectItem.Item.SuccessGetItemID) : null;
		MakeItemIcon.gameObject.SetActive(selectItem != null);

		MakeItemListScrollAdapter.RefreshData();
	}

	private void ClearEffect()
	{
		for (int i = 0; i < EffectMakeItem.Length; i++)
			EffectMakeItem[i].SetActive(false);
	}

	public void OnResetMakeCount()
	{
		DrawMaterialCount(1);
		MakeCount.text = 1.ToString();
	}

	public void OnChangeMakeCount(int _cnt)
	{
		int craftCnt = Convert.ToInt32(MakeCount.text);

		if (_cnt > 0 && CheckMakeMaxCount() < Convert.ToUInt64(craftCnt + _cnt))
		{
			UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
				_popup.Open(ZUIString.WARRING, DBLocale.GetText("WProduct_Make_NoMatterial"), new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate {
					_popup.Close(); } });
			});
			return;
		}

		if (_cnt < 0 && craftCnt == 1)
			return;

		DrawMaterialCount(Convert.ToUInt64(_cnt < 0 && craftCnt == 0 ? 0 : craftCnt + _cnt));
		MakeCount.text = _cnt < 0 && craftCnt == 0 ? 0.ToString() : (craftCnt + _cnt).ToString();
	}

	public void OnChangeMakeCountTen()
	{
		ulong craftCnt = Convert.ToUInt64(MakeCount.text);
		ulong checkMaterialCnt = CheckMakeMaxCount();
		ulong makeValue = 0;

		if (checkMaterialCnt == 0)
		{
			UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
				_popup.Open(ZUIString.WARRING, DBLocale.GetText("WProduct_Make_NoMatterial"), new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate {
					_popup.Close(); } });
			});
			return;
		}
		else if (checkMaterialCnt < craftCnt + 10)
        {
			UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
				_popup.Open(ZUIString.WARRING, DBLocale.GetText("WProduct_Make_NoMatterial"), new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate {
					_popup.Close(); } });
			});
			return;
		}

		makeValue = craftCnt + 10;

		DrawMaterialCount(makeValue);
		MakeCount.text = makeValue.ToString();
	}

	public void OnChangeMakeCountMax()
	{
		ulong craftCnt = Convert.ToUInt64(MakeCount.text);
		ulong checkMaterialCnt = CheckMakeMaxCount();

		if (checkMaterialCnt == 0)
		{
			UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
				_popup.Open(ZUIString.WARRING, DBLocale.GetText("WProduct_Make_NoMatterial"), new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate {
					_popup.Close(); } });
			});
			return;
		}
		else if (checkMaterialCnt < craftCnt + 1)
		{
			UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
				_popup.Open(ZUIString.WARRING, DBLocale.GetText("WProduct_Make_NoMatterial"), new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate {
					_popup.Close(); } });
			});
			return;
		}

		DrawMaterialCount(checkMaterialCnt);
		MakeCount.text = checkMaterialCnt.ToString();
	}

	public void OnStartMake()
	{
		if (SelectMakeTid == 0 || Convert.ToUInt32(MakeCount) == 0)
			return;

		if (CheckMakeMaxCount() == 0)
		{
			UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
			{
				_popup.Open(ZUIString.WARRING, DBLocale.GetText("재료가 부족합니다."), new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate {
					_popup.Close(); } });
			});
			return;
		}

		List<List<ZItem>> matItems = new List<List<ZItem>>();

		for (int i = 0; i < MaterialList.Count; i++)
		{
			matItems.Add(new List<ZItem>(Me.CurCharData.GetAllInvenItemsUsingMaterial(MaterialList[i].Item.ItemID, MaterialList[i].ItemCount)));
		}

		ZWebManager.Instance.WebGame.REQ_MakeItem(SelectMakeTid, Convert.ToUInt32(MakeCount.text), matItems, (recvPacket, msg) =>
		{
			if (recvPacket.ErrCode == WebNet.ERROR.NO_ERROR)
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

				OnSelectMakeItem(DBMake.GetMakeData(SelectMakeTid));
				OnResetMakeCount();
			}
		});
	}
	#endregion

	private void DrawMaterialCount(ulong _makeCnt)
	{
		for (int i = 0; i < MaterialList.Count; i++)
			MaterialList[i].SetCountText(_makeCnt);
	}




	//-------------------------------------------------------------------------------
	private ulong CheckMakeMaxCount()
	{
		ulong maxCnt = 0;

		for (int i = 0; i < MaterialList.Count; i++)
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

	private bool CheckCost()
	{
		foreach (var iter in MaterialList)
		{
			if (ConditionHelper.CheckCompareCost(iter.Item.ItemID, iter.ItemCount) == false)
				return false;
		}
		return true;
	}











	private void ClearMakeData()
	{
		SelectMakeTid = 0;

		MakeBtn.interactable = false;

		CountObject.SetActive(false);

		MakeItemIcon.gameObject.SetActive(false);
		MakeCount.text = 1.ToString();

		ClearMakeMaterialListData();
		ClearOptionListData();
	}

	private void ClearMakeMaterialListData()
	{
		for (int i = 0; i < MaterialList.Count; i++)
			Destroy(MaterialList[i].gameObject);

		MaterialList.Clear();
	}

	private void ClearOptionListData()
	{
		for (int i = 0; i < OptionList.Count; i++)
			Destroy(OptionList[i].gameObject);

		OptionList.Clear();
	}

	private void SetMaterialList(Make_Table _table)
	{
		if (_table == null)
			return;

		ClearMakeMaterialListData();

		var material = DBMake.GetMakeData(_table.MakeID);

		if (material != null)
		{
			if (material.MaterialItemID_01.Count != 0) createMaterial(material.MaterialItemID_01[0], material.MaterialItemCount_01[0]);
			if (material.MaterialItemID_02.Count != 0) createMaterial(material.MaterialItemID_02[0], material.MaterialItemCount_02[0]);
			if (material.MaterialItemID_03.Count != 0) createMaterial(material.MaterialItemID_03[0], material.MaterialItemCount_03[0]);
			if (material.MaterialItemID_04.Count != 0) createMaterial(material.MaterialItemID_04[0], material.MaterialItemCount_04[0]);
			if (material.MaterialItemID_05.Count != 0) createMaterial(material.MaterialItemID_05[0], material.MaterialItemCount_05[0]);
		}

		void createMaterial(uint _makeItemId, uint _makeCnt)
		{
			UIItemMakeMaterialListItem obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIItemMakeMaterialListItem)).GetComponent<UIItemMakeMaterialListItem>();

			if (obj != null)
			{
				obj.transform.SetParent(ItemMakeMaterialListScrollView.content, false);
				obj.Initialize(DBItem.GetItem(_makeItemId), _makeCnt);
				MaterialList.Add(obj);
			}
		}
	}

	public void OnClose()
	{
		UIManager.Instance.Close<UIFrameItemMake>();
		
		Initialize();
	}
	//-------------------------------------------------------------------------------------

	protected virtual void Initialize(ZUIFrameBase _frame) { }
}
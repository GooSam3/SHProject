using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class UIFrameCook : ZUIFrameBase
{
	#region External

	#endregion

	#region Variable
	public UICookEnum.E_CookState SelectCookType = UICookEnum.E_CookState.Combine;

	public Image CookIcon = null;
	public Text CookCount = null;

	[SerializeField] private UICookResultPopup ResultPopup;

	[SerializeField] private ZToggle[] TopTab;

	[SerializeField] private GameObject[] CookTabObject = new GameObject[ZUIConstant.COOK_STATE_INDEX_COUNT];
	[SerializeField] private GameObject[] EffectMakeItem = new GameObject[ZUIConstant.ITEM_GRADE_COUNT];

	[SerializeField] private Text MaterialCount;
	[SerializeField] private Text RecipeMakeCost;

	[SerializeField] private ZButton CookBtn;

	[SerializeField] private ScrollRect CookSelectMaterialListScrollView;
	[SerializeField] private ScrollRect CookMaterialListScrollView;
	[SerializeField] private ScrollRect CookOptionListScrollView;

	public UICookInvenMaterialAdapter MaterialListScrollAdapter = null;
	public UICookRecipeListAdapter RecipeListScrollAdapter = null;

	[SerializeField] private List<UICookSelectMaterialListItem> SelectMaterialList = new List<UICookSelectMaterialListItem>();
	[SerializeField] private List<UICookMaterialListItem> MaterialList = new List<UICookMaterialListItem>();
	[SerializeField] private List<UICookOptionListItem> OptionList = new List<UICookOptionListItem>();
	public uint SelectCookTid { get; private set; } = 0;
	public override bool IsBackable => true;
	private bool isInit = false;
	#endregion

	protected override void OnInitialize()
	{
		base.OnInitialize();

		ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UICookMaterialListItem), delegate {
			ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UICookOptionListItem), delegate {
				ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UICookSelectMaterialListItem), delegate {
					ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UICookInvenViewsHolder), delegate {
						ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UICookRecipeListViewsHolder), delegate {
							isInit = true;
							SelectCookType = UICookEnum.E_CookState.Combine;
							Initialize();
						}); 
					}); 
				}); 
			}); 
		});
	}

	protected override void OnRemove()
	{
		base.OnRemove();

		ZPoolManager.Instance.Clear(E_PoolType.UI, nameof(UICookMaterialListItem));
		ZPoolManager.Instance.Clear(E_PoolType.UI, nameof(UICookOptionListItem));
		ZPoolManager.Instance.Clear(E_PoolType.UI, nameof(UICookSelectMaterialListItem));
		ZPoolManager.Instance.Clear(E_PoolType.UI, nameof(UICookInvenViewsHolder));
		ZPoolManager.Instance.Clear(E_PoolType.UI, nameof(UICookRecipeListViewsHolder));
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

		SelectCookType = UICookEnum.E_CookState.Combine;
		TopTab[(int)SelectCookType].SelectToggle();
		OnSelectMainTab((int)SelectCookType);
	}

	#region Input
	public void OnSelectMainTab(int _idx)
	{
		SelectCookType = (UICookEnum.E_CookState)_idx;

		for (int i = 0; i < CookTabObject.Length; i++)
			CookTabObject[i].SetActive(i == _idx);

		switch(SelectCookType)
		{
			case UICookEnum.E_CookState.Combine:
				if(!MaterialListScrollAdapter.IsInitialized)
					MaterialListScrollAdapter.Initialize();
				
				MaterialListScrollAdapter.SetData();

				ClearSelectMaterialSlot();

				MaterialCount.text = MaterialListScrollAdapter.Data.List.Count.ToString() + "/" + Me.CurCharData.InvenMaxCnt.ToString();
				break;

			case UICookEnum.E_CookState.Recipe:
				if (!RecipeListScrollAdapter.IsInitialized)
					RecipeListScrollAdapter.Initialize();

				RecipeListScrollAdapter.SetData();

				ClearMakeMaterialListData();
				break;
		}
	}

	/// <summary>레시피 요리 선택 </summary>
	/// <param name="_selectItem"></param>
	public void OnSelectCook(Cooking_Table _selectItem)
	{
		ClearCookData();

		SelectCookTid = SelectCookType == UICookEnum.E_CookState.Recipe ? _selectItem.CookingID : 0;

		for (int i = 0; i < RecipeListScrollAdapter.Data.List.Count; i++)
			RecipeListScrollAdapter.Data.List[i].isSelect = false;

		ClearEffect();

		var selectItem = RecipeListScrollAdapter.Data.List.Find(item => item.CookTid == _selectItem.CookingID);

		if (selectItem != null)
		{
			selectItem.isSelect = true;
			SetMaterialList(_selectItem);
			RecipeMakeCost.text = DBCooking.Get(selectItem.CookTid).GoldCount.ToString();

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

			for (int i = 0; i < itemDesc.Count; i++)
			{
				UICookOptionListItem obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UICookOptionListItem)).GetComponent<UICookOptionListItem>();

				if (obj != null)
				{
					obj.transform.SetParent(CookOptionListScrollView.content, false);
					obj.Initialize(itemDesc[i].strDesc, itemDesc[i].Value);
					OptionList.Add(obj);
				}
			}
		}

		CookBtn.interactable = selectItem != null;

		CookIcon.sprite = selectItem != null ? UICommon.GetItemIconSprite(DBCooking.Get(selectItem.CookTid).SuccessGetItemID) : null;
		CookIcon.gameObject.SetActive(selectItem != null);

		RecipeListScrollAdapter.RefreshData();
	}

	public void OnSelectCombineCook(ZItem _item)
	{
		if (_item == null)
			return;

		var holderData = MaterialListScrollAdapter.Data.List.Find(item => item.Item.item_tid == _item.item_tid);

		if (holderData != null)
		{
			holderData.isSelect = true;
			MaterialListScrollAdapter.RefreshData();
		}
			
		var data = SelectMaterialList.Find(item => item.Item != null && item.Item.item_tid == _item.item_tid);

		if (data != null)
			return;

		for (int i = 0; i < SelectMaterialList.Count; i++)
			if (SelectMaterialList[i].Item == null)
			{
				SelectMaterialList[i].Initialize(_item);
				break;
			}

		SortSelectMaterialSlot();

		if(MoveSelectMaterialScroll() > 4)
			CookSelectMaterialListScrollView.verticalNormalizedPosition = 0;
	}

	public void OnDeselectCombineCook(ZItem _item)
	{
		for(int i = 0; i < SelectMaterialList.Count; i++)
		{
			if(SelectMaterialList[i].Item != null && SelectMaterialList[i].Item.item_tid == _item.item_tid)
			{
				SelectMaterialList[i].Initialize(null);
				break;
			}
		}

		SortSelectMaterialSlot();

		if(MoveSelectMaterialScroll() <= 4)
			CookSelectMaterialListScrollView.verticalNormalizedPosition = 1;
	}

	private uint MoveSelectMaterialScroll()
	{
		uint itemSelectCnt = 0;
		for (int i = 0; i < SelectMaterialList.Count; i++)
			if (SelectMaterialList[i].Item != null)
				itemSelectCnt++;

		return itemSelectCnt;
	}

	public void OnResetCookCount()
	{
		DrawMaterialCount(1);
		CookCount.text = 1.ToString();
	}

	public void OnChangeCookCount(int _cnt)
	{
		int craftCnt = Convert.ToInt32(CookCount.text);

		if (_cnt > 0 && CheckMakeMaxCount() < Convert.ToUInt64(craftCnt + _cnt))
		{
			UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
			{
				_popup.Open(ZUIString.WARRING, DBLocale.GetText("WProduct_Make_NoMatterial"), new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate {
					_popup.Close(); } });
			});
			return;
		}

		if (_cnt < 0 && craftCnt == 1)
			return;

		DrawMaterialCount(Convert.ToUInt64(_cnt < 0 && craftCnt == 0 ? 0 : craftCnt + _cnt));
		CookCount.text = _cnt < 0 && craftCnt == 0 ? 0.ToString() : (craftCnt + _cnt).ToString();
	}

	public void OnChangeCookCountTen()
	{
		ulong craftCnt = Convert.ToUInt64(CookCount.text);
		ulong checkMaterialCnt = CheckMakeMaxCount();
		ulong makeValue = 0;

		if (checkMaterialCnt == 0)
		{
			UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
			{
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
		CookCount.text = makeValue.ToString();
	}

	public void OnChangeCookCountMax()
	{
		ulong craftCnt = Convert.ToUInt64(CookCount.text);
		ulong checkMaterialCnt = CheckMakeMaxCount();

		if (checkMaterialCnt == 0)
		{
			UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
			{
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
		CookCount.text = checkMaterialCnt.ToString();
	}

	public void OnStartCookCombine()
	{
		List<List<ZItem>> matItems = new List<List<ZItem>>();

		var list = GetSortSelectMaterialList();

		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].Item != null)
				matItems.Add(new List<ZItem>(Me.CurCharData.GetAllInvenItemsUsingMaterial(SelectMaterialList[i].Item.item_tid, SelectMaterialList[i].Item.cnt)));
		}

		if (matItems.Count <= 1)
		{
			UICommon.OpenSystemPopup_One(ZUIString.WARRING,
					  "재료가 2개 이상 필요합니다.", ZUIString.LOCALE_OK_BUTTON);
			return;
		}

		CheckCookEnable(matItems, list);
	}

	private List<UICookSelectMaterialListItem> GetSortSelectMaterialList()
	{
		List<UICookSelectMaterialListItem> sortMaterialList = SelectMaterialList;

		sortMaterialList.Sort((x, y) => {
			if (x.Item == null && y.Item != null) return 1;
			return 0;
		});

		return sortMaterialList;
	}

	private Cooking_Table GetRecipeInfo(List<UICookSelectMaterialListItem> sortlist)
	{
		DBCooking.GetAllTable(out List<Cooking_Table> cookAllTable);
		Cooking_Table recipe = null;
		for (int i = 0; i < cookAllTable.Count; i++)
		{
			// 재료는 최소 2개 이상 있어야 레시피 조합이 가능
			if (sortlist[0].Item == null || cookAllTable[i].MaterialItemID_1 != sortlist[0].Item.item_tid) continue;
			if (sortlist[1].Item == null || cookAllTable[i].MaterialItemID_2 != sortlist[1].Item.item_tid) continue;
			if ((sortlist[2].Item != null && cookAllTable[i].MaterialItemID_3 != sortlist[2].Item.item_tid) || (sortlist[2].Item == null && cookAllTable[i].MaterialItemID_3 != 0)) continue;
			if ((sortlist[3].Item != null && cookAllTable[i].MaterialItemID_4 != sortlist[3].Item.item_tid) || (sortlist[3].Item == null && cookAllTable[i].MaterialItemID_4 != 0)) continue;
			if ((sortlist[4].Item != null && cookAllTable[i].MaterialItemID_5 != sortlist[4].Item.item_tid) || (sortlist[4].Item == null && cookAllTable[i].MaterialItemID_5 != 0)) continue;
			if ((sortlist[5].Item != null && cookAllTable[i].MaterialItemID_6 != sortlist[5].Item.item_tid) || (sortlist[5].Item == null && cookAllTable[i].MaterialItemID_6 != 0)) continue;

			recipe = cookAllTable[i];
			break;
		}

		return recipe;
	}

	private void CheckCookEnable(List<List<ZItem>> matItems, List<UICookSelectMaterialListItem> sortlist = null)
	{
		Cooking_Table recipe = null;
		// 레시피 체크
		switch(SelectCookType)
		{
			case UICookEnum.E_CookState.Combine:
				if (sortlist == null)
					return;

				recipe = GetRecipeInfo(sortlist);
				break;

			case UICookEnum.E_CookState.Recipe:
				recipe = DBCooking.Get(SelectCookTid);
				break;
		}

		if (recipe != null)
		{
			uint cookCount = SelectCookType == UICookEnum.E_CookState.Combine ? 1 : Convert.ToUInt32(CookCount.text);

			ZWebManager.Instance.WebGame.REQ_MakeCookItem(recipe.CookingID, DBConfig.Gold_ID, cookCount, matItems, (recvPacket, msg) =>
			{
				if (recvPacket.ErrCode == WebNet.ERROR.NO_ERROR)
				{
					if (msg.ResultMakeLength > 0)
						ResultPopup.Initialize(msg.ResultMake(0).Value.ItemTid);
					else
						UICommon.OpenSystemPopup_One("제작 결과",
						DBLocale.GetText("Cooking_Fail_Notice"), ZUIString.LOCALE_OK_BUTTON);

					callback();
				}
			});
		}
		else
		{
			ZWebManager.Instance.WebGame.REQ_MakeCookItemNoRecipe(matItems, (recvPacket, Msg) => {
				if (recvPacket.ErrCode == WebNet.ERROR.NO_ERROR)
				{
					UICommon.OpenSystemPopup_One("제작 결과",
						DBLocale.GetText("Cooking_Fail_Notice"), ZUIString.LOCALE_OK_BUTTON);

					callback();
				}
			});
		}

		void callback()
		{
			switch (SelectCookType)
			{
				case UICookEnum.E_CookState.Combine:
					MaterialListScrollAdapter.SetData();
					ClearSelectMaterialSlot();
					break;

				case UICookEnum.E_CookState.Recipe:
					OnSelectCook(DBCooking.Get(SelectCookTid));
					break;
			}
		}

		OnResetCookCount();
	}

	public void OnResetSelectMaterialList()
	{
		ClearSelectMaterialSlot();
	}

	public void OnStartCook()
	{
		if (SelectCookTid == 0 || Convert.ToUInt32(CookCount) == 0)
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

		if(Me.CurCharData.GetItem(DBConfig.Gold_ID,  NetItemType.TYPE_ACCOUNT_STACK).cnt < Convert.ToUInt64(RecipeMakeCost.text))
        {
			UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
			{
				_popup.Open(ZUIString.WARRING, DBLocale.GetText("재화가 부족합니다."), new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate {
					_popup.Close(); } });
			});
			return;
		}

		List<List<ZItem>> matItems = new List<List<ZItem>>();

		for (int i = 0; i < MaterialList.Count; i++)
		{
			matItems.Add(new List<ZItem>(Me.CurCharData.GetAllInvenItemsUsingMaterial(MaterialList[i].Item.ItemID, MaterialList[i].ItemCount)));
		}

		CheckCookEnable(matItems);
	}

	public void OnActivePopup(bool _active)
	{
		ResultPopup.gameObject.SetActive(_active);
	}

	public void OnClose()
	{
		UIManager.Instance.Close<UIFrameCook>();

		Initialize();
	}
	#endregion

	private void DrawMaterialCount(ulong _makeCnt)
	{
		for (int i = 0; i < MaterialList.Count; i++)
			MaterialList[i].SetCountText(_makeCnt);
	}

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

	private void SetMaterialList(Cooking_Table _table)
	{
		if (_table == null)
			return;

		ClearMakeMaterialListData();

		var material = DBCooking.Get(_table.CookingID);

		if (material != null)
		{
			if (material.MaterialItemID_1 != 0) createMaterial(material.MaterialItemID_1);
			if (material.MaterialItemID_2 != 0) createMaterial(material.MaterialItemID_2);
			if (material.MaterialItemID_3 != 0) createMaterial(material.MaterialItemID_3);
			if (material.MaterialItemID_4 != 0) createMaterial(material.MaterialItemID_4);
			if (material.MaterialItemID_5 != 0) createMaterial(material.MaterialItemID_5);
			if (material.MaterialItemID_6 != 0) createMaterial(material.MaterialItemID_6);
		}

		void createMaterial(uint _makeItemId)
		{
			UICookMaterialListItem obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UICookMaterialListItem)).GetComponent<UICookMaterialListItem>();

			if (obj != null)
			{
				obj.transform.SetParent(CookMaterialListScrollView.content, false);
				obj.Initialize(DBItem.GetItem(_makeItemId), 1);
				MaterialList.Add(obj);
			}
		}
	}

	private void ClearCookData()
	{
		SelectCookTid = 0;

		CookBtn.interactable = false;

		CookIcon.gameObject.SetActive(false);
		CookCount.text = 1.ToString();
		RecipeMakeCost.text = 0.ToString();

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

	private void ClearSelectMaterialSlot()
	{
		if (MaterialListScrollAdapter.Data == null)
			return;

		for (int i = 0; i < MaterialListScrollAdapter.Data.Count; i++)
			MaterialListScrollAdapter.Data.List[i].isSelect = false;

		MaterialListScrollAdapter.RefreshData();

		if (SelectMaterialList.Count > 0)
			for (int i = 0; i < SelectMaterialList.Count; i++)
				SelectMaterialList[i].Initialize(null);
		else
			for (int i = 0; i < ZUIConstant.COOK_SELECT_MATERIAL_COUNT; i++)
				CreateSelectMaterialSlot(null);

		CookSelectMaterialListScrollView.verticalNormalizedPosition = 1;
	}

	private void SortSelectMaterialSlot()
	{
		List<ZItem> selectMaterialList = new List<ZItem>();

		for (int i = 0; i < SelectMaterialList.Count; i++)
			if (SelectMaterialList[i].Item != null)
				selectMaterialList.Add(SelectMaterialList[i].Item);

		ClearSelectMaterialSlot();

		for (int i = 0; i < selectMaterialList.Count; i++)
		{
			SelectMaterialList[i].Initialize(selectMaterialList[i]);

			for (int j = 0; j < MaterialListScrollAdapter.Data.List.Count; j++)
				if (MaterialListScrollAdapter.Data.List[j].Item.item_tid == selectMaterialList[i].item_tid)
					MaterialListScrollAdapter.Data.List[j].isSelect = true;
		}

		MaterialListScrollAdapter.RefreshData();
	}

	private void CreateSelectMaterialSlot(ZItem _item)
	{
		UICookSelectMaterialListItem obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UICookSelectMaterialListItem)).GetComponent<UICookSelectMaterialListItem>();

		if (obj != null)
		{
			obj.transform.SetParent(CookSelectMaterialListScrollView.content, false);
			obj.Initialize(_item);
			SelectMaterialList.Add(obj);
		}
	}

	private void ClearEffect()
	{
		for (int i = 0; i < EffectMakeItem.Length; i++)
			EffectMakeItem[i].SetActive(false);
	}
}
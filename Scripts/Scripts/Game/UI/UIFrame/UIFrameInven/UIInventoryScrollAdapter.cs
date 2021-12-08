using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using Com.TheFallenGames.OSA.DataHelpers;
using frame8.Logic.Misc.Other.Extensions;
using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using ZDefine;
using ZNet.Data;

public class UIInventoryScrollAdapter : GridAdapter<MyGridParams, UIInvenViewsHolder>
{
	// 스크롤 데이터 리스트
	public SimpleDataHelper<ScrollInvenData> Data { get; private set; }

	/// <summary>리스트 홀더를 갱신해준다.(스크롤이 Active 또는 Cell Group이 이동될 때(Drag))</summary>
	/// <param name="_holder">홀더 오브젝트</param>
	protected override void UpdateCellViewsHolder(UIInvenViewsHolder _holder)
	{
		if (_holder == null)
			return;

		ScrollInvenData data = Data[_holder.ItemIndex];
		_holder.UpdateTitleByItemIndex(data);
	}
	
	protected override CellGroupViewsHolder<UIInvenViewsHolder> GetNewCellGroupViewsHolder()
	{
		return new MyCellGroupViewsHolder();
	}

	protected override void UpdateViewsHolder(CellGroupViewsHolder<UIInvenViewsHolder> newOrRecycled)
	{
		base.UpdateViewsHolder(newOrRecycled);

		if (newOrRecycled.NumActiveCells > 0)
		{
			var firstCellVH = newOrRecycled.ContainingCellViewsHolders[0];
			int modelIndex = firstCellVH.ItemIndex;
			var model = Data[modelIndex];
			var newOrRecycledCasted = newOrRecycled as MyCellGroupViewsHolder;
			if (model.type == ScrollInvenData.CellType.Expansion)
				newOrRecycledCasted.ShowHeader();
			else
				newOrRecycledCasted.ClearHeader();
		}

		ScheduleComputeVisibilityTwinPass();
	}


	/// <summary>홀더 데이터 리스트 전체 갱신</summary>
	public void RefreshData()
    {
		for(int i = 0; i < base.CellsCount; i++)
			UpdateCellViewsHolder(base.GetCellViewsHolder(i));
	}

	/// <summary>Adapter 초기 세팅 (최초 1회)</summary>
	public void Initialize()
	{
		if (Parameters.Grid.CellPrefab == null)
		{
			GameObject InvenHolder = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIInvenViewsHolder));
			Parameters.Grid.CellPrefab = InvenHolder.GetComponent<RectTransform>();
			Parameters.Grid.CellPrefab.SetParent(GetComponent<Transform>());
			Parameters.Grid.CellPrefab.transform.localScale = Vector2.one;
			Parameters.Grid.CellPrefab.transform.localPosition = Vector3.zero;
			Parameters.Grid.CellPrefab.gameObject.SetActive(false);
			gameObject.SetActive(Parameters.Grid.CellPrefab != null);
		}

		if (!IsInitialized)
			Init();
	}

	private void ClearData()
	{
		Data.List.Clear();
	}

    /// <summary>스크롤 리스트 Setting</summary>
    /// <param name="_callback">스크롤 리스트 데이터 세팅 이후 처리할 콜백 실행.</param>
    public void SetData(Action _callback = null)
	{
		if (Data == null)
			Data = new SimpleDataHelper<ScrollInvenData>(this);

		if (!IsInitialized)
			Initialize();

		if (!UIManager.Instance.Find(out UIFrameInventory _inventory))
			return;

		ClearData();

		#region 사용자 변경 로직
		for (int i = 0; i < Me.CurCharData.InvenList.Count; i++)
		{
			var itemData = DBItem.GetItem(Me.CurCharData.InvenList[i].item_tid);

			// 아이템 적재
			if(itemData != null && itemData.InvenUseType == GameDB.E_InvenUseType.UseInven)
			{
				var data = Data.List.Find(item => item.Item.item_id == Me.CurCharData.InvenList[i].item_id && item.Item.netType == Me.CurCharData.InvenList[i].netType);

				if (data == null && Me.CurCharData.InvenList[i].cnt != 0)
					Data.List.Add(new ScrollInvenData() { Item = Me.CurCharData.InvenList[i] });
				else if(data != null && data.Item.cnt != 0)
					data.Reset(new ScrollInvenData() { Item = Me.CurCharData.InvenList[i] });
			}
		}

		CheckData(_inventory.CurSortType);

		SetExtensionData();


		#endregion

		if (_inventory.GetSelectObject() != null && _inventory.InfoPopup != null)
		{
			var data = Data.List.Find(item => item.Item != null && item.Item.item_id == _inventory.InfoPopup.Item.item_id && item.Item.netType == _inventory.InfoPopup.Item.netType);

			if (data != null)
				data.isSelected = true;
			else
			{
				_inventory.SetSelectObject(null);
				_inventory.RemoveInfoPopup();
			}
		}

		// 데이터 갱신
		Data.NotifyListChangedExternally();

		// 홀더 업데이트
		RefreshData();

		_callback?.Invoke();
	}

	private void CheckData(E_InvenSortType _type)
	{
		List<ZItem> itemList = new List<ZItem>();

		switch (_type)
		{
			case E_InvenSortType.Equipment:
				for (int i = 0; i < Data.List.Count; i++)
					if (Data.List[i].Item.netType != NetItemType.TYPE_EQUIP)
						itemList.Add(Data.List[i].Item);
				break;

			case E_InvenSortType.ETC:
				for (int i = 0; i < Data.List.Count; i++)
					if (Data.List[i].Item.netType == NetItemType.TYPE_EQUIP)
						itemList.Add(Data.List[i].Item);
				break;

			case E_InvenSortType.Disassemble:
				for (int i = 0; i < Data.List.Count; i++)
				{
					var itemBreakData = DBItem.GetItem(Data[i].Item.item_tid);

					if (Data[i].Item.netType != NetItemType.TYPE_EQUIP || Data[i].Item.IsLock || Data[i].Item.slot_idx != 0 || !itemBreakData.LimitType.HasFlag(E_LimitType.Break))
					{
						itemList.Add(Data.List[i].Item);
						continue;
					}

					// 분해창에 등록된 아이템인 경우
					if(UIManager.Instance.Find(out UIFrameItemDisassemble _disassemble) && _disassemble.ScrollAdapter.Data != null)
					{
						var data = _disassemble.ScrollAdapter.Data.List.Find(item => item.Item.item_id == Data[i].Item.item_id && item.Item.item_tid == Data[i].Item.item_tid);
					
						if(data != null)
						{
							itemList.Add(Data.List[i].Item);
							continue;
						}	
					}
				}
				break;

			case E_InvenSortType.Enhance:
				if (!UIManager.Instance.Find(out UIFrameItemEnhance _enhance))
					return;

				for (int i = 0; i < Data.List.Count; i++)
				{
					var itemData = DBItem.GetItem(Data[i].Item.item_tid);

					if (itemData.ItemUseType == GameDB.E_ItemUseType.Enchant)
					{
						if(_enhance.EnhanceItemSlot.item_Tid != 0)
                        {
							var tableItemEnchantData = DBItem.GetEnchantData(_enhance.EnhanceItemSlot.item_Tid);
							var tableEquipItemData = DBItem.GetItem(_enhance.EnhanceItemSlot.item_Tid);

							if (tableItemEnchantData.EnchantType.HasFlag(E_EnchantType.NoUseItemEnchant))
								continue;

							if (tableEquipItemData.EnchantUseType.HasFlag(E_EnchantUseType.NormalEnchant) && tableItemEnchantData.NormalUseItemID != null && tableItemEnchantData.NormalUseItemID.Find(item => item == Data[i].Item.item_tid) != 0)
								continue;
							else if (tableEquipItemData.EnchantUseType.HasFlag(E_EnchantUseType.BlessEnchant) && tableItemEnchantData.BlessUseItemID != null && tableItemEnchantData.BlessUseItemID.Find(item => item == Data[i].Item.item_tid) != 0)
								continue;
							else if (tableEquipItemData.EnchantUseType.HasFlag(E_EnchantUseType.CurseEnchant) && tableItemEnchantData.CurseUseItemID != null && tableItemEnchantData.CurseUseItemID.Find(item => item == Data[i].Item.item_tid) != 0)
								continue;
							else
								itemList.Add(Data.List[i].Item);
						}
					}
					else
						itemList.Add(Data.List[i].Item);
				}

				break;

			case E_InvenSortType.EnhanceEquip:
				if (!UIManager.Instance.Find(out UIFrameItemEnhance _enhanceframe))
					return;

				for (int i = 0; i < Data.List.Count; i++)
				{
					var tableEquipItemData = DBItem.GetItem(Data[i].Item.item_tid);
					var tableItemEnchantData = DBItem.GetEnchantData(Data[i].Item.item_tid);

					if (_enhanceframe.MaterialSlot.item_Tid != 0 && tableEquipItemData != null && tableItemEnchantData != null && tableEquipItemData.EnchantUseType != E_EnchantUseType.None)
					{
						if (tableEquipItemData.EnchantUseType.HasFlag(E_EnchantUseType.NormalEnchant) && tableItemEnchantData.NormalUseItemID != null && tableItemEnchantData.NormalUseItemID.Find(item => item == _enhanceframe.MaterialSlot.item_Tid) != 0)
							continue;
						else if (tableEquipItemData.EnchantUseType.HasFlag(E_EnchantUseType.BlessEnchant) && tableItemEnchantData.BlessUseItemID != null && tableItemEnchantData.BlessUseItemID.Find(item => item == _enhanceframe.MaterialSlot.item_Tid) != 0)
							continue;
						else if (tableEquipItemData.EnchantUseType.HasFlag(E_EnchantUseType.CurseEnchant) && tableItemEnchantData.CurseUseItemID != null && tableItemEnchantData.CurseUseItemID.Find(item => item == _enhanceframe.MaterialSlot.item_Tid) != 0)
							continue;
						else
							itemList.Add(Data.List[i].Item);
					}
					else if(_enhanceframe.MaterialSlot.item_Tid == 0 && tableEquipItemData != null && tableItemEnchantData != null && tableEquipItemData.EnchantUseType != E_EnchantUseType.None)
                    {
						//[박윤성] 주문서를 선택안할때 가방에 표시할 아이템 목록
						bool IsAddList = false;

						//잠긴건 안보여줌
						if (Data.List[i].Item.IsLock)
							IsAddList = true;
						//장비가 아닌건 안보여줌
						if (!DBItem.IsEquipItem(Data.List[i].Item.item_tid))
							IsAddList = true;
						//다음 강화가 없으면 안보여줌
						if (!DBItem.IsExistStepUpId(Data.List[i].Item.item_tid))
							IsAddList = true;

						if (EnumHelper.CheckFlag(tableEquipItemData.EnchantUseType, E_EnchantUseType.NormalEnchant) == false &&
							EnumHelper.CheckFlag(tableEquipItemData.EnchantUseType, E_EnchantUseType.BlessEnchant) == false &&
							EnumHelper.CheckFlag(tableEquipItemData.EnchantUseType, E_EnchantUseType.CurseEnchant) == false)
							IsAddList = true;


						if(IsAddList)
							itemList.Add(Data.List[i].Item);

					}
					else
						itemList.Add(Data.List[i].Item);
				}

				break;

			case E_InvenSortType.Enchant:
				for (int i = 0; i < Data.List.Count; i++)
				{
					var itemData = DBItem.GetItem(Data[i].Item.item_tid);

					if (itemData.ItemUseType != E_ItemUseType.SmeltScroll)
						itemList.Add(Data.List[i].Item);
				}

				break;

			case E_InvenSortType.EnchantEquip:
				for (int i = 0; i < Data.List.Count; i++)
				{
					var itemData = DBItem.GetItem(Data[i].Item.item_tid);

					if (itemData.ItemUseType == GameDB.E_ItemUseType.Equip && itemData.SmeltScrollUseType == E_SmeltScrollUseType.SmeltScroll)
						continue;
					else
						itemList.Add(Data.List[i].Item);
				}

				break;
		}

        // 데이터 체크
        for (int j = 0; j < itemList.Count; j++)
        {
            var removeData = Data.List.Find(item => item.Item.item_id == itemList[j].item_id && item.Item.netType == itemList[j].netType);

            if (removeData != null)
                Data.List.Remove(removeData);
        }
    }

	private void SetExtensionData()
	{
		if (!UIManager.Instance.Find(out UIFrameInventory _inventory))
			return;

		int dataCount = Data.List.Count;
		// 빈 슬롯 여부 확인하고 추가
		if (Me.CurCharData.GetShowInvenItems().Count <= Me.CurCharData.InvenMaxCnt)
		{
			// 빈 슬롯 추가
			for (int i = 0; i < Me.CurCharData.InvenMaxCnt - dataCount; i++)
				Data.List.Add(new ScrollInvenData() { Item = null });
		}
		else
		{
			int addSlotCount = (int)(4 - ((dataCount - Me.CurCharData.InvenMaxCnt) % 4) == 4 ? 0 : 4 - ((dataCount - Me.CurCharData.InvenMaxCnt) % 4));
			
			for (int i = 0; i < addSlotCount; i++)
				Data.List.Add(new ScrollInvenData() { Item = null });
		}

		// 확장 슬롯 추가 여부 확인
		if (DBConfig.Max_Inventory > Me.CurCharData.InvenMaxCnt && _inventory.CurSortType == E_InvenSortType.All)
			Data.List.Add(new ScrollInvenData() { Item = null, type = ScrollInvenData.CellType.Expansion });
	}

	// to do : 추후 통합하거나 제거해야하는 함수
	public void RemoveData(ZItem _item)
	{
		var data = Data.List.Find(item => item.Item != null && item.Item.item_id == _item.item_id && item.Item.netType == NetItemType.TYPE_EQUIP);
		if (data != null)
		{
			data.Reset(new ScrollInvenData() { Item = null, isNew = false, isSelected = false, type = ScrollInvenData.CellType.Normal });

			SetData();

			UIManager.Instance.Find<UIFrameInventory>().RefreshInvenVolume();
		}
	}

	// to do : 추후 통합하거나 제거해야하는 함수
	public void RemoveData(List<ZItem> _item)
	{
		for (int i = 0; i < _item.Count; i++)
		{
			var data = Data.List.Find(item => item.Item != null && item.Item.item_id == _item[i].item_id && item.Item.netType == NetItemType.TYPE_EQUIP);
			if (data != null)
			{
				data.Reset(new ScrollInvenData() { Item = null });
			}
		}

		SetData();

		UIManager.Instance.Find<UIFrameInventory>().RefreshInvenVolume();
	}
}

///<summary>Scroll Item Define (Scroll 전용 사용자 정의 자료구조 선언)</summary>
public class ScrollInvenData
{
	public ZItem Item;

	public bool isSelected = false;
	public bool isNew = false;

	public CellType type = CellType.Normal;

	public enum CellType
	{
		Normal,
		Expansion
	}

	public void Reset(ScrollInvenData _data)
	{
		Item = _data.Item;
		isSelected = _data.isSelected;
		isNew = _data.isNew;
	}
}

[Serializable]
public class MyGridParams : GridParams
{
	[SerializeField] private GameObject _CellGroupPrefab = null;

	protected override GameObject CreateCellGroupPrefabGameObject()	{
		if (_CellGroupPrefab == null || !_CellGroupPrefab)
			return base.CreateCellGroupPrefabGameObject();

		return _CellGroupPrefab; 
	}
}

public class MyCellGroupViewsHolder : CellGroupViewsHolder<UIInvenViewsHolder>
{
	public UnityEngine.UI.ContentSizeFitter contentSizeFitterComponent;

	private ZButton ItemSlotInvenAdd = null;

	public override void CollectViews()
	{
		base.CollectViews();

		contentSizeFitterComponent = root.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();
		contentSizeFitterComponent.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

		contentSizeFitterComponent.enabled = true;

		root.GetComponentAtPath("ItemSlot_Inven_Add", out ItemSlotInvenAdd);
		ItemSlotInvenAdd.onClick.AddListener(SlotExpansion);
	}

	public void ShowHeader()
	{
		ItemSlotInvenAdd.gameObject.SetActive(true);
	}

	public void ClearHeader()
	{
		ItemSlotInvenAdd.gameObject.SetActive(false);
	}

	public void SlotExpansion()
	{
		if (Me.CurCharData.InvenMaxCnt >= DBConfig.Max_Inventory)
		{
			UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
			{
				_popup.Open(ZUIString.LOCALE_OK_BUTTON, DBLocale.GetText("인벤토리가 최대 확장된 상태입니다."),
				new string[] { ZUIString.LOCALE_OK_BUTTON },
				new Action[] { delegate { _popup.Close(); } });
			});
			return;
		}

		ZItem havItem = Me.CurCharData.GetInvenItemUsingMaterial(DBConfig.Inven_Slot_Plus_ID);
		if (havItem != null && havItem.cnt > 0)
		{
			UICommon.OpenCostConfirmPopup((UIPopupCostConfirm _popup) =>
			{
				_popup.Open(DBLocale.GetText("가방 확장"),
							DBLocale.GetText("가방 슬롯을 " + DBConfig.Expend_Inventory_Each.ToString() + "개 확장하시겠습니까?"),
							DBLocale.GetText("1"),
							DBLocale.GetText("item_scroll_change"),
							new string[] { ZUIString.LOCALE_CANCEL_BUTTON, ZUIString.LOCALE_OK_BUTTON },
							new Action[] {
								delegate
								{
									_popup.Close();
								},
								delegate
								{
									BuyInventorySlot(havItem);
									_popup.Close();
								}
							});
			});
			return;
		}

		if (ConditionHelper.CheckCompareCost(DBConfig.Diamond_ID, DBConfig.Expend_Inventory_Diamond) == false)
			return;

		UICommon.OpenCostConfirmPopup((UIPopupCostConfirm _popup) =>
		{
			_popup.Open(DBLocale.GetText("가방 확장"),
						DBLocale.GetText("가방 슬롯을 " + DBConfig.Expend_Inventory_Each.ToString() + "개 확장하시겠습니까?"),
						DBConfig.Expend_Inventory_Diamond.ToString(),
						DBLocale.GetText("item_gem"),
						new string[] { ZUIString.LOCALE_CANCEL_BUTTON, ZUIString.LOCALE_OK_BUTTON },
						new Action[] {
								delegate
								{
									_popup.Close();
								},
								delegate
								{
									BuyInventorySlot(null);
									_popup.Close();
								}
						});
		});
	}

	private void BuyInventorySlot(ZItem _currencyItem = null)
	{
		ulong id = _currencyItem == null ? 0 : _currencyItem.item_id;
		uint tid = _currencyItem == null ? DBConfig.Diamond_ID : _currencyItem.item_tid;

		ZWebManager.Instance.WebGame.BuyInventorySlot(id, tid, delegate
		{
			if (UIManager.Instance.Find(out UIFrameInventory _inventory))
			{
				_inventory.ScrollAdapter.SetData();
				_inventory.RefreshInvenVolume();
			}
		});
	}
}
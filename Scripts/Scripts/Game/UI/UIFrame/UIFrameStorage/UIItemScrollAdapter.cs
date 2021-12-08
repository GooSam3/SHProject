using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using Com.TheFallenGames.OSA.DataHelpers;
using frame8.Logic.Misc.Other.Extensions;
using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using ZDefine;
using ZNet.Data;

// 아이템 스크롤에 쓰일 어댑터
public class UIItemScrollAdapter : GridAdapter<MyGridParams, UIScrollItemSlot>
{
	public SimpleDataHelper<ScrollItemData> Data { get; private set; }

	private Action<ScrollItemData> onClickSlot;

	private bool isFixedList = false;

	// FixedList에서만 사용!
	// 마지막으로 갱신된 리스트의 갯수 캐싱
	// list overflow 조건용~~
	// todo_ljh : 변수명이 ~~count로 끝나야할듯
	public int LastIndex { get; private set; }

	private bool isUseAddSlot;

	private Action onAddedStorage;

	// 수동 init
	protected override void Start() { }

	protected override void UpdateCellViewsHolder(UIScrollItemSlot _holder)
	{
		if (_holder == null)
			return;

		ScrollItemData data = Data[_holder.ItemIndex];
		_holder.UpdateItemSlot(data, onClickSlot);
	}

	protected override CellGroupViewsHolder<UIScrollItemSlot> GetNewCellGroupViewsHolder()
	{
		if (isUseAddSlot == false)
			return base.GetNewCellGroupViewsHolder();


		return new StorageGroupViewsHolder(AddStorageSlot);
	}

	protected override void UpdateViewsHolder(CellGroupViewsHolder<UIScrollItemSlot> newOrRecycled)
	{
		base.UpdateViewsHolder(newOrRecycled);

		if (isUseAddSlot == false)
			return;

		if (newOrRecycled.NumActiveCells > 0)
		{
			var first = newOrRecycled.ContainingCellViewsHolders[0];

			var slot = newOrRecycled as StorageGroupViewsHolder;

			if (first.ItemIndex >= Me.CurUserData.GetStorageMaxCnt())
				slot.ShowHeader();
			else
				slot.ClearHeader();

		}

		ScheduleComputeVisibilityTwinPass();
	}

	public void RefreshData()
	{
		for (int i = 0; i < base.CellsCount; i++)
			UpdateCellViewsHolder(base.GetCellViewsHolder(i));
	}

	// 직관성을위해
	public void InitializeStorage(Action<ScrollItemData> _onClickSlot, Action _onAddedStorage, uint capacity = 0)
	{
		isUseAddSlot = true;
		onAddedStorage = _onAddedStorage;
		// +1 : 확장슬롯
		Initialize(_onClickSlot, capacity + 1);
	}

	// initialCount == 0 일시 일반 리스트
	// 0 이상일시 숫자 고정으로 동작(ex - 인벤토리 빈 슬롯 표현)
	public void Initialize(Action<ScrollItemData> _onClickSlot, uint capacity = 0)
	{
		if (Data == null)
		{
			Data = new SimpleDataHelper<ScrollItemData>(this);

			isFixedList = capacity > 0;
		}

		GameObject itemslot = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIItemSlot));
		Parameters.Grid.CellPrefab = itemslot.GetComponent<RectTransform>();
		Parameters.Grid.CellPrefab.SetParent(GetComponent<Transform>());
		Parameters.Grid.CellPrefab.transform.localScale = Vector2.one;
		Parameters.Grid.CellPrefab.transform.localPosition = new Vector3(0, 0, 0);
		Parameters.Grid.CellPrefab.gameObject.SetActive(false);

		onClickSlot = _onClickSlot;

		Init();

		if (isFixedList)
		{
			List<ScrollItemData> listData = new List<ScrollItemData>();
			for (int i = 0; i < capacity; i++)
			{
				listData.Add(new ScrollItemData());
			}

			Data.InsertItemsAtEnd(listData);
		}
	}

	public void SetAction(Action<ScrollItemData> _onClickSlot)
	{
		onClickSlot = _onClickSlot;
	}

	// 더 넣을수 있는상태입니까?
	public bool HasRemainCapacity()
	{
		if (isFixedList == false) return true;

		return Data.Count > LastIndex;
	}

	public void CastListType<T>() where T : ScrollItemData, new()
	{
		for (int i = 0; i < Data.Count; i++)
		{
			Data.List[i] = new T();
		}
	}

	public void ResetFixedListData(List<ScrollShopItemData> dataList)
	{
		LastIndex = dataList.Count;
		for (int i = 0; i < Data.Count; i++)
		{
			if (dataList.Count > i)
			{
				(Data[i] as ScrollShopItemData).Reset(dataList[i]);
			}
			else
			{
				(Data[i] as ScrollShopItemData).Reset();
			}
		}

		RefreshData();
	}

	public void ResetFixedListData(List<ScrollItemData> dataList)
	{
		LastIndex = dataList.Count;
		for (int i = 0; i < Data.Count; i++)
		{
			if (dataList.Count > i)
			{
				(Data[i]).Reset(dataList[i]);
			}
			else
			{
				(Data[i]).Reset();
			}
		}

		RefreshData();
	}


	public void ResetData(List<ScrollItemData> dataList)
	{
		if (isFixedList)
		{
			ResetFixedListData(dataList);
		}
		else
		{
			Data.ResetItems(dataList);
		}
	}

	private void AddStorageSlot(int added)
	{
		List<ScrollItemData> listData = new List<ScrollItemData>();
		for (int i = 0; i < added; i++)
		{
			listData.Add(new ScrollItemData());
		}

		Data.InsertItemsAtEnd(listData);
		onAddedStorage?.Invoke();
		SetNormalizedPosition(1);
		Refresh();
	}

	// 리스트를 합칠수 있는가?
	public bool CanMerge<T>(List<T> target, int originDataCount) where T : ScrollItemData
	{
		int addedValue = 0;

		foreach (var iter in target)
		{
			if (iter.Item == null) break;

			var findItem = Data.List.Find(item => (item.Item != null) && item.Item.item_id == iter.Item.item_id);

			if (findItem == null)// 없음, 새로 추가되는녀석
			{

				addedValue++;
				continue;
			}

			if (DBItem.GetItem(iter.Item.item_tid, out GameDB.Item_Table table) == false)
				continue;

			if (table.ItemStackType == GameDB.E_ItemStackType.AccountStack ||
				table.ItemStackType == GameDB.E_ItemStackType.Stack)
				continue;

			addedValue++;
		}
		return (originDataCount + addedValue) <= (Data.Count + (isUseAddSlot ? -1 : 0));
	}
}

public class ScrollItemData
{
	// 설계 미스,,
	public enum E_SlotType
	{
		Shop = 0, // 디폴트로 shop 사용
		View = 1
	}

	public E_SlotType slotType;

	public uint tid;
	public Item_Table table;

	public ZItem Item;
	public bool IsSelected = false;
	public ulong Count = 0;// 외부 노출용 수량, zitem의 수량과 다를수있음

	public bool IsInteractive = false; // 상호작용 여부
	public bool InteractiveValue = false; // 상호작용 값

	public bool isEmpty = true;// 비어있는가?

	// tid, cnt만 갖고 동작할때
	public ScrollItemData(uint _tid, uint _cnt)
	{
		Reset();
		slotType = E_SlotType.View;
		tid = _tid;
		table = DBItem.GetItem(tid);
		Count = _cnt;
	}

	public ScrollItemData() { slotType = E_SlotType.Shop; }

	public virtual void Reset(ScrollItemData data)
	{
		Item = data.Item;
		IsSelected = data.IsSelected;
		Count = data.Count;
		IsInteractive = data.IsInteractive;
		InteractiveValue = data.InteractiveValue;
		isEmpty = data.isEmpty;

		tid = data.tid;
		table = data.table;
	}

	public virtual void Reset()
	{
		Item = null;
		IsSelected = false;
		Count = 0;
		IsInteractive = false;
		InteractiveValue = false;
		isEmpty = true;

		tid = 0;
		table = null;
	}
}

public class StorageGroupViewsHolder : CellGroupViewsHolder<UIScrollItemSlot>
{
	private ZButton ItemSlotInvenAdd = null;

	private Action<int> onExpandSlot;

	public StorageGroupViewsHolder(Action<int> _onExpandSlot)
	{
		onExpandSlot = _onExpandSlot;
	}

	public override void CollectViews()
	{
		base.CollectViews();

		root.GetComponentAtPath("ItemSlot_Inven_Add", out ItemSlotInvenAdd);
		ItemSlotInvenAdd.onClick.AddListener(OnClickStorageExpansion);
	}
	public void ShowHeader()
	{
		ItemSlotInvenAdd.gameObject.SetActive(true);
	}

	public void ClearHeader()
	{
		ItemSlotInvenAdd.gameObject.SetActive(false);
	}
	public void OnClickStorageExpansion()
	{
		if (Me.CurUserData.GetStorageMaxCnt() >= DBConfig.Max_Storage_Count)
		{
			// 창고 최대강화상태
			//UIMessagePopup.ShowPopupOk()
			return;
		}

		var pastCnt = Me.CurUserData.GetStorageMaxCnt();

		// 창고슬롯 늘리실?
		UIMessagePopup.ShowCostPopup("Storage_Title_SlotUp", "Storage_Slot_Up_Message", DBConfig.Diamond_ID, DBConfig.Expend_Storage_Diamond, () =>
		{
			if (ConditionHelper.CheckCompareCost(DBConfig.Diamond_ID, DBConfig.Expend_Storage_Diamond) == false)
				return;

			ZWebManager.Instance.WebGame.REQ_BuyStorageSlot(0, DBConfig.Diamond_ID, delegate
			{
				int AddedCnt = (int)Me.CurUserData.GetStorageMaxCnt() - (int)pastCnt;
				onExpandSlot?.Invoke(AddedCnt);
			});
		});


	}
}
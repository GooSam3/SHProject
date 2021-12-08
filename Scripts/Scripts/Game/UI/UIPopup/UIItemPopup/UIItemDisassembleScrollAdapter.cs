
using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using Com.TheFallenGames.OSA.DataHelpers;
using System;
using System.Collections.Generic;
using UnityEngine;
using ZDefine;

public class UIItemDisassembleScrollAdapter : GridAdapter<GridParams, UIItemDisassembleHolder>
{
	// 스크롤 데이터 리스트
	public SimpleDataHelper<ScrollDisassembleData> Data
	{
		get; private set;
	}

	/// <summary>리스트 홀더를 갱신해준다.(스크롤이 Active 또는 Cell Group이 이동될 때(Drag))</summary>
	/// <param name="_holder">홀더 오브젝트</param>
	protected override void UpdateCellViewsHolder(UIItemDisassembleHolder _holder)
	{
		if (_holder == null)
			return;

		ScrollDisassembleData data = Data[_holder.ItemIndex];
		_holder.UpdateHolder(data);
	}

	/// <summary>홀더 데이터 리스트 전체 갱신</summary>
	public void RefreshData()
	{
		for (int i = 0; i < base.CellsCount; i++)
			UpdateCellViewsHolder(base.GetCellViewsHolder(i));
	}

	/// <summary>Adapter 초기 세팅 (최초 1회)</summary>
	public void Initialize()
	{
		if (Parameters.Grid.CellPrefab == null)
		{
			GameObject itemslot = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIItemDisassembleHolder));
			Parameters.Grid.CellPrefab = itemslot.GetComponent<RectTransform>();
			Parameters.Grid.CellPrefab.SetParent(GetComponent<Transform>());
			Parameters.Grid.CellPrefab.transform.localScale = Vector2.one;
			Parameters.Grid.CellPrefab.transform.localPosition = Vector3.zero;
			Parameters.Grid.CellPrefab.gameObject.SetActive(false);
			gameObject.SetActive(Parameters.Grid.CellPrefab != null);
		}

		Init();
	}

	/// <summary>스크롤 리스트 Setting</summary>
	/// <param name="_callback">스크롤 리스트 데이터 세팅 이후 처리할 콜백 실행</param>
	public void AddData(ZItem _item, Action _callback = null)
	{
		if (Data == null)
			Data = new SimpleDataHelper<ScrollDisassembleData>(this);

		#region 사용자 변경 로직
		if (Data.List.Find(item => item.Item.item_id == _item.item_id && item.Item.netType == NetItemType.TYPE_EQUIP) == null)
			Data.InsertOne(Data.Count, new ScrollDisassembleData() { Item = _item});
		#endregion

		// 데이터 갱신
		Data.NotifyListChangedExternally();

		_callback?.Invoke();
	}

	public void AddData(List<ZItem> _itemList, Action _callback = null)
	{
		if (Data == null)
			Data = new SimpleDataHelper<ScrollDisassembleData>(this);

		for (int i = 0; i < _itemList.Count; i++)
		{
			if (Data.List.Find(item => item.Item.item_id == _itemList[i].item_id && item.Item.netType == NetItemType.TYPE_EQUIP) == null)
				Data.InsertOne(Data.Count, new ScrollDisassembleData() { Item = _itemList[i] });
		}

		// 데이터 갱신
		Data.NotifyListChangedExternally();

		_callback?.Invoke();
	}

	public ulong GetTotalCost()
	{
		ulong cost = 0;
		for (int i = 0; i < Data.List.Count; i++)
		{
			var table = DBItem.GetItem(Data.List[i].Item.item_tid);
			cost += table.BreakUseCount; 
		}
		return cost;
	}

	/// <summary>데이터 삭제</summary>
	public void RemoveData(ZItem _item)
    {
		var data = Data.List.Find(item => item.Item.item_id == _item.item_id && item.Item.netType == NetItemType.TYPE_EQUIP);
		if (data != null)
        {
			Data.List.Remove(data);

			// 데이터 갱신
			Data.NotifyListChangedExternally();
		}
	}

	/// <summary>데이터 전체 삭제</summary>
	public void ClearData()
    {
		Data.List.Clear();

		// 데이터 갱신
		Data.NotifyListChangedExternally();
	}
}

///<summary>Scroll Item Define (Scroll 전용 사용자 정의 자료구조 선언)</summary>
public class ScrollDisassembleData
{
	public ZItem Item = null;

	public void Reset(ScrollDisassembleData _data)
	{
		Item = _data.Item;
	}
}
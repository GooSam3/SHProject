using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using ZDefine;
using ZNet.Data;

public class UIItemCollectionScrollAdapter : OSA<BaseParamsWithPrefab, UIItemCollectionViewsHolder>
{
	public SimpleDataHelper<ScrollItemCollectionData> Data
	{
		get; private set;
	}

	protected override UIItemCollectionViewsHolder CreateViewsHolder(int itemIndex)
	{
		var instance = new UIItemCollectionViewsHolder();
		instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
		return instance;
	}

	protected override void UpdateViewsHolder(UIItemCollectionViewsHolder newOrRecycled)
	{
		ScrollItemCollectionData model = Data[newOrRecycled.ItemIndex];
		newOrRecycled.UpdateViews(model);
	}

	public void RefreshData()
	{
		for (int i = 0; i < base.GetItemsCount(); i++)
		{
			if (GetItemViewsHolder(i) != null)
				UpdateViewsHolder(GetItemViewsHolder(i));
		}
	}

	public void Initialize()
	{
		if (Parameters.ItemPrefab == null)
		{
			GameObject CollectionHodler = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIItemCollectionViewsHolder));
			Parameters.ItemPrefab = CollectionHodler.GetComponent<RectTransform>();
			Parameters.ItemPrefab.SetParent(GetComponent<Transform>());
			Parameters.ItemPrefab.transform.localScale = Vector2.one;
			Parameters.ItemPrefab.transform.localPosition = Vector3.zero;
			Parameters.ItemPrefab.gameObject.SetActive(false);
			gameObject.SetActive(Parameters.ItemPrefab != null);
		}
		Init();
	}

	public List<ScrollItemCollectionData> HoleDataList = new List<ScrollItemCollectionData>();

	public void SetScrollData(List<ItemCollection_Table> _dataList, Action _callback = null)
	{
		if (Data == null)
			Data = new SimpleDataHelper<ScrollItemCollectionData>(this);

		HoleDataList.Clear();
		for (int i = 0; i < _dataList.Count; i++)
		{
			HoleDataList.Add(new ScrollItemCollectionData() { ItemCollection = _dataList[i] });
		}

		DataPageControll();

		_callback?.Invoke();
	}

	public void DataPageControll(int _count = 1)
	{
		Data.List.Clear();
		int count = _count;
		int maxCount = ((count - 1) * 30) + 30;
		if (maxCount > HoleDataList.Count)
			maxCount = HoleDataList.Count;

		List<ScrollItemCollectionData> list = new List<ScrollItemCollectionData>();		

		for (int i = (count - 1) * 30; i < maxCount; i++)
		{
			list.Add(HoleDataList[i]);
		}

		Data.InsertItemsAtEnd(list);

		Data.NotifyListChangedExternally();
		RefreshData();
	}

	public virtual void ResetListData(List<ScrollItemCollectionData> _listData)
	{
		if (IsInitialized)
		{
			Data.ResetItems(_listData);
			SetNormalizedPosition(1);
		}
		else
			Initialized += () => ResetListData(_listData);
	}
}

public class ScrollItemCollectionData
{
	public ItemCollection_Table ItemCollection;

	public void Reset(ItemCollection_Table _itemCollection)
	{
		ItemCollection = _itemCollection;
	}
}

using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///  ** 제약사항 **
///  AddressableKey 의 이름을 갖는 프리팹이 스폰되있어야함
/// </summary>
/// <typeparam name="T"></typeparam>

public abstract class ZAdapterHolderBase<T> : BaseItemViewsHolder
{
	public abstract void SetSlot(T data);
}

// horizontal or vertical 용
// T : Data
public abstract class ZScrollAdapterBase<DATATYPE, HOLDER> : OSA<BaseParamsWithPrefab, HOLDER> where HOLDER : ZAdapterHolderBase<DATATYPE>, new()
{
	public SimpleDataHelper<DATATYPE> Data { get; private set; }

	// 어드레서블 키
	protected abstract string AddressableKey { get; }

	// 초기화 될때 호출
	protected virtual void OnInitialize() { }

	// 홀더 생성될때 호출
	protected virtual void OnCreateHolder(HOLDER holder) { }

	// 홀더 업데이트 될때 호출
	protected virtual void OnUpdateViewHolder(HOLDER holder) { }

	// 데이터 갱신될때 호출(리셋)
	protected virtual void OnResetData(List<DATATYPE> _listData) { }

	[Header("PREFAB SIZE CHANGE / 0 is defualt"), SerializeField]
	private int PrefabSize = 0;

	protected override void Start() { }

	protected override HOLDER CreateViewsHolder(int itemIndex)
	{
		var instance = new HOLDER();

		instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);

		OnCreateHolder(instance);

		return instance;
	}

	protected override void UpdateViewsHolder(HOLDER newOrRecycled)
	{
		DATATYPE data = Data[newOrRecycled.ItemIndex];
		newOrRecycled.SetSlot(data);

		OnUpdateViewHolder(newOrRecycled);
	}

	public virtual void RefreshData()
	{
		for (int i = 0; i < VisibleItemsCount; i++)
		{
			var holder = base.GetItemViewsHolder(i);
			UpdateViewsHolder(holder);
		}
	}


	public virtual void Initialize()
	{
		Data = new SimpleDataHelper<DATATYPE>(this);

		GameObject obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, AddressableKey);

		var holderPrefab = obj.GetComponent<RectTransform>();

		if (PrefabSize > 0)
		{
			var originSize = holderPrefab.sizeDelta;

			originSize.y = PrefabSize;

			holderPrefab.sizeDelta = originSize;
		}
		Parameters.ItemPrefab = holderPrefab;

		Parameters.ItemPrefab.SetParent(transform);
		Parameters.ItemPrefab.transform.localScale = Vector2.one;
		Parameters.ItemPrefab.transform.localPosition = Vector3.zero;
		Parameters.ItemPrefab.gameObject.SetActive(false);

		OnInitialize();

		Init();
	}

	public virtual void ResetListData(List<DATATYPE> listData, int normalPos = 1)
	{
		if (IsInitialized)
		{
			Data.ResetItems(listData);
			OnResetData(listData);

			SetNormalizedPosition(normalPos);
		}
		else
			Initialized += () => ResetListData(listData);
	}
}

public abstract class ZFittedAdapterData
{
	// 해당값은 생성자, Reset등 데이터가 갱신되는부분에서 base.Reset()함수로 갱신해줘야 합니다.
	public bool HasPendingSizeChange { get; set; }

	protected void Reset()
	{
		HasPendingSizeChange = true;
	}
}

public abstract class ZFittedAdapterHolderBase<DATATYPE> : BaseItemViewsHolder where DATATYPE : ZFittedAdapterData
{
	public ContentSizeFitter CSF { get; set; }

	public abstract void SetSlot(DATATYPE data);

	public override void CollectViews()
	{
		CSF = root.GetComponent<ContentSizeFitter>();
		CSF.enabled = false;

		base.CollectViews();
	}

	public override void MarkForRebuild()
	{
		base.MarkForRebuild();

		if (CSF)
			CSF.enabled = true;
	}

	public override void UnmarkForRebuild()
	{
		if (CSF)
			CSF.enabled = false;

		base.UnmarkForRebuild();
	}
}

// 가변적으로 늘어나는 리스트 어댑터
public abstract class ZFittedScrollerAdapterBase<DATATYPE, HOLDER> : OSA<BaseParamsWithPrefab, HOLDER> where HOLDER : ZFittedAdapterHolderBase<DATATYPE>, new()
																									   where DATATYPE : ZFittedAdapterData
{
	public LazyDataHelper<DATATYPE> Data { get; private set; }
	protected List<DATATYPE> listData = new List<DATATYPE>();

	// 어드레서블 키
	protected abstract string AddressableKey { get; }

	// 초기화 될때 호출
	protected virtual void OnInitialize() { }

	// 홀더 생성될때 호출
	protected virtual void OnCreateHolder(HOLDER holder) { }

	// 홀더 업데이트 될때 호출
	protected virtual void OnUpdateViewHolder(HOLDER holder) { }

	// 데이터 갱신될때 호출(리셋)
	protected virtual void OnResetData(List<DATATYPE> _listData) { }

	protected override void Start() { }

	protected override HOLDER CreateViewsHolder(int itemIndex)
	{
		var instance = new HOLDER();
		instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);

		OnCreateHolder(instance);

		return instance;
	}

	public void RefreshData(bool refreshSize = false)
	{
		for (int i = 0; i < VisibleItemsCount; i++)
		{
			var holder = base.GetItemViewsHolder(i);
			UpdateViewsHolder(holder);
			OnUpdateViewHolder(holder);

			if (refreshSize)
				base.ForceRebuildViewsHolderAndUpdateSize(holder);
		}
	}

	protected override void OnItemHeightChangedPreTwinPass(HOLDER viewsHolder)
	{
		base.OnItemHeightChangedPreTwinPass(viewsHolder);
		var m = Data.GetOrCreate(viewsHolder.ItemIndex);
		m.HasPendingSizeChange = false;
	}

	protected override void UpdateViewsHolder(HOLDER newOrRecycled)
	{
		DATATYPE data = Data.GetOrCreate(newOrRecycled.ItemIndex);
		newOrRecycled.SetSlot(data);

		if (data.HasPendingSizeChange)
		{
			ScheduleComputeVisibilityTwinPass(true);
		}
	}

	protected override void OnItemIndexChangedDueInsertOrRemove(
		HOLDER shiftedViewsHolder, int oldIndex, bool wasInsert, int removeOrInsertIndex)
	{
		base.OnItemIndexChangedDueInsertOrRemove(shiftedViewsHolder, oldIndex, wasInsert, removeOrInsertIndex);

		shiftedViewsHolder.SetSlot(Data.GetOrCreate(shiftedViewsHolder.ItemIndex));
		ScheduleComputeVisibilityTwinPass(true);
	}

	protected override void RebuildLayoutDueToScrollViewSizeChange()
	{
		foreach (var data in Data.GetEnumerableForExistingItems())
			data.HasPendingSizeChange = true;

		base.RebuildLayoutDueToScrollViewSizeChange();
	}

	public void Initialize()
	{
		Data = new LazyDataHelper<DATATYPE>(this, CreateModel);

		GameObject obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, AddressableKey);

		Parameters.ItemPrefab = obj.GetComponent<RectTransform>();

		Parameters.ItemPrefab.SetParent(transform);
		Parameters.ItemPrefab.transform.localScale = Vector2.one;
		Parameters.ItemPrefab.transform.localPosition = Vector3.zero;
		Parameters.ItemPrefab.gameObject.SetActive(false);

		OnInitialize();

		Init();
	}

	/// <summary>
	/// 해당하는 인덱스의 데이터를 생성 후 반환
	/// 원본의 인덱스보다 itemIndex가 크다면 에러나니 조심
	/// </summary>
	public abstract DATATYPE CreateModel(int itemIndex);
	/* EX
        return new DATATYPE(BaseData[itemIndex]);
     */

	// fittedAdapter의 데이터는 동적으로 생성 후(CreateModel) 가져옴
	public virtual void ResetListData(List<DATATYPE> _listData)
	{
		if (IsInitialized)
		{
			OnResetData(listData);

			listData = _listData;
			Data.ResetItems(listData.Count);
			SetNormalizedPosition(1);
		}
		else
			Initialized += () => ResetListData(listData);
	}
}

public abstract class ZGridAdapterHolderBase<T> : CellViewsHolder
{
	public abstract void SetSlot(T data);
}

public abstract class ZGridScrollAdapter<DATATYPE, HOLDER> : GridAdapter<GridParams, HOLDER> where HOLDER : ZGridAdapterHolderBase<DATATYPE>, new()
{
	public SimpleDataHelper<DATATYPE> Data { get; private set; }

	// 어드레서블 키
	public abstract string AddressableKey { get; }

	// 초기화 될때 호출
	protected virtual void OnInitialize() { }

	// 홀더 생성될때 호출
	protected virtual void OnCreateHolder(HOLDER holder) { }

	// 홀더 업데이트 될때 호출
	protected virtual void OnUpdateViewHolder(HOLDER holder) { }

	// 데이터 갱신될때 호출(리셋)
	protected virtual void OnResetData(List<DATATYPE> _listData) { }

	protected override void Start() { }

	protected override void OnCellViewsHolderCreated(HOLDER cellVH, CellGroupViewsHolder<HOLDER> cellGroup)
	{
		base.OnCellViewsHolderCreated(cellVH, cellGroup);
		OnCreateHolder(cellVH);
	}

	protected override void UpdateCellViewsHolder(HOLDER viewsHolder)
	{
		DATATYPE data = Data[viewsHolder.ItemIndex];
		viewsHolder.SetSlot(data);
		OnUpdateViewHolder(viewsHolder);
	}

	public virtual void RefreshData()
	{
		for (int i = 0; i < base.GetNumVisibleCells(); i++)
			UpdateCellViewsHolder(base.GetCellViewsHolder(i));
	}

	public virtual void Initialize()
	{
		Data = new SimpleDataHelper<DATATYPE>(this);

		GameObject obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, AddressableKey);

		Parameters.Grid.CellPrefab = obj.GetComponent<RectTransform>();

		Parameters.Grid.CellPrefab.SetParent(transform);
		Parameters.Grid.CellPrefab.transform.localScale = Vector2.one;
		Parameters.Grid.CellPrefab.transform.localPosition = Vector3.zero;
		Parameters.Grid.CellPrefab.gameObject.SetActive(false);

		OnInitialize();

		Init();
	}

	public virtual void ResetListData(List<DATATYPE> listData)
	{
		if (IsInitialized)
		{
			Data.ResetItems(listData);
			OnResetData(listData);
			SetNormalizedPosition(1);
		}
		else
			Initialized += () => ResetListData(listData);
	}
}
